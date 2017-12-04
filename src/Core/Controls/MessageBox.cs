using System;
using System.Threading.Tasks;
using XForms.Layouts;

namespace XForms.Controls
{
    public enum MessageBoxResult
    {
        Cancel = 0,
        OK = 1,
    }

    public class MessageBox : Popover
    {
        private TaskCompletionSource<MessageBoxResult> _task;
        private string _message, _title;
        private MessageBoxResult _result;
        private TextView _messageView;

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

            var contentLayout = new DockLayout()
            {
                MaximumSize = new Size(300f, Dimension.Auto),
            };

            this._messageView = new TextView()
            {
                Text = this._message,
                ForegroundColor = Colors.White,
                FontSize = 14f,
                WordWrap = true,
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Fill,
                Margin = new Thickness(20),
            };
            contentLayout.Children.Add(this._messageView, DockRegion.Top);

            var buttonsLayout = new DistributedStackLayout()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.End,
            };
            contentLayout.Children.Add(buttonsLayout, DockRegion.Top);

            var cancelButton = new Button()
            {
                Text = "Cancel",
                IsAlertStyle = true,
                Size = new Size(100, 30),
                Margin = new Thickness(5),
                HorizontalAlignment = LayoutAlignment.Center,
            };
            cancelButton.Clicked += CancelButton_Clicked;
            buttonsLayout.Children.Add(cancelButton);

            var okButton = new Button()
            {
                Text = "Ok",
                IsAlertStyle = true,
                Size = new Size(100, 30),
                Margin = new Thickness(5),
                HorizontalAlignment = LayoutAlignment.Center,
            };
            okButton.Clicked += OkButton_Clicked;
            buttonsLayout.Children.Add(okButton);

            this.Content = contentLayout;

            this.OnApplyTheme(this.Application.Theme); // HACK
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

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            // HACK
            if (null == this._message)
            {
                return;
            }

            base.OnApplyTheme(theme);

            if (theme == AppTheme.Dark)
            {
                this._messageView.ForegroundColor = Colors.White;
            }
            else
            {
                this._messageView.ForegroundColor = Colors.Black;
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            this._task?.SetResult(this._result);
        }
    }
}
