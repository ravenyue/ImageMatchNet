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

        public ElasticsearchSignatureStorage(EsStorageOptions options)
            : base(options.WordWidth, options.WordNumber, options.SignatureOptions)
        {
            _client = options.Client;
            _index = options.Index;
        }

        public override void InsertOrUpdateSignature<TMetadata>(SignatureData<TMetadata> data)
        {
            var id = GetDocumentIdByKey(data.Key);

            if (string.IsNullOrWhiteSpace(id))
            {
                _client.Index(data, idx => idx.Index(_index));
            }
            else
            {
                _client.Index(data, idx => idx.Index(_index).Id(id));
            }
        }

        public override async Task InsertOrUpdateSignatureAsync<TMetadata>(SignatureData<TMetadata> data)
        {
            var id = await GetDocumentIdByKeyAsync(data.Key);

            if (string.IsNullOrWhiteSpace(id))
            {
                await _client.IndexAsync(data, idx => idx.Index(_index));
            }
            else
            {
                await _client.IndexAsync(data, idx => idx.Index(_index).Id(id));
            }
        }

        public override List<MatchedRecord<TMetadata>> SearchSignature<TMetadata>(SignatureData data)
        {
            var request = new SearchRequest(_index)
            {
                From = 0,
                Size = 10,
                Query = MakeTermQuerys(data)
            };

            //var json = _client.SourceSerializer.SerializeToString(request);

            var res = _client.Search<SignatureData<TMetadata>>(request);

            return BuildMatchedRecords(data.Signature, res);
        }

        public override async Task<List<MatchedRecord<TMetadata>>> SearchSignatureAsync<TMetadata>(SignatureData data)
        {
            var request = new SearchRequest(_index)
            {
                From = 0,
                Size = 10,
                Query = MakeTermQuerys(data)
            };

            //var json = _client.SourceSerializer.SerializeToString(request);

            var res = await _client.SearchAsync<SignatureData<TMetadata>>(request);

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

        private string GetDocumentIdByKey(string key)
        {
            var res = _client.Search<SignatureData>(d => d
                .Query(q => q
                    .Match(c => c
                        .Field(x => x.Key)
                        .Query(key)
                    )
                )
            );

            return res.Hits.Select(x => x.Id).FirstOrDefault();
        }

        private async Task<string> GetDocumentIdByKeyAsync(string key)
        {
            var res = await _client.SearchAsync<SignatureData>(d => d
                .Query(q => q
                    .Match(c => c
                        .Field(x => x.Key)
                        .Query(key)
                    )
                )
            );

            return res.Hits.Select(x => x.Id).FirstOrDefault();
        }

        private List<MatchedRecord<TMetadata>> BuildMatchedRecords<TMetadata>(int[] sourceSignature, ISearchResponse<SignatureData<TMetadata>> searchResponse)
            where TMetadata : class
        {
            var records = new List<MatchedRecord<TMetadata>>(searchResponse.Hits.Count);

            foreach (var signData in searchResponse.Hits.Select(h => h.Source))
            {
                records.Add(new MatchedRecord<TMetadata>
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
