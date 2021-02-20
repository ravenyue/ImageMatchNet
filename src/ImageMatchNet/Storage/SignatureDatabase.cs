using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Storage
{
    public abstract class SignatureDatabase
    {
        private readonly int _wordWidth;
        private readonly int _wordNumber;
        private readonly ImageSignature _generator;

        public SignatureDatabase()
            : this(16, 63, new SignatureOptions())
        {

        }
        public SignatureDatabase(int wordWidth, int wordNumber, SignatureOptions options)
        {
            _wordWidth = wordWidth;
            _wordNumber = wordNumber;
            _generator = new ImageSignature(options);
        }

        public abstract List<MatchedRecord> SearchSignature(SignatureData data);
        public abstract Task<List<MatchedRecord>> SearchSignatureAsync(SignatureData data);

        public abstract void InsertSignature(SignatureData data);
        public abstract Task InsertSignatureAsync(SignatureData data);

        public void AddImage(string storagePath, string imagePath, Dictionary<string, string> metadata = null)
        {
            using (var image = File.OpenRead(imagePath))
            {
                AddImage(storagePath, image, metadata);
            }
        }

        public void AddImage(string storagePath, Stream image, Dictionary<string, string> metadata = null)
        {
            var signData = MakeSignatureData(storagePath, image, _generator, _wordWidth, _wordNumber, metadata);
            InsertSignature(signData);
        }

        public async Task AddImageAsync(string storagePath, string imagePath, Dictionary<string, string> metadata)
        {
            using (var image = File.OpenRead(imagePath))
            {
                await AddImageAsync(storagePath, image, metadata);
            }
        }

        public Task AddImageAsync(string storagePath, Stream image, Dictionary<string, string> metadata)
        {
            var signData = MakeSignatureData(storagePath, image, _generator, _wordWidth, _wordNumber, metadata);
            return InsertSignatureAsync(signData);
        }

        public List<MatchedRecord> SearchImage(string imagePath, bool allOrientations)
        {
            using (var image = File.OpenRead(imagePath))
            {
                return SearchImage(image, allOrientations);
            }
        }

        public List<MatchedRecord> SearchImage(Stream image, bool allOrientations)
        {
            var signData = MakeSignatureData(string.Empty, image, _generator, _wordWidth, _wordNumber);
            return SearchSignature(signData);
        }

        public async Task<List<MatchedRecord>> SearchImageAsync(string imagePath, bool allOrientations)
        {
            using (var image = File.OpenRead(imagePath))
            {
                var response = await SearchImageAsync(image, allOrientations);
                return response;
            }
        }

        public Task<List<MatchedRecord>> SearchImageAsync(Stream image, bool allOrientations)
        {
            var signData = MakeSignatureData(string.Empty, image, _generator, _wordWidth, _wordNumber);
            return SearchSignatureAsync(signData);
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

        public SignatureData MakeSignatureData(string storagePath, Stream image, ImageSignature generator, int wordWidth, int wordNumber, Dictionary<string, string> metadata = null)
        {
            var sign = generator.GenerateSignature(image);

            var data = new SignatureData
            {
                Path = storagePath,
                Signature = sign.ToArray(),
                Metadata = metadata,
                SimpleWord = new Dictionary<string, int>()
            };

            var words = MakeSimpleWords(sign, wordWidth, wordNumber);

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
