using Elasticsearch.Net;
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
            Size = ElasticsearchSignatureStorage.DefaultSize;
            Refresh = ElasticsearchSignatureStorage.DefaultRefresh;
            SignatureOptions = new SignatureOptions();
        }

        public string Uri { get; set; }

        /// <summary>
        /// A name for the Elasticsearch index (default 'images')
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// The width of a word (default 16)
        /// </summary>
        public int WordWidth { get; set; }

        /// <summary>
        /// The number of words (default 63)
        /// </summary>
        public int WordNumber { get; set; }

        /// <summary>
        /// Maximum number of Elasticsearch results (default 100)
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Refresh parameters during insertion (default False)
        /// </summary>
        public Refresh Refresh { get; set; }

        public ElasticClient Client { get; set; }
        
        public SignatureOptions SignatureOptions { get; set; }
    }
}
