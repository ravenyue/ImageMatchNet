using ImageMatchNet;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ImageMatchNe.Tests
{
    public class ImageSignatureTest
    {
        private readonly string path1;
        private readonly string path2;
        private readonly string path3;
        private readonly string path4;
        private readonly string path5;

        public ImageSignatureTest()
        {
            path1 = Path.Combine(Directory.GetCurrentDirectory(), "images/1.jpg");
            path2 = Path.Combine(Directory.GetCurrentDirectory(), "images/2.jpg");
            path3 = Path.Combine(Directory.GetCurrentDirectory(), "images/3.jpg");
            path4 = Path.Combine(Directory.GetCurrentDirectory(), "images/4.png");
            path5 = Path.Combine(Directory.GetCurrentDirectory(), "images/5.jpg");
        }

        [Fact]
        public void SignatureLengthShouldBeFixed()
        {
            var gis = new ImageSignature();
            var sign1 = gis.GenerateSignature(path1);
            var sign2 = gis.GenerateSignature(path2);

            Assert.Equal(9 * 9 * 8, sign1.Length);
            Assert.Equal(9 * 9 * 8, sign2.Length);
        }

        [Fact]
        public void SameImageShouldMatch()
        {
            var gis = new ImageSignature();
            var sign = gis.GenerateSignature(path1);
            var dist = gis.NormalizedDistance(sign, sign);

            Assert.Equal(0.0, dist);
        }

        [Fact]
        public void Image1AndImage2ShouldMatch()
        {
            var gis = new ImageSignature();
            var sign1 = gis.GenerateSignature(path1);
            var sign2 = gis.GenerateSignature(path2);
            var dist = gis.NormalizedDistance(sign1, sign2);
            
            Assert.True(gis.IsMatch(sign1, sign2));
            Assert.True(dist < SignatureComparison.DefaultMatchThreshold);
        }

        [Fact]
        public void SquashedImageShouldMatch()
        {
            var gis = new ImageSignature();
            var sign1 = gis.GenerateSignature(path1);
            var sign5 = gis.GenerateSignature(path5);
            var dist = gis.NormalizedDistance(sign1, sign5);

            Assert.True(gis.IsMatch(sign1, sign5));
            Assert.True(dist < SignatureComparison.DefaultMatchThreshold);
        }

        [Fact]
        public void Image1AndImage3ShouldNotMatch()
        {
            var gis = new ImageSignature();
            var sign1 = gis.GenerateSignature(path1);
            var sign3 = gis.GenerateSignature(path3);
            var dist = gis.NormalizedDistance(sign1, sign3);

            Assert.False(gis.IsMatch(sign1, sign3));
            Assert.False(dist < SignatureComparison.DefaultMatchThreshold);
        }

        [Fact]
        public void SolidColorPictureSignatureShouldBeZero()
        {
            var gis = new ImageSignature();
            var sign4 = gis.GenerateSignature(path4);

            Assert.True(sign4.All(x => x == 0));
        }
    }
}
