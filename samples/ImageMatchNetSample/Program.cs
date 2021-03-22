using ImageMatchNet;
using System;

namespace ImageMatchNetSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var path1 = @"E:\projects\ImageMatchNet\samples\ImageMatchNetSample\images\1.jpg";
            var path2 = @"E:\projects\ImageMatchNet\samples\ImageMatchNetSample\images\2.jpg";

            var gis = new ImageSignature();

            var sign1 = gis.GenerateSignature(path1);
            var sign2 = gis.GenerateSignature(path2);

            var dist = gis.NormalizedDistance(sign1, sign2);

            Console.WriteLine(dist);
        }
    }
}
