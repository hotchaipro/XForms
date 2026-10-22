using System;
using System.Threading.Tasks;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public enum MessageBoxResult
    {
        Cancel = 0,
        OK = 1,
    }

    /// <summary>
    /// A flyout asking the user to confirm something, with OK and Cancel.
    /// </summary>
    public class MessageBox : Flyout
    {
        private TaskCompletionSource<MessageBoxResult> _task;
        private MessageBoxResult _result;

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

            this._result = MessageBoxResult.Cancel;
            this.Content = new MessageBoxView(this, message, title);
        }

        internal void Choose(
            MessageBoxResult result)
        {
            this._result = result;
            this.Close();
        }

        private async Task<MessageBoxResult> ShowMessageBoxAsync()
        {
            this._task = new TaskCompletionSource<MessageBoxResult>();

            base.ShowAsync();

            var result = await this._task.Task;

            return result;
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            this._task?.SetResult(this._result);
        }
    }

    /// <summary>
    /// A <see cref="MessageBox"/>'s content: its message, and the choices. Its
    /// theme's template lays it out, with buttons that call
    /// <see cref="Choose"/>.
    /// </summary>
    public sealed class MessageBoxView : TemplatedContentControl
    {
        private readonly MessageBox _messageBox;

        internal MessageBoxView(
            MessageBox messageBox,
            string message,
            string title)
        {
            this._messageBox = messageBox;
            this.Message = message;
            this.Title = title;
        }

        public string Message
        {
            get;
        }

        public string Title
        {
            get;
        }

        /// <summary>
        /// Closes the message box with the user's choice.
        /// </summary>
        public void Choose(
            MessageBoxResult result)
        {
            this._messageBox.Choose(result);
        }
    }
}
