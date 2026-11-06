using System;
using XForma.Layouts;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
{
    public class RadioButtons : ContentControl
    {
        private StyledValue<float> _height = new StyledValue<float>(Dimension.Auto);

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.Height = this._height.Apply(this.Height, theme.RadioButtonStyle.Height);
        }

        public static readonly ViewProperty<RadioButtons, object> SelectedItemProperty = new(nameof(SelectedItem), v => v.SelectedItem, (v, x) => v.SelectedItem = x);

        private DockPanel _buttonsLayout;
        private object? _selectedValue;

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        public RadioButtons()
        {
            this._buttonsLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this.Items = new RadioButtonItemCollection(this);

            this.Content = this._buttonsLayout;
        }

        // User state: the item chosen, when that changed since they were first
        // attached (null is a choice too: none)
        private object? _initialSelectedItem;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialSelectedItem = this.SelectedItem;
        }

        protected override object? GetUserState()
        {
            return Equals(this.SelectedItem, this._initialSelectedItem) ? null : new SelectionState(this.SelectedItem);
        }

        protected override void SetUserState(
            object state)
        {
            if (state is SelectionState selection)
            {
                this.SelectedItem = selection.Item;
            }
        }

        private sealed record SelectionState(object? Item);

        public object? SelectedItem
        {
            get
            {
                return this._selectedValue;
            }

            set
            {
                object? oldSelectedItem = this._selectedValue;
                object? newSelectedItem = value;

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
            RadioButton? selectedButton)
        {
            object? newSelectedItem = selectedButton?.RadioButtonItem.Value;

            this.SelectedItem = newSelectedItem;
        }
    }
}
