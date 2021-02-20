using ImageMatchNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ImageMatchNetSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var path1 = @"E:\projects\ImageMatch.NET\samples\ImageMatchSample\images\1.jpg";
            var path2 = @"E:\projects\ImageMatch.NET\samples\ImageMatchSample\images\2.jpg";

            var imgs1 = SixLabors.ImageSharp.Image.Load<Rgba32>(path1);
            var imgs2 = SixLabors.ImageSharp.Image.Load<RgbaVector>(path1);
            
            Stopwatch sw = new Stopwatch();

            var gis = new ImageSignature();

            sw.Start();
            var sign = gis.GenerateSignature(imgs1);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            
            //PrintArray(sign);

            Console.WriteLine("OK");
        }


        public static void PrintArray(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                int num = arr[i];
                if ((i + 1) % 24 == 0)
                {
                    if (num < 0)
                        Console.WriteLine($" {num}");
                    else
                        Console.WriteLine($"  {num}");
                }
                else
                {
                    if (num < 0)
                        Console.Write($" {num}");
                    else
                        Console.Write($"  {num}");
                }

            }
        }

    }
}
