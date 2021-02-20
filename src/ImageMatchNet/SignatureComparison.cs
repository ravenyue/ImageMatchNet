using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet
{
    public static class SignatureComparison
    {
        public static double NormalizedDistance
        (
            this ReadOnlySpan<int> left,
            ReadOnlySpan<int> right
        )
        {
            var subtractedVectors = right.Subtract(left);

            var subtractedLength = subtractedVectors.EuclideanLength();

            double leftLength = left.EuclideanLength();
            double rightLength = right.EuclideanLength();

            var combinedLength = leftLength + rightLength;

            if (combinedLength == 0.0)
            {
                return 0.0;
            }

            return subtractedLength / combinedLength;
        }

        public static double EuclideanLength(this ReadOnlySpan<int> signature)
        {
            var sum = 0.0;
            foreach (var val in signature)
            {
                sum += Math.Pow(val, 2);
            }

            return Math.Sqrt(sum);
        }

        public static double EuclideanLength(this ReadOnlySpan<sbyte> signature)
        {
            var sum = 0.0;
            foreach (var val in signature)
            {
                sum += Math.Pow(val, 2);
            }

            return Math.Sqrt(sum);
        }

        public static ReadOnlySpan<sbyte> Subtract
        (
            this ReadOnlySpan<int> left,
            ReadOnlySpan<int> right
        )
        {
            Span<sbyte> result = new sbyte[left.Length];

            for (var i = 0; i < left.Length; ++i)
            {
                var leftValue = (sbyte)left[i];

                if (i >= right.Length)
                {
                    result[i] = leftValue;
                    continue;
                }

                var rightValue = (sbyte)right[i];

                result[i] = (sbyte)(leftValue - rightValue);
            }

            return result;
        }
    }
}
