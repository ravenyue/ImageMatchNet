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
        }

        public string Uri { get; set; }

        public string Index { get; set; }

        /// <summary>
        /// The width of a word (default 16)
        /// </summary>
        public int WordWidth { get; set; }

        /// <summary>
        /// The number of words (default 63)
        /// </summary>
        public int WordNumber { get; set; }

        public ElasticClient Client { get; set; }
        
        public SignatureOptions SignatureOptions { get; set; }
    }
}
