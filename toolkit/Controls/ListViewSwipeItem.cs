using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Toolkit
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
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
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

                this._textFormat.HorizontalAlignment = (value == ListViewSwipeDirection.Primary ? HorizontalAlignment.Right : HorizontalAlignment.Left);

                this.InvalidateVisual();
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
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

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            const float Padding = 10.0f;

            base.Render(drawContext);

            bounds = bounds.Deflate(Padding, 0);

            float origin = (this._swipeDirection == ListViewSwipeDirection.Primary ? 1.0f : 0.0f);

            Rect imageBounds = new Rect(
                bounds.X + (origin * (bounds.Width - this._iconSize.Width)),
                bounds.Center.Y - (this._iconSize.Height / 2.0f),
                this._iconSize.Width,
                this._iconSize.Height);

            Rect textBounds = bounds.Deflate(imageBounds.Width + Padding, 0);

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
