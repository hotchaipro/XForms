using System;
using System.Collections.Generic;
using System.Windows.Input;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    internal sealed class CommandBarButtonCollection
    {
        private CommandBar _commandBar;
        private List<CommandBarButton> _buttons;
        private Dock _buttonRegion;

        internal CommandBarButtonCollection(
            CommandBar commandBar,
            Dock buttonRegion)
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

            button.Width = 48f;
            button.Height = 48f;

            this._commandBar.ButtonsLayout.Children.Add(button, this._buttonRegion);
        }
    }

    internal sealed class CommandBarButton : UserControl, ITapGestureDelegate
    {
        // Animates the control itself (press feedback, spinning)
        private readonly DrawnAnimator _ownAnimator;

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Text;
            }
        }

        private const float PressedScale = 0.9f;

        private string _text;
        private TextFormat _textFormat;
        private Bitmap _icon;

        public CommandBarButton()
        {
            this._ownAnimator = this.CreateDrawnAnimator();

            this._textFormat = new TextFormat()
            {
                FontSize = 9.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            this._ownAnimator.Scale = 1.0f;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public ICommand Command
        {
            get;
            set;
        }

        private object _commandParameter;

        /// <summary>
        /// The value passed to the command. The button shows only while the
        /// command can run with it.
        /// </summary>
        public object CommandParameter
        {
            get
            {
                return this._commandParameter;
            }

            set
            {
                this._commandParameter = value;

                if (null != this.Command)
                {
                    this.IsVisible = this.Command.CanExecute(value);
                }
            }
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
            }
        }

        private Color AccentColor
        {
            get
            {
                return this.ActualTheme.Colors.Accent;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }



        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;

            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            drawContext.DrawImage(this._icon, imageBounds, this.AccentColor);

            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);

            drawContext.DrawText(this.Text, textBounds, this.AccentColor, this._textFormat);
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._ownAnimator.ScaleTo(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.Command?.Execute(this.CommandParameter);
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            if (this._ownAnimator.Scale != 1.0f)
            {
                this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));
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
