using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Toolkit
{
    public class DaysOfWeekPicker : ContentControl
    {
        private UniformGrid _buttonsLayout;
        private DayOfWeekToggleButton[] _buttons = new DayOfWeekToggleButton[7];

        public DaysOfWeekPicker()
        {
            this._buttonsLayout = new UniformGrid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MaxWidth = 320,
            };

            this.Height = 40;

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
        // Animates the control itself (press feedback, spinning)
        private readonly DrawnAnimator _ownAnimator;

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
            this._ownAnimator = this.CreateDrawnAnimator();

            if (null == parent)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.DaysOfWeekPicker = parent;

            this._textFormat = new TextFormat()
            {
                FontSize = 20.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.Width = 40;
            this.Height = 40;
            this._ownAnimator.Scale = IsUncheckedScale;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
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
                    this.InvalidateVisual();
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

                    this.InvalidateVisual();
                    this.UpdateScale();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
            }
        }

        public DaysOfWeekPicker DaysOfWeekPicker
        {
            get;
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            // Calculate text bounds

            float textHeight = bounds.Height;
            Rect textBounds = new Rect(bounds.X, bounds.Bottom - textHeight, bounds.Width, textHeight);

            // Calculate button bounds

            float buttonDimension = Math.Min(bounds.Width, bounds.Height);
            float buttonRadius = buttonDimension / 2;
            Rect buttonBounds = new Rect(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - buttonRadius,
                buttonDimension,
                buttonDimension);

            if (this._isTouchDown)
            {
                drawContext.FillEllipse(buttonBounds, this.ActualTheme.Colors.SubtleForeground);
                drawContext.DrawText(this._text, textBounds, Colors.White, this._textFormat);
            }
            else if (this.IsChecked)
            {
                drawContext.FillEllipse(buttonBounds, this.ActualTheme.Colors.Accent);
                drawContext.DrawText(this._text, textBounds, Colors.White, this._textFormat);
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, this.ActualTheme.Colors.SubtleForeground, strokeWidth: 1.0f);
                drawContext.DrawText(this._text, textBounds, this.ActualTheme.Colors.SubtleForeground, this._textFormat);
            }
        }

        private void UpdateScale()
        {
            if (this._isTouchDown)
            {
                this._ownAnimator.ScaleTo(IsUncheckedScale - TouchDownScaleDelta, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked)
            {
                this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                this._ownAnimator.ScaleTo(IsUncheckedScale, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseOut));
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._isTouchDown = true;
            this.InvalidateVisual();
            this.UpdateScale();
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._isTouchDown = false;
            this.InvalidateVisual();
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
