using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageMatchNet
{
    public class ImageSignature
    {
        private static IReadOnlyList<Point> _neighbourCoordinateMap = new[]
        {
            new Point { X = -1, Y = -1 },
            new Point { X = -1, Y = 0, },
            new Point { X = -1, Y = 1 },
            new Point { X = 0, Y = -1 },
            new Point { X = 0, Y = 1 },
            new Point { X = 1, Y = -1 },
            new Point { X = 1, Y = 0 },
            new Point { X = 1, Y = 1 }
        };

        private readonly SignatureOptions _options;

        public ImageSignature()
            : this(new SignatureOptions())
        {

        }

        public ImageSignature(SignatureOptions options)
        {
            _options = options;
        }

        #region Overload
        public int[] GenerateSignature(Stream stream)
        {
            return GenerateSignature(Image.Load<Rgba32>(stream));
        }

        public int[] GenerateSignature(string filePath)
        {
            return GenerateSignature(Image.Load<Rgba32>(filePath));
        }

        public int[] GenerateSignature(Image image)
        {
            return GenerateSignature(image.CloneAs<Rgba32>());
        }
        #endregion

        public int[] GenerateSignature(Image<Rgba32> image)
        {
            // 裁剪图片
            // image.Mutate(o => o.EntropyCrop());

            // 计算格点坐标
            Point[] coords = ComputeGridPoints(image.Width, image.Height, _options.GridPointNum);

            // 计算以每个网格点为中心的每个P×P方块的灰度平均值。 double[9 * 9=81] OKOK
            double[] squareAverages = ComputeAverageBrightness(image, coords);

            // 格点与相邻的8个格点的灰度差异 double[9 * 9 * 8]
            ReadOnlySpan<double> brightnessDifferences = ComputeNeighbourDifferences(squareAverages);


            // 生成最终的差异级别数组签名
            return ComputeRelativeBrightnessLevels(brightnessDifferences);
        }

        /// <summary>
        /// 获取图片像素的灰度数组
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public double[] GetPixelsGray(Image<Rgba32> image)
        {
            // 获取图片像素数组
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
            {
                throw new InvalidOperationException("The backing buffer for the image was not contiguous.");
            }

            double[] grayArray = new double[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                // 根据RGBA计算灰度
                grayArray[i] = _options.GrayCalculator(pixel.R, pixel.G, pixel.B, pixel.A);
            }
            return grayArray;
        }

        /// <summary>
        /// 计算以每个网格点为中心的P×P方块的灰度平均值
        /// </summary>
        /// <param name="image">图片</param>
        /// <param name="gridPoints">格点坐标</param>
        /// <returns></returns>
        private double[] ComputeAverageBrightness(Image<Rgba32> image, Point[] gridPoints)
        {
            // 正方形大小
            var squareSize = (int)Math.Max
            (
                2.0,
                Math.Round(Math.Min(image.Width, image.Height) / 20.0)
            );

            // 获取图片像素数组
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
            {
                throw new InvalidOperationException("The backing buffer for the image was not contiguous.");
            }

            // 格点平均灰度数组
            double[] averageBrightness = new double[gridPoints.Length];
            for (var i = 0; i < gridPoints.Length; i++)
            {
                var point = gridPoints[i];
                // 计算格点正方形区域平均灰度值
                averageBrightness[i] = ComputeSquareAverage
                (
                    pixels,
                    image.Width,
                    image.Height,
                    point,
                    squareSize
                );
            }

            return averageBrightness;
        }

        /// <summary>
        /// 计算格点坐标
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The centers.</returns>

        private Point[] ComputeGridPoints(int width, int height, int gridPointNum)
        {
            var coords = new Point[gridPointNum * gridPointNum];
            // 行偏移
            double xOffset = height / (gridPointNum + 1.0);
            // 列偏移
            double yOffset = width / (gridPointNum + 1.0);

            var index = 0;

            for (var x = 0; x < _options.GridPointNum; ++x)
            {
                for (var y = 0; y < _options.GridPointNum; ++y)
                {
                    // 第 X 行,第 Y 列
                    coords[index] = new Point
                    {
                        X = (int)(xOffset * (x + 1)),
                        Y = (int)(yOffset * (y + 1))
                    };
                    index++;
                }
            }

            return coords;
        }

        /// <summary>
        /// 计算图像中以给定大小为中心的正方形的平均灰度。
        /// </summary>
        /// <param name="pixels">图片像素数组</param>
        /// <param name="imageWidth">图片宽</param>
        /// <param name="imageHeight">图片高</param>
        /// <param name="squareCenter">正方形中心坐标</param>
        /// <param name="squareSize">正方形边长大小</param>
        /// <returns></returns>
        private double ComputeSquareAverage
        (
            ReadOnlySpan<Rgba32> pixels,
            int imageWidth,
            int imageHeight,
            Point squareCenter,
            int squareSize
        )
        {
            // 正方形左上角坐标
            int squareCornerX = (int)Math.Round(squareCenter.X - (squareSize / 2.0));
            int squareCornerY = (int)Math.Round(squareCenter.Y - (squareSize / 2.0));

            double sum = 0.0;
            // 遍历正方形区域说有像素
            for (int x = squareCornerX; x < squareCornerX + squareSize; x++)
            {
                if (x > imageHeight || x < 0)
                {
                    continue;
                }

                for (int y = squareCornerY; y < squareCornerY + squareSize; y++)
                {
                    if (y > imageWidth || y < 0)
                    {
                        continue;
                    }
                    // 累加像素灰度值
                    sum += GetPixelGray(pixels, imageWidth, imageHeight, x, y, _options.UseAveragePixel);
                }
            }
            // 返回平均值
            return sum / (squareSize * squareSize);
        }

        /// <summary>
        /// 获取像素灰度
        /// </summary>
        /// <param name="pixels">图片像素数组</param>
        /// <param name="imageWidth">图片宽</param>
        /// <param name="imageHeight">图片高</param>
        /// <param name="xCoord">像素x坐标(第几行)</param>
        /// <param name="yCoord">像素y坐标(第几列)</param>
        /// <param name="useAveragePixel">是否使用9宫格的平均值代替像素本身的值</param>
        /// <returns></returns>
        private double GetPixelGray(ReadOnlySpan<Rgba32> pixels,
            int imageWidth, int imageHeight,
            int xCoord, int yCoord,
            bool useAveragePixel = false)
        {
            // 使用像素本身的值
            if (!useAveragePixel)
            {
                int idx = (xCoord * imageWidth) + yCoord;
                Rgba32 pixel = pixels[idx];

                double gray = _options.GrayCalculator(pixel.R, pixel.G, pixel.B, pixel.A);
                return gray;
            }

            // 使用以该像素为中心的3×3块的平均值
            double sum = 0.0;

            for (int yOffset = 0; yOffset < 3; yOffset++)
            {
                int y = (yCoord - 1) + yOffset;

                if (y > imageHeight - 1 || y < 0)
                {
                    continue;
                }

                for (int xOffset = 0; xOffset < 3; xOffset++)
                {
                    int x = (xCoord - 1) + xOffset;

                    if (x > imageWidth - 1 || x < 0)
                    {
                        continue;
                    }

                    int spanIndex = y + (x * imageWidth);
                    Rgba32 pixel = pixels[spanIndex];

                    double gray = _options.GrayCalculator(pixel.R, pixel.G, pixel.B, pixel.A);
                    sum += gray;
                }
            }

            return sum / 9;
        }

        /// <summary>
        /// 计算采样邻域之间的绝对值差。如果不存在相邻，则返回的值为零
        /// </summary>
        /// <param name="brightnessAverages"></param>
        /// <returns></returns>
        private double[] ComputeNeighbourDifferences(ReadOnlySpan<double> brightnessAverages)
        {
            double[] neighbourDifferences = new double[_options.GridPointNum * _options.GridPointNum * _neighbourCoordinateMap.Count];
            int spanIndex = 0;

            for (int x = 0; x < _options.GridPointNum; x++)
            {
                for (int y = 0; y < _options.GridPointNum; y++)
                {
                    int index = y + (_options.GridPointNum * x);

                    // 格点灰度
                    double baseBrightness = brightnessAverages[index];

                    for (var i = 0; i < _neighbourCoordinateMap.Count; ++i)
                    {
                        var (tileX, tileY) = _neighbourCoordinateMap[i];

                        int neighbourX = x + tileX;
                        int neighbourY = y + tileY;

                        var neighbourIndex = neighbourY + (_options.GridPointNum * neighbourX);
                        if (neighbourIndex < 0 || neighbourIndex >= brightnessAverages.Length
                            || neighbourX < 0 || neighbourX >= _options.GridPointNum
                            || neighbourY < 0 || neighbourY >= _options.GridPointNum)
                        {
                            neighbourDifferences[spanIndex] = 0.0;
                        }
                        else
                        {
                            neighbourDifferences[spanIndex] = baseBrightness - brightnessAverages[neighbourIndex];
                        }

                        spanIndex++;
                    }
                }
            }

            return neighbourDifferences;
        }

        private int[] ComputeRelativeBrightnessLevels
        (
            ReadOnlySpan<double> neighbourDifferences
        )
        {
            var darks = new List<double>();
            var lights = new List<double>();

            foreach (var difference in neighbourDifferences)
            {
                if (Math.Abs(difference) < _options.IdenticalTolerance)
                {
                    // 这种差异被认为是一个相同的值。
                    continue;
                }

                if (difference <= -_options.IdenticalTolerance)
                {
                    darks.Add(difference);
                    continue;
                }

                if (difference >= _options.IdenticalTolerance)
                {
                    lights.Add(difference);
                }
            }

            // 亮度级别数组 最终结果
            int[] brightnessLevels = new int[neighbourDifferences.Length];

            if (darks.Count == 0)
            {
                return brightnessLevels;
            }

            var lightRanges = Extensions.Linspace(0, 1, _options.Level + 1);
            var darkRanges = Extensions.Linspace(1, 0, _options.Level + 1);
            // 0.0081594987995740331 0.11829038571264948 0.69427196640844191
            var lightCutoffs = lights.Percentile(lightRanges);
            var darkCutoffs = darks.Percentile(darkRanges);

            for (var i = 0; i < neighbourDifferences.Length; i++)
            {
                var difference = neighbourDifferences[i];
                if (difference >= -_options.IdenticalTolerance && difference <= _options.IdenticalTolerance)
                {
                    brightnessLevels[i] = 0;
                    continue;
                }

                if (difference > 0.0)
                {
                    brightnessLevels[i] = ComputeLightLevel(difference, lightCutoffs);
                    continue;
                }
                else
                {
                    brightnessLevels[i] = ComputeDarkLevel(difference, darkCutoffs);
                }
            }

            return brightnessLevels;
        }

        private int ComputeLightLevel(double diff, double[] lightCutoffs)
        {
            for (int i = 0; i < lightCutoffs.Length - 1; i++)
            {
                if (diff >= lightCutoffs[i] && diff <= lightCutoffs[i + 1])
                {
                    return i + 1;
                }
            }
            return 0;
        }

        private int ComputeDarkLevel(double diff, double[] darkCutoffs)
        {
            for (int i = 0; i < darkCutoffs.Length - 1; i++)
            {
                if (diff <= darkCutoffs[i] && diff >= darkCutoffs[i + 1])
                {
                    return -(i + 1);
                }
            }
            return 0;
        }
    }
}
