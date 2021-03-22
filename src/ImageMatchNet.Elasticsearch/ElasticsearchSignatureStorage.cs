using Elasticsearch.Net;
using ImageMatchNet.Storage;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatchNet.Elasticsearch
{
    public class ElasticsearchSignatureStorage : SignatureStorageBase
    {
        public const string DefaultIndex = "images";

        private readonly ElasticClient _client;
        private readonly string _index;

        public ElasticsearchSignatureStorage(string esUri, string index = DefaultIndex)
        {
            var settings = new ConnectionSettings(new Uri(esUri));
            settings.DefaultIndex(index);

            _client = new ElasticClient(settings);
            _index = index;
        }

        public ElasticsearchSignatureStorage(ElasticClient client, string index = DefaultIndex)
        {
            _client = client;
            _index = index;
        }

        public override void InsertSignature(SignatureData data)
        {
            _client.Index(data, idx => idx.Index(_index));
        }

        public override Task InsertSignatureAsync(SignatureData data)
        {
            return _client.IndexAsync(data, idx => idx.Index(_index));
        }

        public override List<MatchedRecord> SearchSignature(SignatureData data)
        {
            var request = new SearchRequest(_index)
            {
                From = 0,
                Size = 10,
                Query = MakeTermQuerys(data)
            };

            //var json = _client.SourceSerializer.SerializeToString(request);

            var res = _client.Search<SignatureData>(request);

            return BuildMatchedRecords(data.Signature, res);
        }

        public override async Task<List<MatchedRecord>> SearchSignatureAsync(SignatureData data)
        {
            var request = new SearchRequest(_index)
            {
                From = 0,
                Size = 10,
                Query = MakeTermQuerys(data)
            };

            //var json = _client.SourceSerializer.SerializeToString(request);

            var res = await _client.SearchAsync<SignatureData>(request);

            return BuildMatchedRecords(data.Signature, res);
        }

        public override void DeleteImage(string key)
        {
            _client.DeleteByQuery<SignatureData>(d => d
                .Query(q => q
                    .Match(c => c
                        .Field(x => x.Key)
                        .Query(key)
                    )
                )
            );
        }

        public override Task DeleteImageAsync(string key)
        {
            return _client.DeleteByQueryAsync<SignatureData>(d => d
                .Query(q => q
                    .Match(c => c
                        .Field(x => x.Key)
                        .Query(key)
                    )
                )
            );
        }

        private List<MatchedRecord> BuildMatchedRecords(int[] sourceSignature, ISearchResponse<SignatureData> searchResponse)
        {
            var records = new List<MatchedRecord>(searchResponse.Hits.Count);

            foreach (var signData in searchResponse.Hits.Select(h => h.Source))
            {
                records.Add(new MatchedRecord
                {
                    Dist = Generator.NormalizedDistance(sourceSignature, signData.Signature),
                    Metadata = signData.Metadata,
                    Key = signData.Key
                });
            }
            return records;
        }

        private QueryContainer MakeTermQuerys(SignatureData data)
        {
            QueryContainer querys = new QueryContainer();

            foreach (var word in data.SimpleWord)
            {
                var field = $"simpleWord.{word.Key}";
                querys = querys || new TermQuery { Field = new Field(field), Value = word.Value };
            }

            return querys;
        }
    }
}
