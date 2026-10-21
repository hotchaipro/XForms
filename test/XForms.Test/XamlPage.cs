using System;
using System.ComponentModel;
using System.Windows.Input;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;
using XForms.Xaml;

namespace XForms.Test
{
    /// <summary>
    /// A page written in XAML, loaded by the XForms.Xaml proof-of-concept spike.
    /// </summary>
    public class XamlPage : Page
    {
        private const string Markup = @"
<Page xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <Page.Resources>
    <Color x:Key='Accent'>#FF2196F3</Color>
    <Style TargetType='TextBlock'>
      <Setter Property='FontSize' Value='16' />
    </Style>
    <Style x:Key='Header' TargetType='TextBlock'>
      <Setter Property='FontSize' Value='26' />
      <Setter Property='FontWeight' Value='SemiBold' />
    </Style>
    <ControlTemplate x:Key='PillButton'>
      <Border Background='{TemplateBinding Background}' CornerRadius='20' Padding='24,10'>
        <ContentPresenter />
      </Border>
    </ControlTemplate>
    <Storyboard x:Key='Loaded'>
      <DoubleAnimation TargetName='Card' Property='Opacity' From='0' To='1' Duration='0:0:0.6' />
      <DoubleAnimation TargetName='Card' Property='TranslateY' From='40' To='0' Duration='0:0:0.6' />
    </Storyboard>
  </Page.Resources>

  <StackPanel Margin='24' Spacing='16'>
    <TextBlock Style='{StaticResource Header}' Text='{Binding Title}' />
    <Border x:Name='Card' Background='#FFF3F3F3' BorderBrush='#FFDDDDDD' BorderThickness='1' CornerRadius='8' Padding='16'>
      <Grid ColumnDefinitions='Auto,*' RowDefinitions='Auto,Auto,Auto'>
        <TextBlock Grid.Row='0' Grid.Column='0' Text='First name' Margin='0,0,12,8' />
        <TextBox Grid.Row='0' Grid.Column='1' PlaceholderText='Ada' Text='{Binding FirstName, Mode=TwoWay}' Margin='0,0,0,8' />
        <TextBlock Grid.Row='1' Grid.Column='0' Text='Last name' Margin='0,0,12,8' />
        <TextBox Grid.Row='1' Grid.Column='1' PlaceholderText='Lovelace' Text='{Binding LastName, Mode=TwoWay}' Margin='0,0,0,8' />
        <TextBlock Grid.Row='2' Grid.Column='1' Text='{Binding Greeting}' Foreground='{StaticResource Accent}' />
      </Grid>
    </Border>
    <StackPanel Orientation='Horizontal' Spacing='12'>
      <Button Template='{StaticResource PillButton}' Background='{StaticResource Accent}' Foreground='#FFFFFFFF' Command='{Binding GreetCommand}'>Greet</Button>
      <Button Template='{StaticResource PillButton}' Background='#FF9E9E9E' Foreground='#FFFFFFFF' Command='{Binding ResetCommand}'>Reset</Button>
    </StackPanel>
  </StackPanel>
</Page>";

        public XamlPage()
        {
            var viewModel = new GreetingViewModel();
            var view = XamlLoader.Load(Markup, viewModel);

            var layout = new DockPanel();
            layout.Children.Add(view.Root, Dock.CenterOverlay);
            this.Content = layout;

            // GAP: Views have no "attached" or "loaded" event. WORKAROUND: start
            // the storyboard once the UI thread has finished building the page.
            if (view.Storyboards.TryGetValue("Loaded", out var storyboard))
            {
                this.Application.Dispatcher.Post(storyboard.Begin);
            }
        }

        private sealed class GreetingViewModel : INotifyPropertyChanged
        {
            private string _greeting = "Press Greet";

            public GreetingViewModel()
            {
                this.GreetCommand = new DelegateCommand(() => this.Greeting = $"Hello, {this.FirstName} {this.LastName}!");
                this.ResetCommand = new DelegateCommand(() => this.Greeting = "Press Greet");
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public string Title => "XAML on XForms";

            public string FirstName { get; set; } = "Ada";

            public string LastName { get; set; } = "Lovelace";

            public string Greeting
            {
                get => this._greeting;
                set
                {
                    this._greeting = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Greeting)));
                }
            }

            public ICommand GreetCommand { get; }

            public ICommand ResetCommand { get; }
        }

        private sealed class DelegateCommand : ICommand
        {
            private readonly Action _execute;

            public DelegateCommand(Action execute)
            {
                this._execute = execute;
            }

            public event EventHandler CanExecuteChanged { add { } remove { } }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) => this._execute();
        }
    }
}
