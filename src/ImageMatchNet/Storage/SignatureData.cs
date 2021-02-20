using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet.Storage
{
    public class SignatureData
    {
        public string Path { get; set; }

        public int[] Signature { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public Dictionary<string, int> SimpleWord { get; set; }
    }
}
