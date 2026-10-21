using System;
using System.Threading.Tasks;
using XForms.Layouts;
using XForms.Controls;

namespace XForms.Toolkit
{
    public enum MessageBoxResult
    {
        Cancel = 0,
        OK = 1,
    }

    public class MessageBox : Flyout
    {
        private TaskCompletionSource<MessageBoxResult> _task;
        private string _message, _title;
        private MessageBoxResult _result;
        private TextBlock _messageView;

        public static async Task<MessageBoxResult> ShowAsync(
            string message,
            string title)
        {
            var messageBox = new MessageBox(message, title);
            var result = await messageBox.ShowMessageBoxAsync();
            return result;
        }

        private MessageBox(
            string message,
            string title)
        {
            if (null == message)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (null == title)
            {
                throw new ArgumentNullException(nameof(title));
            }

            this._message = message;
            this._title = title;
            this._result = MessageBoxResult.Cancel;

            var contentLayout = new DockPanel()
            {
                MaxWidth = 300f,
            };

            this._messageView = new TextBlock()
            {
                Text = this._message,
                Foreground = Colors.White,
                FontSize = 14f,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(20),
            };
            contentLayout.Children.Add(this._messageView, Dock.Top);

            var buttonsLayout = new UniformGrid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            contentLayout.Children.Add(buttonsLayout, Dock.Top);

            var cancelButton = new Button()
            {
                Text = "Cancel",
                IsAlertStyle = true,
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            cancelButton.Clicked += CancelButton_Clicked;
            buttonsLayout.Children.Add(cancelButton);

            var okButton = new Button()
            {
                Text = "Ok",
                IsAlertStyle = true,
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            okButton.Clicked += OkButton_Clicked;
            buttonsLayout.Children.Add(okButton);

            this.Content = contentLayout;
        }

        private void CancelButton_Clicked(
            object sender,
            EventArgs e)
        {
            this._result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void OkButton_Clicked(
            object sender,
            EventArgs e)
        {
            this._result = MessageBoxResult.OK;
            this.Close();
        }

        private async Task<MessageBoxResult> ShowMessageBoxAsync()
        {
            this._task = new TaskCompletionSource<MessageBoxResult>();

            base.ShowAsync();

            var result = await this._task.Task;

            return result;
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this._messageView.Foreground = theme.Colors.OnSurface;
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            this._task?.SetResult(this._result);
        }
    }
}
