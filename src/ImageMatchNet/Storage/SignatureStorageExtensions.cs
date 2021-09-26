using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Storage
{
    public static class SignatureStorageExtensions
    {
        public static int[] AddOrUpdateImage(
            this ISignatureStorage storage, string key, string filePath)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            using (var image = File.OpenRead(filePath))
            {
                return storage.AddOrUpdateImage(key, image, SignatureStorageBase.EmptyMetadata);
            }
        }

        public async static Task<int[]> AddOrUpdateImageAsync(
            this ISignatureStorage storage, string key, string filePath)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            using (var image = File.OpenRead(filePath))
            {
                return await storage.AddOrUpdateImageAsync(key, image, SignatureStorageBase.EmptyMetadata).ConfigureAwait(false);
            }
        }

        public static int[] AddOrUpdateImage(this ISignatureStorage storage, string key, Stream stream)
        {
            return storage.AddOrUpdateImage(key, stream, SignatureStorageBase.EmptyMetadata);
        }

        public static Task<int[]> AddOrUpdateImageAsync(this ISignatureStorage storage, string key, Stream stream)
        {
            return storage.AddOrUpdateImageAsync(key, stream, SignatureStorageBase.EmptyMetadata);
        }

        public static int[] AddOrUpdateImage<TMetadata>(
            this ISignatureStorage storage, string key, string filePath, TMetadata metadata)
            where TMetadata : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            using (var image = File.OpenRead(filePath))
            {
                return storage.AddOrUpdateImage(key, image, metadata);
            }
        }

        public async static Task<int[]> AddOrUpdateImageAsync<TMetadata>(
            this ISignatureStorage storage, string key, string filePath, TMetadata metadata)
            where TMetadata : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (metadata is null) throw new ArgumentNullException(nameof(metadata));

            using (var image = File.OpenRead(filePath))
            {
                return await storage.AddOrUpdateImageAsync(key, image, metadata).ConfigureAwait(false);
            }
        }

        public static List<MatchedRecord<TMetadata>> SearchImage<TMetadata>(
            this ISignatureStorage storage, string filePath, bool allOrientations = true)
            where TMetadata : class
        {
            using (var image = File.OpenRead(filePath))
            {
                return storage.SearchImage<TMetadata>(image, allOrientations);
            }
        }

        public async static Task<List<MatchedRecord<TMetadata>>> SearchImageAsync<TMetadata>(
            this ISignatureStorage storage, string filePath, bool allOrientations = true)
            where TMetadata : class
        {
            using (var image = File.OpenRead(filePath))
            {
                return await storage.SearchImageAsync<TMetadata>(image, allOrientations).ConfigureAwait(false);
            }
        }

        public static List<MatchedRecord> SearchImage(
            this ISignatureStorage storage, int[] sign)
        {
            var records = storage.SearchImage<object>(sign);
            return records.Select(x => (MatchedRecord)x).ToList();
        }

        public static List<MatchedRecord> SearchImage(
            this ISignatureStorage storage, string filePath, bool allOrientations = true)
        {
            using (var image = File.OpenRead(filePath))
            {
                return storage.SearchImage(image, allOrientations);
            }
        }

        public static List<MatchedRecord> SearchImage(
            this ISignatureStorage storage, Stream stream, bool allOrientations = true)
        {
            var records = storage.SearchImage<object>(stream, allOrientations);
            return records.Select(x => (MatchedRecord)x).ToList();
        }

        public async static Task<List<MatchedRecord>> SearchImageAsync(
            this ISignatureStorage storage, int[] sign)
        {
            var records = await storage.SearchImageAsync<object>(sign).ConfigureAwait(false);
            return records.Select(x => (MatchedRecord)x).ToList();
        }

        public async static Task<List<MatchedRecord>> SearchImageAsync(
            this ISignatureStorage storage, string filePath, bool allOrientations = true)
        {
            using (var image = File.OpenRead(filePath))
            {
                return await storage.SearchImageAsync(image, allOrientations).ConfigureAwait(false);
            }
        }

        public async static Task<List<MatchedRecord>> SearchImageAsync(
            this ISignatureStorage storage, Stream stream, bool allOrientations = true)
        {
            var records = await storage.SearchImageAsync<object>(stream, allOrientations).ConfigureAwait(false);
            return records.Select(x => (MatchedRecord)x).ToList();
        }
    }
}
