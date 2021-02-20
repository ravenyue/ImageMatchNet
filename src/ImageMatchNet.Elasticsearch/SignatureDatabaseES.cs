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
    public class SignatureDatabaseES : SignatureDatabase
    {
        private readonly ElasticClient _client;
        private readonly string _index;

        public SignatureDatabaseES(string esUri, string index = "images")
        {
            var settings = new ConnectionSettings(new Uri(esUri));
            settings.DefaultIndex(index);

            _client = new ElasticClient(settings);
            _index = index;
        }

        public SignatureDatabaseES(ElasticClient client, string index = "images")
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

            var json = _client.SourceSerializer.SerializeToString(request);

            var res = _client.Search<SignatureData>(request);

            var records = new List<MatchedRecord>(res.Hits.Count);

            foreach (var signData in res.Hits.Select(h => h.Source))
            {
                records.Add(new MatchedRecord
                {
                    Dist = SignatureComparison.NormalizedDistance(data.Signature, signData.Signature),
                    Metadata = signData.Metadata,
                    Path = signData.Path
                });
            }
            return records;
        }

        public override async Task<List<MatchedRecord>> SearchSignatureAsync(SignatureData data)
        {
            var request = new SearchRequest(_index)
            {
                From = 0,
                Size = 10,
                Query = MakeTermQuerys(data)
            };

            var json = _client.SourceSerializer.SerializeToString(request);

            var res = await _client.SearchAsync<SignatureData>(request);

            var records = new List<MatchedRecord>(res.Hits.Count);

            foreach (var signData in res.Hits.Select(h => h.Source))
            {
                records.Add(new MatchedRecord
                {
                    Dist = SignatureComparison.NormalizedDistance(data.Signature, signData.Signature),
                    Metadata = signData.Metadata,
                    Path = signData.Path
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
