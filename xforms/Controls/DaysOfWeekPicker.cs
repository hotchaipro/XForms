using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
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

    /// <summary>
    /// A <see cref="Controls.DaysOfWeekPicker"/>'s button for one day. Its
    /// theme's template draws it.
    /// </summary>
    public sealed class DayOfWeekToggleButton : TemplatedControl, ITapGestureDelegate
    {
        private string _text;
        private bool _isChecked;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        internal DayOfWeekToggleButton(
            DaysOfWeekPicker parent)
        {
            if (null == parent)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.DaysOfWeekPicker = parent;

            this.Width = 40;
            this.Height = 40;
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
                    this.OnStateChanged();
                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
            }
        }

        public DaysOfWeekPicker DaysOfWeekPicker
        {
            get;
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
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
