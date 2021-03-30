using ImageMatchNet.Storage;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet.Elasticsearch
{
    public class EsStorageOptions
    {
        public EsStorageOptions()
        {
            Uri = "http://localhost:9200";
            Index = ElasticsearchSignatureStorage.DefaultIndex;
            WordWidth = SignatureStorageBase.DefaultWordWidth;
            WordNumber = SignatureStorageBase.DefaultWordNumber;
            SignatureOptions = new SignatureOptions();

            var settings = new ConnectionSettings(new Uri(Uri));
            settings.DefaultIndex(Index);
            Client = new ElasticClient(settings);
        }

        public string Uri { get; set; }

        public string Index { get; set; }

        public int WordWidth { get; set; }

        public int WordNumber { get; set; }

        public ElasticClient Client { get; set; }
        
        public SignatureOptions SignatureOptions { get; set; }
    }
}
