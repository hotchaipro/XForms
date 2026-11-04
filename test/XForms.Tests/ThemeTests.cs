using System;
using Xunit;

namespace XForms.Tests
{
    public class ThemeTests
    {
        [Theory]
        [InlineData(0xa4c400, 0x6a7f00)]
        [InlineData(0x2196f3, 0x0b78ce)]
        [InlineData(0xe3c800, 0x857500)]
        [InlineData(0xf0a30a, 0x9f6c07)]
        [InlineData(0x00aba9, 0x008281)]
        [InlineData(0x0b78ce, 0x0b78ce)]
        [InlineData(0x000000, 0x000000)]
        public void StrongColorKeepsTheHueAndDarkensUntilWhiteReads(
            int color,
            int expected)
        {
            var strong = Theme.StrongColor(Color.FromRgb((uint)color));

            Assert.Equal(Color.FromRgb((uint)expected), strong);
        }

        [Theory]
        [InlineData(0xffeb3b)]
        [InlineData(0xff9800)]
        [InlineData(0x2196f3)]
        [InlineData(0xd81b60)]
        [InlineData(0x8e24aa)]
        [InlineData(0x5c6bc0)]
        [InlineData(0x000000)]
        public void BrightColorReadsOnBlackAndStaysAsDeepAsItCan(
            int color)
        {
            var original = Color.FromRgb((uint)color);
            var bright = Theme.BrightColor(original);

            Assert.True(ContrastOnBlack(bright) >= 4.5, $"{bright} on black");
            if (ContrastOnBlack(original) >= 4.5)
            {
                Assert.Equal(original, bright);
            }
            else
            {
                // A step darker wouldn't read
                Assert.True(ContrastOnBlack(bright) < 4.7, $"{bright} is lighter than it needs to be");
            }
        }

        private static double ContrastOnBlack(
            Color color)
        {
            static double Channel(byte value)
            {
                double c = value / 255.0;
                return (c <= 0.04045) ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            double luminance = (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
            return (luminance + 0.05) / 0.05;
        }

        [Fact]
        public void BothModesShareTheStrongAccent()
        {
            var accent = Color.FromRgb(0xa4c400);

            Assert.Equal(Color.FromRgb(0x6a7f00), Theme.CreateColors(ThemeMode.Light, accent).StrongAccent);
            Assert.Equal(Color.FromRgb(0x6a7f00), Theme.CreateColors(ThemeMode.Dark, accent).StrongAccent);
        }
    }
}
