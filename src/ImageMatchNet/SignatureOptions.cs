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
            // 不带透明通道
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
        /// 图像上的网格点数量n x n。默认9x9网格点
        /// </summary>
        public int GridPointNum { get; set; }

        public (int lower, int upper) CropPercentiles { get; set; }

        /// <summary>
        /// 声明两个相邻网格点相同的边界值。默认2.0/255
        /// </summary>
        public double IdenticalTolerance { get; set; }

        /// <summary>
        /// 将相邻差异分层到的正、负组数。Levels=2->[-2，-1，0，1，2]（默认值为2）
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 使用以该像素为中心的3×3块的平均值替代像素的灰度级别本身
        /// </summary>
        public bool UseAveragePixel { get; set; }

        /// <summary>
        /// 像素灰度计算公式
        /// </summary>
        public GrayComputeExpression GrayCalculator { get; set; }
    }

    /// <summary>
    /// 像素灰度计算公式
    /// </summary>
    /// <param name="R">像素R值(0-255)</param>
    /// <param name="G">像素G值(0-255)</param>
    /// <param name="B">像素B值(0-255)</param>
    /// <param name="A">像素透明通道Alpha值(0-255)</param>
    /// <returns></returns>
    public delegate double GrayComputeExpression(byte R, byte G, byte B, byte A);
}
