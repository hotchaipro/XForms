using System;

namespace XForms.Controls
{
    /// <summary>
    /// The old and new values of a range control.
    /// </summary>
    public sealed class RangeValueChangedEventArgs : EventArgs
    {
        public RangeValueChangedEventArgs(
            double oldValue,
            double newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public double OldValue
        {
            get;
        }

        public double NewValue
        {
            get;
        }
    }

    /// <summary>
    /// Base for controls showing a value within a range (a slider, a progress
    /// bar): the value is kept between the minimum and the maximum.
    /// </summary>
    public abstract class RangeBase : TemplatedControl
    {
        private double _minimum;
        private double _maximum = 100;
        private double _value;

        protected RangeBase()
        {
        }

        public event EventHandler<RangeValueChangedEventArgs> ValueChanged;

        public double Minimum
        {
            get
            {
                return this._minimum;
            }

            set
            {
                this._minimum = value;
                if (this._maximum < value)
                {
                    this._maximum = value;
                }

                this.Value = this._value;
                this.InvalidateVisual();
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
                this.InvalidateVisual();
            }
        }

        public double Value
        {
            get
            {
                return this._value;
            }

            set
            {
                double coerced = Math.Clamp(this.CoerceValue(value), this._minimum, this._maximum);
                if (coerced == this._value)
                {
                    return;
                }

                double oldValue = this._value;
                this._value = coerced;
                this.OnStateChanged();
                this.OnPropertyChanged(nameof(this.Value));
                this.NotifyAccessibilityStateChanged();
                this.ValueChanged?.Invoke(this, new RangeValueChangedEventArgs(oldValue, coerced));
            }
        }

        /// <summary>
        /// How far the value is through the range, from 0 to 1.
        /// </summary>
        public double Fraction
        {
            get
            {
                double span = this._maximum - this._minimum;
                return (span > 0) ? (this._value - this._minimum) / span : 0;
            }
        }

        /// <summary>
        /// Adjusts a value before it's kept in the range (a slider snaps it to
        /// its steps).
        /// </summary>
        protected virtual double CoerceValue(
            double value)
        {
            return value;
        }

        public override AccessibilityRange? AccessibilityRange
        {
            get
            {
                return new AccessibilityRange(this._minimum, this._maximum, this._value);
            }
        }
    }
}
