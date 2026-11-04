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

        [Fact]
        public void BothModesShareTheStrongAccent()
        {
            var accent = Color.FromRgb(0xa4c400);

            Assert.Equal(Color.FromRgb(0x6a7f00), Theme.CreateColors(ThemeMode.Light, accent).StrongAccent);
            Assert.Equal(Color.FromRgb(0x6a7f00), Theme.CreateColors(ThemeMode.Dark, accent).StrongAccent);
        }
    }
}
