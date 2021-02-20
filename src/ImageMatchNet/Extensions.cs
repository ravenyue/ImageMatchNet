using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ImageMatchNet
{
    public static class Extensions
    {
        public static double Percentile(this IEnumerable<double> source, double percentile)
        {
            var sorted = source.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                throw new InvalidOperationException("source contains no elements.");
            }

            int N = sorted.Count;
            double n = (N - 1) * percentile + 1;
            if (n == 1d) return sorted[0];
            else if (n == N) return sorted[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return sorted[k - 1] + d * (sorted[k] - sorted[k - 1]);
            }
        }

        public static double[] Percentile(this IEnumerable<double> source, params double[] percentiles)
        {
            var sorted = source.OrderBy(x => x).ToList();
            if (sorted.Count == 0)
            {
                throw new InvalidOperationException("source contains no elements.");
            }

            int N = sorted.Count;

            var result = new double[percentiles.Length];

            for (int i = 0; i < percentiles.Length; i++)
            {
                double n = (N - 1) * percentiles[i] + 1;

                if (n == 1d)
                {
                    result[i] = sorted[0];
                }
                else if (n == N)
                {
                    result[i] = sorted[N - 1];
                }
                else
                {
                    int k = (int)n;
                    double d = n - k;
                    result[i] = sorted[k - 1] + d * (sorted[k] - sorted[k - 1]);
                }
            }

            return result;
        }

        public static double[] Linspace(double start, double stop, int num, bool endpoint = true)
        {
            var result = new double[num];

            double step = 0;
            if (endpoint)
                step = (stop - start) / (num - 1);
            else
                step = (stop - start) / num;

            for (int i = 0; i < num; i++)
            {
                result[i] = start + i * step;
            }

            return result;
        }

        public static int[] LinspaceInt(double start, double stop, int num, bool endpoint = true)
        {
            var result = new int[num];

            double step = 0;
            if (endpoint)
                step = (stop - start) / (num - 1);
            else
                step = (stop - start) / num;

            for (int i = 0; i < num; i++)
            {
                result[i] = (int)(start + i * step);
            }

            return result;
        }
        public static int[] ArrayRange(int length)
        {
            var arr = new int[length];

            for (int i = 0; i < length; i++)
            {
                arr[i] = i;
            }

            return arr;
        }

        public static int[] ArrayPower(int bottom, int[] indices)
        {
            var arr = new int[indices.Length];

            for (int i = 0; i < indices.Length; i++)
            {
                arr[i] = (int)Math.Pow(bottom, indices[i]);
            }

            return arr;
        }

        public static int DotProduct(this int[] first, int[] second)
        {
            if (first.Length != second.Length)
            {
                throw new InvalidOperationException("数组长度必须相等");
            }
            int result = 0;
            for (int i = 0; i < first.Length; i++)
            {
                result += first[i] * second[i];
            }

            return result;
        }

        public static int[] AddNum(this int[] source, int value)
        {
            for (int i = 0; i < source.Length; i++)
            {
                source[i] += value;
            }

            return source;
        }
    }
}
