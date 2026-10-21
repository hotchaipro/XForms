using System;
using System.Globalization;

namespace XForms
{
    public struct Color
    {
        private Color(
            byte a,
            byte r,
            byte g,
            byte b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public bool IsTransparent
        {
            get
            {
                return (this.A == 0);
            }
        }

        public static Color FromArgb(
            byte a,
            byte r,
            byte g,
            byte b)
        {
            return new Color(a, r, g, b);
        }

        public static Color FromArgb(
            uint argb)
        {
            Color color = new Color()
            {
                A = (byte)(argb >> 24),
                R = (byte)(argb >> 16),
                G = (byte)(argb >> 8),
                B = (byte)(argb),
            };

            return color;
        }

		public static Color FromArgb(
            float a,
            float r,
	        float g,
	        float b)
		{
            return new Color(
                a: (byte)(byte.MaxValue * a),
                r: (byte)(byte.MaxValue * r),
                g: (byte)(byte.MaxValue * g),
                b: (byte)(byte.MaxValue * b));
		}

		public static Color FromRgb(
            byte r,
            byte g,
            byte b)
        {
            return new Color(0xff, r, g, b);
        }

        public static Color FromRgb(
            uint rgb)
        {
            Color color = new Color()
            {
                A = 0xff,
                R = (byte)(rgb >> 16),
                G = (byte)(rgb >> 8),
                B = (byte)(rgb),
            };

            return color;
        }

		public static Color FromArgb(
            float r,
            float g,
            float b)
		{
			return new Color(
				a: byte.MaxValue,
				r: (byte)(byte.MaxValue * r),
				g: (byte)(byte.MaxValue * g),
				b: (byte)(byte.MaxValue * b));
		}

		public static Color FromString(
            string colorString)
        {
            if (String.IsNullOrWhiteSpace(colorString))
            {
                throw new ArgumentNullException(nameof(colorString));
            }

            byte a, r, g, b;
            if ((colorString.Length == 9) && (colorString[0] == '#'))
            {
                a = byte.Parse(colorString.Substring(1, 2), NumberStyles.HexNumber);
                r = byte.Parse(colorString.Substring(3, 2), NumberStyles.HexNumber);
                g = byte.Parse(colorString.Substring(5, 2), NumberStyles.HexNumber);
                b = byte.Parse(colorString.Substring(7, 2), NumberStyles.HexNumber);
            }
            else if ((colorString.Length == 7) && (colorString[0] == '#'))
            {
                a = 255;
                r = byte.Parse(colorString.Substring(1, 2), NumberStyles.HexNumber);
                g = byte.Parse(colorString.Substring(3, 2), NumberStyles.HexNumber);
                b = byte.Parse(colorString.Substring(5, 2), NumberStyles.HexNumber);
            }
            else
            {
                throw new ArgumentException("Invalid color format", "colorString");
            }

            return Color.FromArgb(a, r, g, b);
        }

        public Color MultiplyAlpha(
            float multiplier)
        {
            double a = Math.Round(this.A * multiplier);
            if (a < 0) a = 0;
            if (a > 255) a = 255;
            return new Color((byte)a, this.R, this.G, this.B);
        }

        public uint ToArgb()
        {
            uint argb = unchecked((uint)(this.A << 24 | this.R << 16 | this.G << 8 | this.B));
            return argb;
        }

        public string ToString(
            CultureInfo cultureInfo)
        {
            if (this.A == 255)
            {
                return String.Format(cultureInfo, "#{0:X2}{1:X2}{2:X2}", this.R, this.G, this.B);
            }
            else
            {
                return String.Format(cultureInfo, "#{0:X2}{1:X2}{2:X2}{3:X2}", this.A, this.R, this.G, this.B);
            }
        }

        public override string ToString()
        {
            return this.ToString(CultureInfo.InvariantCulture);
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Color)
            {
                return this == (Color)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + A.GetHashCode();
                hash = hash * 23 + R.GetHashCode();
                hash = hash * 23 + G.GetHashCode();
                hash = hash * 23 + B.GetHashCode();
                return hash;
            }
        }

        public static bool operator == (Color a, Color b)
        {
            return  (a.A == b.A) && (a.R == b.R) && (a.G == b.G) && (a.B == b.B);
        }

        public static bool operator != (Color a, Color b)
        {
            return (a.A != b.A) || (a.R != b.R) || (a.G != b.G) || (a.B != b.B);
        }
    }
}
