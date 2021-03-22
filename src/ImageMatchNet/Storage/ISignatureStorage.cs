using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Storage
{
    public interface ISignatureStorage
    {
        int[] AddImage(string key, string filePath, Dictionary<string, string> metadata = null);
        int[] AddImage(string key, Stream stream, Dictionary<string, string> metadata = null);
        Task<int[]> AddImageAsync(string key, string filePath, Dictionary<string, string> metadata = null);
        Task<int[]> AddImageAsync(string key, Stream stream, Dictionary<string, string> metadata = null);
        List<MatchedRecord> SearchImage(int[] sign);
        List<MatchedRecord> SearchImage(string filePath, bool allOrientations);
        List<MatchedRecord> SearchImage(Stream stream, bool allOrientations);
        Task<List<MatchedRecord>> SearchImageAsync(int[] sign);
        Task<List<MatchedRecord>> SearchImageAsync(string filePath, bool allOrientations);
        Task<List<MatchedRecord>> SearchImageAsync(Stream stream, bool allOrientations);
        void DeleteImage(string key);
        Task DeleteImageAsync(string key);
    }
}
