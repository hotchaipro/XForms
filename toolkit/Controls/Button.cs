using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;

namespace XForms.Toolkit
{
    public class Button : UserControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<Button, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);
        public static readonly ViewProperty<Button, bool> IsEnabledProperty = new(nameof(IsEnabled), v => v.IsEnabled, (v, x) => v.IsEnabled = x);
        public static readonly ViewProperty<Button, ICommand> CommandProperty = new(nameof(Command), v => v.Command, (v, x) => v.Command = x);

        // Animates the control itself (press feedback, spinning)
        private readonly Animator _ownAnimator;

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

        private const float TouchDownScale = 0.9f;

        private TextFormat _textFormat;
        private string _text;
        private bool _isEnabled = true;
        private bool _isAlert;

        private ICommand _command;

        public event EventHandler Clicked;

        public Button()
        {
            this._ownAnimator = this.CreateOwnAnimator();

            this._textFormat = new TextFormat()
            {
                FontSize = 14.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                this._isEnabled = value;

                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The command the button runs when clicked. The button is enabled
        /// while the command can run.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                if (null != this._command)
                {
                    this._command.CanExecuteChanged -= this.Command_CanExecuteChanged;
                }

                this._command = value;

                if (null != value)
                {
                    value.CanExecuteChanged += this.Command_CanExecuteChanged;
                }

                this.UpdateCanExecute();
                this.OnPropertyChanged();
            }
        }

        private object _commandParameter;

        /// <summary>
        /// The value passed to the command.
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
                this.UpdateCanExecute();
                this.OnPropertyChanged();
            }
        }

        private void Command_CanExecuteChanged(
            object sender,
            EventArgs e)
        {
            this.UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            if (null != this._command)
            {
                this.IsEnabled = this._command.CanExecute(this._commandParameter);
            }
        }

        public bool IsAlertStyle
        {
            get
            {
                return this._isAlert;
            }

            set
            {
                this._isAlert = value;

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

                this.OnPropertyChanged();
            }
        }

        private Color? ForegroundColor
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
                if (this.IsAlertStyle)
                {
                    return Colors.Red;
                }
                else
                {
                    return this.Application.Theme.AccentColor;
                }
            }
        }

        private Color? DisabledColor
        {
            get
            {
                return this.Application.Theme.SubtleForegroundColor;
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            Rect textBounds = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            if (this.IsEnabled)
            {
                base.Render(drawContext);

                var foregroundColor = this.ForegroundColor;
                if (foregroundColor.HasValue)
                {
                    drawContext.DrawText(this.Text, textBounds, foregroundColor.Value, this._textFormat);
                }
            }
            else
            {
                drawContext.Clear(Colors.Transparent);

                var foregroundColor = this.DisabledColor;
                if (foregroundColor.HasValue)
                {
                    drawContext.DrawText(this.Text, textBounds, foregroundColor.Value, this._textFormat);
                }
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._ownAnimator.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._ownAnimator.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            if (this.IsEnabled)
            {
                this.Clicked?.Invoke(this, EventArgs.Empty);

                if ((null != this._command) && this._command.CanExecute(this._commandParameter))
                {
                    this._command.Execute(this._commandParameter);
                }
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
