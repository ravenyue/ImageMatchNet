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

        internal readonly static object EmptyMetadata = new object();

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

        public abstract void InsertOrUpdateSignature<TMetadata>(SignatureData<TMetadata> data) where TMetadata : class;
        public abstract Task InsertOrUpdateSignatureAsync<TMetadata>(SignatureData<TMetadata> data) where TMetadata : class;

        public abstract List<MatchedRecord<TMetadata>> SearchSignature<TMetadata>(SignatureData data) where TMetadata : class;
        public abstract Task<List<MatchedRecord<TMetadata>>> SearchSignatureAsync<TMetadata>(SignatureData data) where TMetadata : class;

        public abstract void DeleteImage(string key);
        public abstract Task DeleteImageAsync(string key);

        public int[] AddOrUpdateImage<TMetadata>(string key, Stream stream, TMetadata metadata)
            where TMetadata : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            var signData = MakeSignatureData(key, stream, metadata);
            InsertOrUpdateSignature(signData);
            return signData.Signature;
        }

        public async Task<int[]> AddOrUpdateImageAsync<TMetadata>(string key, Stream stream, TMetadata metadata)
            where TMetadata : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            var signData = MakeSignatureData(key, stream, metadata);
            await InsertOrUpdateSignatureAsync(signData);

            return signData.Signature;
        }

        public List<MatchedRecord<TMetadata>> SearchImage<TMetadata>(int[] sign)
            where TMetadata : class
        {
            var signData = MakeSignatureData(string.Empty, sign);
            return SearchSignature<TMetadata>(signData);
        }

        public Task<List<MatchedRecord<TMetadata>>> SearchImageAsync<TMetadata>(int[] sign)
            where TMetadata : class
        {
            var signData = MakeSignatureData(string.Empty, sign);
            return SearchSignatureAsync<TMetadata>(signData);
        }

        public List<MatchedRecord<TMetadata>> SearchImage<TMetadata>(Stream stream, bool allOrientations = true)
            where TMetadata : class
        {
            var result = new List<MatchedRecord<TMetadata>>();
            if (allOrientations)
            {
                var signs = Generator.GenerateAllOrientationsSignature(stream);

                foreach (var sign in signs)
                {
                    var signData = MakeSignatureData(string.Empty, sign);
                    var records = SearchSignature<TMetadata>(signData);
                    result.AddRange(records);
                }
            }
            else
            {
                var signData = MakeSignatureData(string.Empty, stream);
                result = SearchSignature<TMetadata>(signData);
            }

            IEqualityComparer<MatchedRecord<TMetadata>> comparer = new MatchedRecordEqualityComparer();
            return result
                .Where(x => x.Dist < SignatureComparison.DefaultMatchThreshold)
                .Distinct(comparer)
                .ToList();
        }

        public async Task<List<MatchedRecord<TMetadata>>> SearchImageAsync<TMetadata>(Stream stream, bool allOrientations = true)
            where TMetadata : class
        {
            var result = new List<MatchedRecord<TMetadata>>();
            if (allOrientations)
            {
                var signs = Generator.GenerateAllOrientationsSignature(stream);

                foreach (var sign in signs)
                {
                    var signData = MakeSignatureData(string.Empty, sign);
                    var records = await SearchSignatureAsync<TMetadata>(signData);
                    result.AddRange(records);
                }
            }
            else
            {
                var signData = MakeSignatureData(string.Empty, stream);
                result = await SearchSignatureAsync<TMetadata>(signData);
            }
            IEqualityComparer<MatchedRecord<TMetadata>> comparer = new MatchedRecordEqualityComparer();
            return result
                .Where(x => x.Dist < SignatureComparison.DefaultMatchThreshold)
                .Distinct(comparer)
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

        public SignatureData MakeSignatureData(string key, int[] sign)
        {
            return MakeSignatureData(key, sign, EmptyMetadata);
        }

        public SignatureData MakeSignatureData(string key, Stream stream)
        {
            return MakeSignatureData(key, stream, EmptyMetadata);
        }

        public SignatureData<TMetadata> MakeSignatureData<TMetadata>(string key, Stream stream, TMetadata metadata = null)
            where TMetadata : class
        {
            var sign = Generator.GenerateSignature(stream);

            return MakeSignatureData(key, sign, metadata);
        }

        public SignatureData<TMetadata> MakeSignatureData<TMetadata>(string key, int[] sign, TMetadata metadata)
            where TMetadata : class
        {
            var data = new SignatureData<TMetadata>
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
