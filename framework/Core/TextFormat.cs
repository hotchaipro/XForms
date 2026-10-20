using System;

namespace XForms
{
    public class TextFormat
    {
        public TextFormat()
        {
            this.FontSize = 12.0f;
            this.FontWeight = FontWeight.Normal;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        public float FontSize
        {
            get;
            set;
        }

        public FontWeight FontWeight
        {
            get;
            set;
        }

        public VerticalAlignment VerticalAlignment
        {
            get;
            set;
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get;
            set;
        }
    }
}
