using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet.Storage
{
    public class MatchedRecord
    {
        public string Key { get; set; }
        public double Dist { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class MatchedRecord<TMetadata>
    {
        public string Key { get; set; }
        public double Dist { get; set; }
        public TMetadata Metadata { get; set; }
    }

    public class MatchedRecordEqualityComparer : IEqualityComparer<MatchedRecord>
    {
        public bool Equals(MatchedRecord x, MatchedRecord y)
        {
            return x.Key == y.Key;
        }

        public int GetHashCode(MatchedRecord obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
