using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class Button : UserControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<Button, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);
        public static readonly ViewProperty<Button, bool> IsEnabledProperty = new(nameof(IsEnabled), v => v.IsEnabled, (v, x) => v.IsEnabled = x);
        public static readonly ViewProperty<Button, ICommand> CommandProperty = new(nameof(Command), v => v.Command, (v, x) => v.Command = x);

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

        private const float TouchDownScale = 0.9f;

        private string _text;
        private bool _isEnabled = true;
        private bool _isAlert;
        private ButtonStyle _style;

        private ICommand _command;

        public event EventHandler Clicked;

        public Button()
        {
            this._ownAnimator = this.CreateDrawnAnimator();

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
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

        /// <summary>
        /// The button's own style, or null (the default) for the theme's.
        /// </summary>
        public ButtonStyle Style
        {
            get
            {
                return this._style;
            }

            set
            {
                this._style = value;
                this.InvalidateVisual();
                this.OnPropertyChanged();
            }
        }

        // The button draws its own rounded fill
        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        // The style in effect: the button's own, over the theme's, over the
        // theme's tokens
        private ButtonStyle ActualStyle
        {
            get
            {
                var theme = this.ActualTheme;
                var colors = theme.Colors;
                var defaults = new ButtonStyle()
                {
                    Background = this.IsAlertStyle ? colors.Error : colors.Accent,
                    Foreground = this.IsAlertStyle ? colors.OnError : colors.OnAccent,
                    CornerRadius = theme.Shapes.SmallCornerRadius,
                    TextStyle = theme.Typography.Label,
                };

                return (this._style ?? new ButtonStyle()).Over(theme.GetStyle<ButtonStyle>()).Over(defaults);
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            var style = this.ActualStyle;
            var format = style.TextStyle.ToTextFormat();
            format.HorizontalAlignment = HorizontalAlignment.Center;
            format.VerticalAlignment = VerticalAlignment.Center;

            drawContext.Clear(Colors.Transparent);
            if (this.IsEnabled)
            {
                drawContext.FillRoundedRectangle(bounds, style.CornerRadius.Value, new SolidColorBrush(style.Background.Value));
                drawContext.DrawText(this.Text, bounds, style.Foreground.Value, format);
            }
            else
            {
                // A disabled button is its text alone, dimmed
                drawContext.DrawText(this.Text, bounds, this.ActualTheme.Colors.Disabled, format);
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._ownAnimator.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
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
