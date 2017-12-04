using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    internal sealed class CommandBarButtonCollection
    {
        private CommandBar _commandBar;
        private List<CommandBarButton> _buttons;
        private DockRegion _buttonRegion;

        internal CommandBarButtonCollection(
            CommandBar commandBar,
            DockRegion buttonRegion)
        {
            if (null == commandBar)
            {
                throw new ArgumentNullException(nameof(commandBar));
            }

            this._commandBar = commandBar;
            this._buttons = new List<CommandBarButton>();
            this._buttonRegion = buttonRegion;
        }

        public int Count
        {
            get
            {
                return this._buttons.Count;
            }
        }

        public void Add(
            CommandBarButton button)
        {
            if (null == button)
            {
                throw new ArgumentNullException(nameof(button));
            }

            button.Size = new Size(48f, 48f);

            this._commandBar.ButtonsLayout.Children.Add(button, this._buttonRegion);
        }
    }

    internal sealed class CommandBarButton : UserControl, ITapGestureDelegate
    {
        private const float PressedScale = 0.9f;

        private string _text;
        private TextFormat _textFormat;
        private Bitmap _icon;

        public CommandBarButton()
        {
            this._textFormat = new TextFormat()
            {
                FontSize = 9.0f,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.End,
            };

            this.Scale = 1.0f;

            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public ICommand Command
        {
            get;
            set;
        }

        public Bitmap Icon
        {
            get
            {
                return this._icon;
            }

            set
            {
                this._icon = value;

                this.Invalidate();
            }
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;

                this.Invalidate();
            }
        }

        private Color AccentColor
        {
            get
            {
                return this.Application.Theme.AccentColor;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (null != this.Command)
            {
                this.IsVisible = this.Command.CanExecute(this.BindingContext);
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;

            Rectangle imageBounds = new Rectangle(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            drawContext.DrawImage(this._icon, imageBounds, this.AccentColor);

            Rectangle textBounds = new Rectangle(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);

            drawContext.DrawText(this.Text, textBounds, this.AccentColor, this._textFormat);
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.ScaleTo(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.Command?.Execute(this.BindingContext);
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            if (this.Scale != 1.0f)
            {
                this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new SpringEase(EasingMode.EaseOut));
            }
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion
    }
}
