using System;
using System.Collections.Generic;
using System.Text;

namespace ImageMatchNet
{
    public class SignatureOptions
    {
        public SignatureOptions()
        {
            GridPointNum = 9;
            CropPercentiles = (0, 100);
            IdenticalTolerance = 2.0 / 255;
            Level = 2;
            UseAveragePixel = false;
            GrayCalculator = DefaultGrayExpression;
        }

        public static double DefaultGrayExpression(byte R, byte G, byte B, byte A)
        {
            if (A == 255)
            {
                return (0.2125 * R + 0.7154 * G + 0.0721 * B) / 255;
            }
            else
            {
                int bgColur = 1;
                double alpha = A / 255.0;

                double red = bgColur * (1 - alpha) + R * alpha;
                double green = bgColur * (1 - alpha) + G * alpha;
                double blue = bgColur * (1 - alpha) + B * alpha;

                return 0.2125 * red + 0.7154 * green + 0.0721 * blue;
            }
        }

        /// <summary>
        /// number of grid imposed on image. Grid is n x n (default 9)
        /// </summary>
        public int GridPointNum { get; set; }

        /// <summary>
        /// lower and upper bounds when considering how much variance to keep in the image (default (0, 100))
        /// </summary>
        public (int lower, int upper) CropPercentiles { get; set; }

        /// <summary>
        /// cutoff difference for declaring two adjacent grid points identical (default 2/255)
        /// </summary>
        public double IdenticalTolerance { get; set; }

        /// <summary>
        /// number of positive and negative groups to stratify neighbor differences into. n = 2 -> [-2, -1, 0, 1, 2] (default 2)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Use the average of the 3×3 blocks centered on the pixel to replace the gray level of the pixel itself (default false)
        /// </summary>
        public bool UseAveragePixel { get; set; }

        /// <summary>
        /// Pixel gray calculation expression
        /// </summary>
        public GrayComputeExpression GrayCalculator { get; set; }
    }

    /// <summary>
    /// Pixel gray calculation expression
    /// </summary>
    /// <param name="R">R(0-255)</param>
    /// <param name="G">G(0-255)</param>
    /// <param name="B">B(0-255)</param>
    /// <param name="A">Alpha(0-255)</param>
    /// <returns></returns>
    public delegate double GrayComputeExpression(byte R, byte G, byte B, byte A);
}
