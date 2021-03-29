using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Storage
{
    public interface ISignatureStorage
    {
        int[] AddOrUpdateImage<TMetadata>(string key, Stream stream, TMetadata metadata) where TMetadata : class;
        Task<int[]> AddOrUpdateImageAsync<TMetadata>(string key, Stream stream, TMetadata metadata) where TMetadata : class;
        List<MatchedRecord<TMetadata>> SearchImage<TMetadata>(int[] sign) where TMetadata : class;
        List<MatchedRecord<TMetadata>> SearchImage<TMetadata>(Stream stream, bool allOrientations = true) where TMetadata : class;
        Task<List<MatchedRecord<TMetadata>>> SearchImageAsync<TMetadata>(int[] sign) where TMetadata : class;
        Task<List<MatchedRecord<TMetadata>>> SearchImageAsync<TMetadata>(Stream stream, bool allOrientations = true) where TMetadata : class;
        void DeleteImage(string key);
        Task DeleteImageAsync(string key);
    }
}
