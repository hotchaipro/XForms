using System;
using System.ComponentModel;

namespace XForma.Controls
{
    /// <summary>
    /// Listens to an object's changes without keeping the listener alive: an
    /// item can outlive the view showing it (it's the app's, and a view isn't
    /// told when an ancestor leaves the tree), and once the view's gone the
    /// next change detaches this.
    /// </summary>
    internal sealed class WeakPropertyChanged<TListener>
        where TListener : class
    {
        private readonly WeakReference<TListener> _listener;
        private readonly Action<TListener> _changed;
        private readonly INotifyPropertyChanged _source;

        private WeakPropertyChanged(
            INotifyPropertyChanged source,
            TListener listener,
            Action<TListener> changed)
        {
            this._source = source;
            this._listener = new WeakReference<TListener>(listener);
            this._changed = changed;
            source.PropertyChanged += this.Source_PropertyChanged;
        }

        /// <summary>
        /// Calls <paramref name="changed"/> with the listener at each change,
        /// while it's alive. It mustn't capture the listener (pass a static
        /// lambda).
        /// </summary>
        public static void Subscribe(
            INotifyPropertyChanged source,
            TListener listener,
            Action<TListener> changed)
        {
            new WeakPropertyChanged<TListener>(source, listener, changed);
        }

        private void Source_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (this._listener.TryGetTarget(out var listener))
            {
                this._changed(listener);
            }
            else
            {
                this._source.PropertyChanged -= this.Source_PropertyChanged;
            }
        }
    }
}
