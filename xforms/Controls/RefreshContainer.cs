using System;
using System.Threading.Tasks;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A refresh the user asked for. Take a deferral to finish it later (the
    /// indicator spins until it's completed); otherwise it ends when the
    /// handlers return.
    /// </summary>
    public sealed class RefreshRequestedEventArgs : EventArgs
    {
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
        private int _deferrals = 1;

        internal RefreshRequestedEventArgs()
        {
        }

        internal Task Completion
        {
            get
            {
                return this._completion.Task;
            }
        }

        /// <summary>
        /// Keeps the refresh going until the returned action is called.
        /// </summary>
        public Action GetDeferral()
        {
            this._deferrals += 1;
            bool isDone = false;
            return () =>
            {
                if (!isDone)
                {
                    isDone = true;
                    this.Release();
                }
            };
        }

        internal void Release()
        {
            this._deferrals -= 1;
            if (this._deferrals <= 0)
            {
                this._completion.TrySetResult(true);
            }
        }
    }

    /// <summary>
    /// Pull to refresh, around a list (an <see cref="ItemsView"/>, such as a
    /// <see cref="ListView"/>): pulling it down past its top shows the
    /// platform's refresh indicator and raises <see cref="RefreshRequested"/>,
    /// and the indicator spins until the app's done.
    /// <see cref="RequestRefresh"/> starts one from the app (a button, a key),
    /// around any content.
    /// </summary>
    public class RefreshContainer : TemplatedControl
    {
        private View _content;
        private bool _isRefreshing;

        public RefreshContainer()
        {
        }

        /// <summary>
        /// Raised when the user pulls to refresh, or the app asks
        /// (<see cref="RequestRefresh"/>).
        /// </summary>
        public event EventHandler<RefreshRequestedEventArgs> RefreshRequested;

        /// <summary>
        /// The list inside (or any view, refreshed only by
        /// <see cref="RequestRefresh"/>).
        /// </summary>
        public new View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                if (this._content is ItemsView oldList)
                {
                    oldList.IsRefreshEnabled = false;
                    oldList.RefreshRequested -= this.List_RefreshRequested;
                }

                this._content = value;
                base.Content = value;

                if (value is ItemsView list)
                {
                    list.IsRefreshEnabled = true;
                    list.RefreshRequested += this.List_RefreshRequested;
                }
            }
        }

        /// <summary>
        /// Whether a refresh is going on.
        /// </summary>
        public bool IsRefreshing
        {
            get
            {
                return this._isRefreshing;
            }
        }

        /// <summary>
        /// Starts a refresh, as pulling would (with the list's indicator).
        /// </summary>
        public void RequestRefresh()
        {
            if (this._isRefreshing)
            {
                return;
            }

            if (this._content is ItemsView list)
            {
                list.RequestRefresh();
            }
            else
            {
                this.Refresh();
            }
        }

        private void List_RefreshRequested(
            object sender,
            EventArgs e)
        {
            this.Refresh();
        }

        private async void Refresh()
        {
            this._isRefreshing = true;
            this.OnPropertyChanged(nameof(this.IsRefreshing));

            var args = new RefreshRequestedEventArgs();
            try
            {
                this.RefreshRequested?.Invoke(this, args);
            }
            finally
            {
                args.Release();
            }

            await args.Completion;
            this._isRefreshing = false;
            if (this._content is ItemsView list)
            {
                list.IsRefreshing = false;
            }

            this.OnPropertyChanged(nameof(this.IsRefreshing));
        }
    }
}
