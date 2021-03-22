using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Storage
{
    public abstract class SignatureStorageBase : ISignatureStorage
    {
        public const int DefaultWordWidth = 16;
        public const int DefaultWordNumber = 63;

        private readonly int _wordWidth;
        private readonly int _wordNumber;
        protected readonly ImageSignature Generator;

        public SignatureStorageBase()
            : this(DefaultWordWidth, DefaultWordNumber, new SignatureOptions())
        { }

        public SignatureStorageBase(SignatureOptions options)
            : this(DefaultWordWidth, DefaultWordNumber, options)
        { }

        public SignatureStorageBase(int wordWidth, int wordNumber, SignatureOptions options)
        {
            _wordWidth = wordWidth;
            _wordNumber = wordNumber;
            Generator = new ImageSignature(options);
        }

        public abstract void InsertSignature(SignatureData data);
        public abstract Task InsertSignatureAsync(SignatureData data);

        public abstract List<MatchedRecord> SearchSignature(SignatureData data);
        public abstract Task<List<MatchedRecord>> SearchSignatureAsync(SignatureData data);

        public abstract void DeleteImage(string key);
        public abstract Task DeleteImageAsync(string key);

        public int[] AddImage(string key, string filePath, Dictionary<string, string> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            using (var image = File.OpenRead(filePath))
            {
                return AddImage(key, image, metadata);
            }
        }

        public int[] AddImage(string key, Stream stream, Dictionary<string, string> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var signData = MakeSignatureData(key, stream, metadata);
            InsertSignature(signData);
            return signData.Signature;
        }

        public async Task<int[]> AddImageAsync(string key, string filePath, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            using (var image = File.OpenRead(filePath))
            {
                return await AddImageAsync(key, image, metadata);
            }
        }

        public async Task<int[]> AddImageAsync(string key, Stream stream, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var signData = MakeSignatureData(key, stream, metadata);
            await InsertSignatureAsync(signData);

            return signData.Signature;
        }

        public List<MatchedRecord> SearchImage(int[] sign)
        {
            var signData = MakeSignatureData(string.Empty, sign);
            return SearchSignature(signData);
        }

        public Task<List<MatchedRecord>> SearchImageAsync(int[] sign)
        {
            var signData = MakeSignatureData(string.Empty, sign);
            return SearchSignatureAsync(signData);
        }

        public List<MatchedRecord> SearchImage(string filePath, bool allOrientations)
        {
            using (var image = File.OpenRead(filePath))
            {
                return SearchImage(image, allOrientations);
            }
        }

        public List<MatchedRecord> SearchImage(Stream stream, bool allOrientations)
        {
            var result = new List<MatchedRecord>();
            if (allOrientations)
            {
                var signs = Generator.GenerateAllOrientationsSignature(stream);

                foreach (var sign in signs)
                {
                    var signData = MakeSignatureData(string.Empty, sign);
                    var records = SearchSignature(signData);
                    result.AddRange(records);
                }
            }
            else
            {
                var signData = MakeSignatureData(string.Empty, stream);
                result = SearchSignature(signData);
            }

            return result
                .Where(x => x.Dist < SignatureComparison.DefaultMatchThreshold)
                .Distinct(new MatchedRecordEqualityComparer())
                .ToList();
        }

        public async Task<List<MatchedRecord>> SearchImageAsync(string filePath, bool allOrientations)
        {
            using (var image = File.OpenRead(filePath))
            {
                return await SearchImageAsync(image, allOrientations);
            }
        }

        public async Task<List<MatchedRecord>> SearchImageAsync(Stream stream, bool allOrientations)
        {
            var result = new List<MatchedRecord>();
            if (allOrientations)
            {
                var signs = Generator.GenerateAllOrientationsSignature(stream);

                foreach (var sign in signs)
                {
                    var signData = MakeSignatureData(string.Empty, sign);
                    var records = await SearchSignatureAsync(signData);
                    result.AddRange(records);
                }

            }
            else
            {
                var signData = MakeSignatureData(string.Empty, stream);
                result = await SearchSignatureAsync(signData);
            }

            return result
                .Where(x => x.Dist < SignatureComparison.DefaultMatchThreshold)
                .Distinct(new MatchedRecordEqualityComparer())
                .ToList();
        }

        public double[] NormalizedDistance(int[] sign, int[][] targetArray)
        {
            var dists = new double[targetArray.Length];

            for (int i = 0; i < targetArray.Length; i++)
            {
                dists[i] = SignatureComparison.NormalizedDistance(sign, targetArray[i]);
            }

            return dists;
        }

        public SignatureData MakeSignatureData(string key, Stream stream, Dictionary<string, string> metadata = null)
        {
            var sign = Generator.GenerateSignature(stream);

            return MakeSignatureData(key, sign, metadata);

        }

        public SignatureData MakeSignatureData(string key, int[] sign, Dictionary<string, string> metadata = null)
        {
            var data = new SignatureData
            {
                Key = key,
                Signature = sign,
                Metadata = metadata,
                SimpleWord = new Dictionary<string, int>()
            };

            var words = MakeSimpleWords(sign, _wordWidth, _wordNumber);

            for (int i = 0; i < words.Length; i++)
            {
                data.SimpleWord[$"simple_word_{i}"] = words[i];
            }

            return data;
        }

        public int[] MakeSimpleWords(ReadOnlySpan<int> sources, int wordWidth, int wordNumber)
        {
            var words = GetWords(sources, wordWidth, wordNumber);

            MaxContrast(words);

            var wordList = WordsToInt(words);

            return wordList;
        }

        public int[][] GetWords(ReadOnlySpan<int> sign, int wordWidth, int wordNumber)
        {
            // 生成每个单词的起始位置
            var wordPositions = Extensions.LinspaceInt(0, sign.Length, wordNumber, false);

            // 检查输入是否有意义
            if (wordWidth > sign.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(wordWidth), "Word length cannot be longer than array length");
            }
            if (wordPositions.Length > sign.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(wordNumber), "Number of words cannot be more than array length");
            }

            var words = new int[wordNumber][];

            // [0, 1, 2, 0, -1, -2, 0, 1]
            // [0, 1, 2]
            // [2, 0, -1]
            // [-1, -2, 0]
            // [0, 1]
            for (int i = 0; i < wordPositions.Length; i++)
            {
                var pos = wordPositions[i];
                if (pos + wordWidth <= sign.Length)
                {
                    words[i] = sign.Slice(pos, wordWidth).ToArray();
                }
                else
                {
                    words[i] = new int[wordWidth];
                    for (int j = 0; j < sign.Slice(pos).Length; j++)
                    {
                        words[i][j] = sign.Slice(pos)[j];
                    }
                }
            }

            return words;
        }

        public int[][] MaxContrast(int[][] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[j].Length; j++)
                {
                    if (array[i][j] > 0)
                        array[i][j] = 1;
                    else if (array[i][j] < 0)
                        array[i][j] = -1;
                }
            }

            return array;
        }

        public int[] WordsToInt(int[][] wordArray)
        {
            var words = new int[wordArray.Length];

            var width = wordArray[0].Length;

            int[] codingVector = Extensions.ArrayPower(3, Extensions.ArrayRange(width));

            for (int i = 0; i < wordArray.Length; i++)
            {
                var word = wordArray[i];
                words[i] = word.AddNum(1).DotProduct(codingVector);
            }

            return words;
        }
    }
}
