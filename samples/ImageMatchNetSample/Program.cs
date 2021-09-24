using ImageMatchNet;
using ImageMatchNet.Elasticsearch;
using ImageMatchNet.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace ImageMatchNetSample
{
    public class Program
    {
        public static string path1 = Path.Combine(Directory.GetCurrentDirectory(), "images/1.jpg");
        public static string path2 = Path.Combine(Directory.GetCurrentDirectory(), "images/2.jpg");
        public static string path3 = Path.Combine(Directory.GetCurrentDirectory(), "images/3.jpg");
        
        static void Main(string[] args)
        {
            //var gis = new ImageSignature(new SignatureOptions { CropPercentiles = (5, 95) });
            var gis = new ImageSignature();

            var sign1 = gis.GenerateSignature(path1);
            var sign2 = gis.GenerateSignature(path2);
            var sign3 = gis.GenerateSignature(path3);

            var dist1 = gis.NormalizedDistance(sign1, sign2);
            var dist2 = gis.NormalizedDistance(sign1, sign3);

            Console.WriteLine($"1.jpg and 2.jpg distances: {dist1}");
            Console.WriteLine($"1.jpg and 3.jpg distances: {dist2}");

            //Storage();
        }

        static void Storage()
        {
            ISignatureStorage storage = new ElasticsearchSignatureStorage("http://localhost:9200");
            var obj = new Person { Name = "lisi", Age = 18 };
            storage.AddOrUpdateImage("iamge1", path1, obj);
            Thread.Sleep(1000);
            //storage.DeleteImage("iamge1");
            var matchs = storage.SearchImage<Person>(path1);

            Console.WriteLine("matched:");
            Console.WriteLine(JsonSerializer.Serialize(matchs));
        }

        static void PrintArray(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var val = array[i];
                if (val < 0)
                {
                    Console.Write(val);
                }
                else
                {
                    Console.Write($" {val}");
                }
                if ((i + 1) % 24 == 0)
                {
                    Console.WriteLine("");
                }
                else
                {
                    Console.Write(" ");
                }
            }
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
