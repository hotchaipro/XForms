using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Typed bindings to a view model: a two-way text box, a computed greeting,
    /// a two-way toggle, and a command that can run only with a name.
    /// </summary>
    public class BindingPage : Page
    {
        protected override View Build()
        {
            var viewModel = new PersonViewModel();

            var layout = new StackPanel()
            {
                Spacing = 12,
                Padding = new Thickness(20),
            };

            var name = new TextBox()
            {
                PlaceholderText = "Your name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            name.Bind(TextBox.TextProperty, viewModel, vm => vm.Name, (vm, value) => vm.Name = value);
            layout.Children.Add(name);

            var greeting = new TextBlock() { FontSize = 18 };
            greeting.Bind(TextBlock.TextProperty, viewModel, vm => vm.Greeting);
            layout.Children.Add(greeting);

            var length = new TextBlock();
            length.Bind(TextBlock.TextProperty, viewModel, vm => $"{vm.Name?.Length ?? 0} characters");
            length.Bind(TextBlock.ForegroundProperty, viewModel, vm => ((vm.Name?.Length ?? 0) > 10) ? Colors.Red : Colors.Gray);
            layout.Children.Add(length);

            var shout = new ToggleButton()
            {
                Text = "Shout",
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            shout.Bind(ToggleButton.IsCheckedProperty, viewModel, vm => vm.IsShouting, (vm, value) => vm.IsShouting = value);
            layout.Children.Add(shout);

            var save = new Button()
            {
                Text = "Save",
                Width = 120,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                Command = viewModel.SaveCommand,
            };
            layout.Children.Add(save);

            var saved = new TextBlock();
            saved.Bind(TextBlock.TextProperty, viewModel, vm => vm.SavedMessage);
            layout.Children.Add(saved);

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            return page;
        }

        private sealed class PersonViewModel : INotifyPropertyChanged
        {
            private string _name = string.Empty;
            private bool _isShouting;
            private string _savedMessage = "Not saved yet";

            public PersonViewModel()
            {
                this.SaveCommand = new RelayCommand(
                    () => this.SavedMessage = $"Saved \"{this.Name}\"",
                    () => !string.IsNullOrWhiteSpace(this.Name));
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public RelayCommand SaveCommand { get; }

            public string Name
            {
                get => this._name;
                set
                {
                    if (this.Set(ref this._name, value))
                    {
                        this.SaveCommand.NotifyCanExecuteChanged();
                        this.OnPropertyChanged(nameof(this.Greeting));
                    }
                }
            }

            public bool IsShouting
            {
                get => this._isShouting;
                set
                {
                    if (this.Set(ref this._isShouting, value))
                    {
                        this.OnPropertyChanged(nameof(this.Greeting));
                    }
                }
            }

            public string Greeting
            {
                get
                {
                    string text = string.IsNullOrWhiteSpace(this.Name) ? "Hello!" : $"Hello, {this.Name}!";
                    return this.IsShouting ? text.ToUpperInvariant() : text;
                }
            }

            public string SavedMessage
            {
                get => this._savedMessage;
                set => this.Set(ref this._savedMessage, value);
            }

            private bool Set<T>(ref T field, T value, [CallerMemberName] string name = null)
            {
                if (Equals(field, value))
                {
                    return false;
                }

                field = value;
                this.OnPropertyChanged(name);
                return true;
            }

            private void OnPropertyChanged(string name)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
