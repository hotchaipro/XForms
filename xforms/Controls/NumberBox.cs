using System;
using System.Globalization;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// Where a number box's spin buttons are.
    /// </summary>
    public enum NumberBoxSpinButtonPlacementMode
    {
        /// <summary>
        /// None: the number is typed, or stepped with the keyboard.
        /// </summary>
        Hidden,

        /// <summary>
        /// Up and down buttons at the end of the box.
        /// </summary>
        Inline,
    }

    /// <summary>
    /// A number typed in a text box, kept between a minimum and a maximum:
    /// what's typed becomes the value on Enter or when focus leaves (or goes
    /// back to the value, if it isn't a number). Up and Down step it by
    /// <see cref="SmallChange"/>, Page Up and Page Down by
    /// <see cref="LargeChange"/>, and so do its spin buttons. Its theme's
    /// template draws the spin buttons, each <see cref="SpinButtonWidth"/>
    /// wide, at the end of the box.
    /// </summary>
    public class NumberBox : TemplatedControl
    {
        public static readonly ViewProperty<NumberBox, double> ValueProperty = new(nameof(Value), v => v.Value, (v, x) => v.Value = x);

        public const float SpinButtonWidth = 32.0f;

        private TextBox _textBox;
        private double _value = double.NaN;
        private double _minimum = double.MinValue;
        private double _maximum = double.MaxValue;
        private string _placeholderText;
        private NumberBoxSpinButtonPlacementMode _spinButtonPlacementMode;
        private NumberBoxPart _pressedPart;

        public NumberBox()
        {
        }

        /// <summary>
        /// Raised when the value changes.
        /// </summary>
        public event EventHandler<RangeValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// The number, or NaN while the box is empty.
        /// </summary>
        public double Value
        {
            get
            {
                return this._value;
            }

            set
            {
                double coerced = double.IsNaN(value) ? value : Math.Clamp(value, this._minimum, this._maximum);
                if (coerced.Equals(this._value))
                {
                    this.ShowValue();
                    return;
                }

                double oldValue = this._value;
                this._value = coerced;
                this.ShowValue();
                this.OnPropertyChanged();
                this.NotifyAccessibilityStateChanged();
                this.ValueChanged?.Invoke(this, new RangeValueChangedEventArgs(oldValue, coerced));
            }
        }

        public double Minimum
        {
            get
            {
                return this._minimum;
            }

            set
            {
                this._minimum = value;
                this._maximum = Math.Max(this._maximum, value);
                this.Value = this._value;
            }
        }

        public double Maximum
        {
            get
            {
                return this._maximum;
            }

            set
            {
                this._maximum = Math.Max(value, this._minimum);
                this.Value = this._value;
            }
        }

        public double SmallChange
        {
            get;
            set;
        } = 1;

        public double LargeChange
        {
            get;
            set;
        } = 10;

        /// <summary>
        /// Shown while the box is empty; it also names the box for assistive
        /// technology.
        /// </summary>
        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }

            set
            {
                this._placeholderText = value;
                this._textBox?.PlaceholderText = value;
            }
        }

        public NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode
        {
            get
            {
                return this._spinButtonPlacementMode;
            }

            set
            {
                this._spinButtonPlacementMode = value;
                this._textBox?.Margin = this.TextBoxMargin;
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// The spin button being pressed.
        /// </summary>
        public NumberBoxPart PressedPart
        {
            get
            {
                return this._pressedPart;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.SpinButton;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._placeholderText;
            }
        }

        public override AccessibilityRange? AccessibilityRange
        {
            get
            {
                return double.IsNaN(this._value) ? null : new AccessibilityRange(this._minimum, this._maximum, this._value);
            }
        }

        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Increment) || (action == AccessibilityAction.Decrement))
            {
                return this.IsEnabled;
            }

            return base.IsAccessibilityActionSupported(action);
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (this.IsEnabled && (action == AccessibilityAction.Increment))
            {
                this.Step(this.SmallChange);
                return true;
            }

            if (this.IsEnabled && (action == AccessibilityAction.Decrement))
            {
                this.Step(-this.SmallChange);
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        public override bool SetAccessibilityRangeValue(
            double value)
        {
            if (!this.IsEnabled)
            {
                return false;
            }

            this.Value = value;
            return true;
        }

        // User state: the value, when it changed since it was first attached
        // (the text box carries what's typed)
        private double _initialValue;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialValue = this.Value;
        }

        protected override object GetUserState()
        {
            return this.Value.Equals(this._initialValue) ? null : this.Value;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is double value)
            {
                this.Value = value;
            }
        }

        private Thickness TextBoxMargin
        {
            get
            {
                float end = (this._spinButtonPlacementMode == NumberBoxSpinButtonPlacementMode.Inline) ? 2 * SpinButtonWidth : 0;
                return new Thickness(0, 0, end, 0);
            }
        }

        protected override View Build()
        {
            this._textBox = new TextBox()
            {
                PlaceholderText = this._placeholderText,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = this.TextBoxMargin,
                KeyboardOptions = KeyboardOptions.None,
            };
            this._textBox.Submitted += (sender, e) => this.Commit();
            this._textBox.LostFocus += (sender, e) => this.Commit();
            this.ShowValue();
            return this._textBox;
        }

        private void ShowValue()
        {
            if (null != this._textBox)
            {
                this._textBox.Text = double.IsNaN(this._value) ? string.Empty : this._value.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Makes what's typed the value: empty is none (NaN), and anything
        /// that isn't a number goes back to the value.
        /// </summary>
        private void Commit()
        {
            string text = this._textBox?.Text?.Trim() ?? string.Empty;
            if (text.Length == 0)
            {
                this.Value = double.NaN;
            }
            else if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double value))
            {
                this.Value = value;
            }
            else
            {
                this.ShowValue();
            }
        }

        private void Step(
            double change)
        {
            this.Commit();
            double value = double.IsNaN(this._value) ? Math.Clamp(0, this._minimum, this._maximum) : this._value + change;
            this.Value = value;
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// Where the up spin button is: the first of the two, at the end.
        /// </summary>
        public Rect UpButtonBounds
        {
            get
            {
                float x = this.IsRightToLeft ? SpinButtonWidth : this.Bounds.Width - (2 * SpinButtonWidth);
                return new Rect(x, 0, SpinButtonWidth, this.Bounds.Height);
            }
        }

        /// <summary>
        /// Where the down spin button is: the last.
        /// </summary>
        public Rect DownButtonBounds
        {
            get
            {
                float x = this.IsRightToLeft ? 0 : this.Bounds.Width - SpinButtonWidth;
                return new Rect(x, 0, SpinButtonWidth, this.Bounds.Height);
            }
        }

        private NumberBoxPart PartAt(
            Point point)
        {
            if (this._spinButtonPlacementMode != NumberBoxSpinButtonPlacementMode.Inline)
            {
                return NumberBoxPart.None;
            }

            if (Contains(this.UpButtonBounds, point))
            {
                return NumberBoxPart.UpButton;
            }

            if (Contains(this.DownButtonBounds, point))
            {
                return NumberBoxPart.DownButton;
            }

            return NumberBoxPart.None;
        }

        private static bool Contains(
            Rect rectangle,
            Point point)
        {
            return (point.X >= rectangle.X) && (point.Y >= rectangle.Y) && (point.X < rectangle.Right) && (point.Y < rectangle.Bottom);
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this._pressedPart = this.PartAt(e.Position);
            this.OnStateChanged();
            e.Handled = (this._pressedPart != NumberBoxPart.None);
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            var part = this.PartAt(e.Position);
            bool isSame = (part != NumberBoxPart.None) && (part == this._pressedPart);
            this._pressedPart = NumberBoxPart.None;
            this.OnStateChanged();
            e.Handled = true;

            if (isSame)
            {
                this.Step((part == NumberBoxPart.UpButton) ? this.SmallChange : -this.SmallChange);
            }
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pressedPart = NumberBoxPart.None;
            this.OnStateChanged();
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Step(this.SmallChange);
                    break;

                case Key.Down:
                    this.Step(-this.SmallChange);
                    break;

                case Key.PageUp:
                    this.Step(this.LargeChange);
                    break;

                case Key.PageDown:
                    this.Step(-this.LargeChange);
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }
    }

    /// <summary>
    /// A part of a number box.
    /// </summary>
    public enum NumberBoxPart
    {
        None,
        UpButton,
        DownButton,
    }
}
