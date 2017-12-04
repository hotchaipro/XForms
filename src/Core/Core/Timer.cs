using System;
using System.Threading;
using System.Threading.Tasks;

namespace XForms
{
    public sealed class Timer
    {
        private static volatile int CurrentInstance;

        private Action _action;
        private CancellationTokenSource _cancelTokenSource;

        public Timer(
            Action action)
        {
            if (null == action)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this._action = action;

            this._cancelTokenSource = new CancellationTokenSource();
        }

        public void Start(
            TimeSpan delay)
        {
            int instance = Interlocked.Increment(ref CurrentInstance);

            Task.Delay(delay, this._cancelTokenSource.Token).ContinueWith((t, s) =>
            {
                // Only execute the action for the latest delay task instance
                int currentInstance = Interlocked.CompareExchange(ref CurrentInstance, 0, 0);
                if ((int)s == currentInstance)
                {
                    // TODO: Execute on UI thread
                    this._action();
                }
            },
            instance,
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public void Cancel()
        {
            this._cancelTokenSource.Cancel();
        }
    }
}
