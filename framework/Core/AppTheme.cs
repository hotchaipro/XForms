using System;

namespace XForms
{
    public abstract class AppTheme
    {
        public static readonly AppTheme Light = new LightTheme();
        public static readonly AppTheme Dark = new DarkTheme();

        public static readonly ThemeColor DefaultAccentColor = ThemeColor.FromRgb(0x64b5f6, 0x2196f3);

        protected AppTheme()
        {
        }

        public abstract Color AccentColor { get; }
        public abstract Color ForegroundColor { get; }
        public abstract Color BackgroundColor { get; }
        public abstract Color SubtleBackgroundColor { get; }
        public abstract Color SubtleForegroundColor { get; }
    }

    public struct ThemeColor
    {
        public static ThemeColor FromRgb(
            uint rgbLightColor,
            uint rgbDarkColor)
        {
            return new ThemeColor(
                Color.FromRgb(rgbLightColor),
                Color.FromRgb(rgbDarkColor));
        }

        public ThemeColor(
            Color lightColor,
            Color darkColor)
        {
            this.Light = lightColor;
            this.Dark = darkColor;
        }

        public readonly Color Light;
        public readonly Color Dark;
    }

    public class LightTheme : AppTheme
    {
        private Color _foregroundColor = Colors.Black;
        private Color _backgroundColor = Colors.White;
        private Color _subtleBackgroundColor = Color.FromRgb(0xf4, 0xf4, 0xf4);
        private Color _subtleForegroundColor = Color.FromRgb(0x66, 0x66, 0x66);

        internal protected LightTheme()
        {
        }

        public override Color AccentColor => Application.Current.AccentColor.Light;
        public override Color ForegroundColor => this._foregroundColor;
        public override Color BackgroundColor => this._backgroundColor;
        public override Color SubtleBackgroundColor => this._subtleBackgroundColor;
        public override Color SubtleForegroundColor => this._subtleForegroundColor;
    }

    public class DarkTheme : AppTheme
    {
        private Color _foregroundColor = Color.FromRgb(0xcc, 0xcc, 0xcc);
        private Color _backgroundColor = Colors.Black;
        private Color _subtleBackgroundColor = Color.FromRgb(0x10, 0x10, 0x10);
        private Color _subtleForegroundColor = Color.FromRgb(0x99, 0x99, 0x99);

        internal protected DarkTheme()
        {
        }

        public override Color AccentColor => Application.Current.AccentColor.Dark;
        public override Color ForegroundColor => this._foregroundColor;
        public override Color BackgroundColor => this._backgroundColor;
        public override Color SubtleBackgroundColor => this._subtleBackgroundColor;
        public override Color SubtleForegroundColor => this._subtleForegroundColor;
    }
}
