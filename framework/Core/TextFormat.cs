using System;

namespace XForms
{
    public class TextFormat
    {
        public TextFormat()
        {
            this.FontSize = 12.0f;
            this.FontWeight = FontWeight.Normal;
            this.VerticalAlignment = LayoutAlignment.Start;
            this.HorizontalAlignment = LayoutAlignment.Start;
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

        public LayoutAlignment VerticalAlignment
        {
            get;
            set;
        }

        public LayoutAlignment HorizontalAlignment
        {
            get;
            set;
        }
    }
}
