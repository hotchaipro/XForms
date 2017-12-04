using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public class RadioButtonPicker : ContentControl
    {
        private DockLayout _buttonsLayout;
        private object _selectedValue;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public RadioButtonPicker()
        {
            this._buttonsLayout = new DockLayout()
            {
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.Fill,
            };

            this.Size = new Size(Dimension.Auto, 60);

            this.Items = new RadioButtonItemCollection(this);

            this.Content = this._buttonsLayout;
        }

        public object SelectedValue
        {
            get
            {
                return this._selectedValue;
            }

            set
            {
                object oldSelectedValue = this._selectedValue;
                object newSelectedValue = value;

                if (oldSelectedValue == newSelectedValue)
                {
                    return;
                }

                // Update the state of the buttons in the group
                bool isValidValue = false;
                foreach (RadioButton button in this._buttonsLayout.Children)
                {
                    if (button.RadioButtonItem.Value == newSelectedValue)
                    {
                        button.IsChecked = true;
                        isValidValue = true;
                    }
                    else
                    {
                        button.IsChecked = false;
                    }
                }

                if (isValidValue)
                {
                    this._selectedValue = newSelectedValue;

                    if (newSelectedValue != oldSelectedValue)
                    {
                        this.SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldSelectedValue, newSelectedValue));
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(SelectedValue));
                }
            }
        }

        public RadioButtonItemCollection Items
        {
            get;
        }

        internal void Add(
            RadioButtonItem radioButtonItem)
        {
            if (null == radioButtonItem)
            {
                throw new ArgumentNullException(nameof(radioButtonItem));
            }

            var radioButton = new RadioButton(this, radioButtonItem);
            radioButton.Command = new Command((o) => OnButtonClicked(o as RadioButton));
            this._buttonsLayout.Children.Add(radioButton, DockRegion.Left);
        }

        private void OnButtonClicked(
            RadioButton selectedButton)
        {
            object newSelectedValue = selectedButton?.RadioButtonItem.Value;

            this.SelectedValue = newSelectedValue;
        }
    }
}
