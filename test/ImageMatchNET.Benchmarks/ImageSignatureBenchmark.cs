using BenchmarkDotNet.Attributes;
using ImageMatchNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ImageMatchNET.Benchmarks
{
    [MemoryDiagnoser]
    public class ImageSignatureBenchmark
    {
        public string path;
        public ImageSignature gis;

        public ImageSignatureBenchmark()
        {
            path = @"E:\projects\ImageMatchNet\samples\ImageMatchNetSample\images\1.jpg";
            gis = new ImageSignature();
        }

        [Benchmark]
        public int[] Signature() => gis.GenerateSignature(path);

    }
}
