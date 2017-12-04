using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    public class DaysOfWeekPicker : ContentControl
    {
        private DistributedStackLayout _buttonsLayout;
        private DayOfWeekToggleButton[] _buttons = new DayOfWeekToggleButton[7];

        public DaysOfWeekPicker()
        {
            this._buttonsLayout = new DistributedStackLayout()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Fill,
                MaximumSize = new Size(320, Dimension.Auto),
            };

            this.Size = new Size(Dimension.Auto, 40);

            this._buttons[(int)DayOfWeek.Monday] = AddButton("M");
            this._buttons[(int)DayOfWeek.Tuesday] = AddButton("T");
            this._buttons[(int)DayOfWeek.Wednesday] = AddButton("W");
            this._buttons[(int)DayOfWeek.Thursday] = AddButton("T");
            this._buttons[(int)DayOfWeek.Friday] = AddButton("F");
            this._buttons[(int)DayOfWeek.Saturday] = AddButton("S");
            this._buttons[(int)DayOfWeek.Sunday] = AddButton("S");

            this.Content = this._buttonsLayout;
        }

        public DayOfWeek[] SelectedDaysOfWeek
        {
            get
            {
                List<DayOfWeek> selectedDaysOfWeek = new List<DayOfWeek>(7);

                foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                {
                    if (this._buttons[(int)dayOfWeek].IsChecked)
                    {
                        selectedDaysOfWeek.Add(dayOfWeek);
                    }
                }

                return selectedDaysOfWeek.ToArray();
            }

            set
            {
                foreach (var button in this._buttons)
                {
                    button.IsChecked = false;
                }

                if ((value == null) || (0 == value.Length))
                {
                    return;
                }

                foreach (DayOfWeek dayOfWeek in value)
                {
                    this._buttons[(int)dayOfWeek].IsChecked = true;
                }
            }
        }

        private DayOfWeekToggleButton AddButton(
            string label)
        {
            var button = new DayOfWeekToggleButton(this)
            {
                Text = label,
            };

            this._buttonsLayout.Children.Add(button);

            return button;
        }
    }

    internal sealed class DayOfWeekToggleButton : UserControl, ITapGestureDelegate
    {
        private const float TouchDownScaleDelta = 0.05f;
        private const float IsUncheckedScale = 0.8f;

        private string _text;
        private TextFormat _textFormat;
        private bool _isChecked;
        private bool _isTouchDown;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        internal DayOfWeekToggleButton(
            DaysOfWeekPicker parent)
        {
            if (null == parent)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.DaysOfWeekPicker = parent;

            this._textFormat = new TextFormat()
            {
                FontSize = 20.0f,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.Center,
            };

            this.Size = new Size(40, 40);
            this.Scale = IsUncheckedScale;

            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.Invalidate();
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }

            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;

                    this.Invalidate();
                    this.UpdateScale();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
            }
        }

        public DaysOfWeekPicker DaysOfWeekPicker
        {
            get;
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            // Calculate text bounds

            float textHeight = bounds.Height;
            Rectangle textBounds = new Rectangle(bounds.X, bounds.Bottom - textHeight, bounds.Width, textHeight);

            // Calculate button bounds

            float buttonDimension = Math.Min(bounds.Width, bounds.Height);
            float buttonRadius = buttonDimension / 2;
            Rectangle buttonBounds = new Rectangle(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - buttonRadius,
                buttonDimension,
                buttonDimension);

            if (this._isTouchDown)
            {
                drawContext.FillEllipse(buttonBounds, this.Theme.SubtleForegroundColor);
                drawContext.DrawText(this._text, textBounds, Colors.White, this._textFormat);
            }
            else if (this.IsChecked)
            {
                drawContext.FillEllipse(buttonBounds, this.Theme.AccentColor);
                drawContext.DrawText(this._text, textBounds, Colors.White, this._textFormat);
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, this.Theme.SubtleForegroundColor, strokeWidth: 1.0f);
                drawContext.DrawText(this._text, textBounds, this.Theme.SubtleForegroundColor, this._textFormat);
            }
        }

        private void UpdateScale()
        {
            if (this._isTouchDown)
            {
                this.ScaleTo(IsUncheckedScale - TouchDownScaleDelta, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked)
            {
                this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                this.ScaleTo(IsUncheckedScale, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseOut));
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._isTouchDown = true;
            this.Invalidate();
            this.UpdateScale();
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._isTouchDown = false;
            this.Invalidate();
            this.UpdateScale();
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
