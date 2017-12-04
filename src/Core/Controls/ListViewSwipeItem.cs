using System;

namespace XForms.Controls
{
    internal enum ListViewSwipeDirection
    {
        Primary = 0,
        Secondary,
    }

    internal class ListViewSwipeItem : UserControl
    {
        private ListViewSwipeDirection _swipeDirection;
        private UICommand _command;
        private TextFormat _textFormat;
        private Size _iconSize;

        public ListViewSwipeItem()
        {
            this._textFormat = new TextFormat()
            {
                FontSize = 12.0f,
                HorizontalAlignment = LayoutAlignment.End,
                VerticalAlignment = LayoutAlignment.Center,
            };

            this._swipeDirection = ListViewSwipeDirection.Primary;
        }

        public ListViewSwipeDirection Direction
        {
            get
            {
                return this._swipeDirection;
            }

            set
            {
                this._swipeDirection = value;

                this._textFormat.HorizontalAlignment = (value == ListViewSwipeDirection.Primary ? LayoutAlignment.End : LayoutAlignment.Start);

                this.Invalidate();
            }
        }

        public UICommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                this._command = value;

                this.Invalidate();
            }
        }

        public Bitmap Icon
        {
            get
            {
                return this._command?.Icon ?? null;
            }
        }

        public Size IconSize
        {
            get
            {
                return this._iconSize;
            }

            set
            {
                this._iconSize = value;

                this.Invalidate();
            }
        }

        public string Text
        {
            get
            {
                return this._command?.Text ?? null;
            }
        }

        private Color ForegroundColor
        {
            get
            {
                return Colors.White;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return this._command?.AccentColor ?? Colors.Transparent;
            }
        }

        public void Reset()
        {
            this.Command = null;
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            const float Padding = 10.0f;

            base.DrawBackground(drawContext, bounds);

            bounds = bounds.Deflate(Padding, 0);

            float origin = (this._swipeDirection == ListViewSwipeDirection.Primary ? 1.0f : 0.0f);

            Rectangle imageBounds = new Rectangle(
                bounds.X + (origin * (bounds.Width - this._iconSize.Width)),
                bounds.Center.Y - (this._iconSize.Height / 2.0f),
                this._iconSize.Width,
                this._iconSize.Height);

            Rectangle textBounds = bounds.Deflate(imageBounds.Width + Padding, 0);

            if (null != this.Icon)
            {
                drawContext.DrawImage(this.Icon, imageBounds, this.ForegroundColor);
            }

            if (null != this.Text)
            {
                drawContext.DrawText(this.Text, textBounds, this.ForegroundColor, this._textFormat);
            }
        }
    }
}
