using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet
{
    public static class SignatureComparison
    {
        public const double DefaultMatchThreshold = 0.4;

        public static double NormalizedDistance(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
            ReadOnlySpan<int> subtractedVectors = right.Subtract(left);

            double subtractedLength = subtractedVectors.EuclideanLength();

            double leftLength = left.EuclideanLength();
            double rightLength = right.EuclideanLength();

            double combinedLength = leftLength + rightLength;

            if (combinedLength == 0.0)
            {
                return 0.0;
            }

            return subtractedLength / combinedLength;
        }

        public static bool IsMatch(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
            return NormalizedDistance(left, right) < DefaultMatchThreshold;
        }

        private static double EuclideanLength(this ReadOnlySpan<int> signature)
        {
            var sum = 0.0;
            foreach (var val in signature)
            {
                sum += Math.Pow(val, 2);
            }

            return Math.Sqrt(sum);
        }

        private static ReadOnlySpan<int> Subtract
        (
            this ReadOnlySpan<int> left,
            ReadOnlySpan<int> right
        )
        {
            var result = new int[left.Length];

            for (var i = 0; i < left.Length; ++i)
            {
                var leftValue = left[i];

                if (i >= right.Length)
                {
                    result[i] = leftValue;
                    continue;
                }

                var rightValue = right[i];

                result[i] = (leftValue - rightValue);
            }

            return result;
        }
    }
}
