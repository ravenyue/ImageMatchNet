using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet.Storage
{
    public class SignatureData
    {
        public string Key { get; set; }
        public int[] Signature { get; set; }
        public Dictionary<string, int> SimpleWord { get; set; }
    }

    public class SignatureData<TMetadata> : SignatureData where TMetadata : class
    {
        public TMetadata Metadata { get; set; }
    }
}
