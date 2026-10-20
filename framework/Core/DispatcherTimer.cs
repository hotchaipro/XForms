using System;
using System.Threading;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// Raises <see cref="Tick"/> on the UI thread after each
    /// <see cref="Interval"/>, until stopped.
    /// </summary>
    public sealed class DispatcherTimer
    {
        private int _generation;

        public DispatcherTimer()
        {
        }

        /// <summary>
        /// Creates a timer that runs the action at each tick.
        /// </summary>
        public DispatcherTimer(
            TimeSpan interval,
            Action action)
        {
            if (null == action)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this.Interval = interval;
            this.Tick += (sender, e) => action();
        }

        public event EventHandler Tick;

        public TimeSpan Interval
        {
            get;
            set;
        }

        public bool IsEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts the timer, or restarts it from now if it's running.
        /// </summary>
        public void Start()
        {
            this.IsEnabled = true;
            int generation = Interlocked.Increment(ref this._generation);
            this.ScheduleTick(generation);
        }

        public void Stop()
        {
            this.IsEnabled = false;
            Interlocked.Increment(ref this._generation);
        }

        private void ScheduleTick(
            int generation)
        {
            var application = Application.Current;
            Task.Delay(this.Interval).ContinueWith((task) =>
            {
                application.InvokeOnUIThreadAsync(() =>
                {
                    // Ignore ticks from before the latest Start or Stop
                    if (!this.IsEnabled || (generation != this._generation))
                    {
                        return;
                    }

                    this.Tick?.Invoke(this, EventArgs.Empty);

                    if (this.IsEnabled && (generation == this._generation))
                    {
                        this.ScheduleTick(generation);
                    }
                });
            }, TaskScheduler.Default);
        }
    }
}
