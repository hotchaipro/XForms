using System;
using System.Threading;
using System.Threading.Tasks;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// Runs code on the UI thread: one queue, with no priorities. From the UI
    /// thread, <c>await</c> already returns to it.
    /// </summary>
    public sealed class Dispatcher
    {
        private readonly IApplicationRenderer _renderer;
        private readonly int _threadId;

        internal Dispatcher(
            IApplicationRenderer renderer)
        {
            this._renderer = renderer;
            this._threadId = Environment.CurrentManagedThreadId;
        }

        /// <summary>
        /// Whether the calling code is on the UI thread.
        /// </summary>
        public bool CheckAccess()
        {
            return Environment.CurrentManagedThreadId == this._threadId;
        }

        /// <summary>
        /// Queues the action to run on the UI thread.
        /// </summary>
        public void Post(
            Action action)
        {
            if (null == action)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this._renderer.InvokeOnUIThreadAsync(action);
        }

        /// <summary>
        /// Runs the action on the UI thread, and completes when it has (with
        /// its exception, if it throws).
        /// </summary>
        public Task InvokeAsync(
            Action action)
        {
            return this.InvokeAsync(() =>
            {
                action();
                return true;
            });
        }

        /// <summary>
        /// Runs the function on the UI thread, and completes with its result.
        /// </summary>
        public Task<TResult> InvokeAsync<TResult>(
            Func<TResult> function)
        {
            if (null == function)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var completion = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.Post(() =>
            {
                try
                {
                    completion.TrySetResult(function());
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            });

            return completion.Task;
        }

        /// <summary>
        /// Runs the asynchronous function on the UI thread, and completes when
        /// it does.
        /// </summary>
        public Task InvokeAsync(
            Func<Task> function)
        {
            if (null == function)
            {
                throw new ArgumentNullException(nameof(function));
            }

            return this.InvokeAsync<Task>(function).Unwrap();
        }
    }
}
