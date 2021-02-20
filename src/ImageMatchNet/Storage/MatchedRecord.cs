using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet.Storage
{
    public class MatchedRecord
    {
        public string Id { get; set; }
        public double Dist { get; set; }
        public string Path { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
    }
}
