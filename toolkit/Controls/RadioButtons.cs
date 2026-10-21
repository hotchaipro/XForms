using System;
using XForms.Layouts;
using XForms.Controls;

namespace XForms.Toolkit
{
    public class RadioButtons : ContentControl
    {
        public static readonly ViewProperty<RadioButtons, object> SelectedItemProperty = new(nameof(SelectedItem), v => v.SelectedItem, (v, x) => v.SelectedItem = x);

        private DockPanel _buttonsLayout;
        private object _selectedValue;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public RadioButtons()
        {
            this._buttonsLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this.Height = 60;

            this.Items = new RadioButtonItemCollection(this);

            this.Content = this._buttonsLayout;
        }

        public object SelectedItem
        {
            get
            {
                return this._selectedValue;
            }

            set
            {
                object oldSelectedItem = this._selectedValue;
                object newSelectedItem = value;

                if (Equals(oldSelectedItem, newSelectedItem))
                {
                    return;
                }

                // Update the state of the buttons in the group
                bool isValidValue = false;
                foreach (RadioButton button in this._buttonsLayout.Children)
                {
                    if (Equals(button.RadioButtonItem.Value, newSelectedItem))
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
                    this._selectedValue = newSelectedItem;

                    if (!Equals(newSelectedItem, oldSelectedItem))
                    {
                        this.SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldSelectedItem, newSelectedItem));
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(SelectedItem));
                }

                this.OnPropertyChanged();
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
            radioButton.Command = new RelayCommand((o) => OnButtonClicked(o as RadioButton));
            this._buttonsLayout.Children.Add(radioButton, Dock.Left);
        }

        private void OnButtonClicked(
            RadioButton selectedButton)
        {
            object newSelectedItem = selectedButton?.RadioButtonItem.Value;

            this.SelectedItem = newSelectedItem;
        }
    }
}
