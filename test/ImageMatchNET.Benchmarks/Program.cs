using BenchmarkDotNet.Running;
using System;

namespace ImageMatchNET.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ImageSignatureBenchmark>();
            
            Console.WriteLine("OK");
        }
    }
}
