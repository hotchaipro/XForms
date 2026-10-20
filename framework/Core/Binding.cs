using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms
{
    /// <summary>
    /// Keeps a view property up to date with a value from a source, such as a
    /// view model, and (for a two-way binding) the source with the view.
    /// </summary>
    /// <remarks>
    /// The value comes from a compiled lambda, so any conversion is ordinary
    /// code. When the source implements <see cref="INotifyPropertyChanged"/>,
    /// the binding reads the lambda again whenever the source raises it, and
    /// updates the view if the value changed. Changes to objects the lambda
    /// reaches through the source (vm.Address.City) are seen only when the
    /// source itself raises a change. The source must raise its changes on the
    /// UI thread.
    ///
    /// The view keeps its bindings, and the source refers to a binding only
    /// weakly, so a view model doesn't keep the views bound to it alive.
    /// </remarks>
    public abstract class Binding : IDisposable
    {
        private protected Binding()
        {
        }

        /// <summary>
        /// Reads the source again and updates the view.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Stops the binding: the view and source stop updating each other.
        /// </summary>
        public abstract void Dispose();
    }

    internal sealed class Binding<TView, TValue, TSource> : Binding
        where TView : View
    {
        private readonly WeakReference<TView> _view;
        private readonly ViewProperty<TView, TValue> _property;
        private readonly TSource _source;
        private readonly Func<TSource, TValue> _getValue;
        private readonly Action<TSource, TValue> _setValue;
        private WeakPropertyChangedHandler _sourceHandler;
        private bool _isUpdating;
        private bool _isDisposed;

        internal Binding(
            TView view,
            ViewProperty<TView, TValue> property,
            TSource source,
            Func<TSource, TValue> getValue,
            Action<TSource, TValue> setValue)
        {
            this._view = new WeakReference<TView>(view);
            this._property = property;
            this._source = source;
            this._getValue = getValue;
            this._setValue = setValue;

            if (source is INotifyPropertyChanged notifier)
            {
                this._sourceHandler = new WeakPropertyChangedHandler(notifier, this);
            }

            if (null != setValue)
            {
                view.PropertyChanged += this.View_PropertyChanged;
            }

            view.AddBinding(this);
            this.Refresh();
        }

        public override void Refresh()
        {
            if (this._isDisposed || !this._view.TryGetTarget(out var view))
            {
                return;
            }

            var value = this._getValue(this._source);
            if (EqualityComparer<TValue>.Default.Equals(value, this._property.GetValue(view)))
            {
                return;
            }

            this._isUpdating = true;
            try
            {
                this._property.SetValue(view, value);
            }
            finally
            {
                this._isUpdating = false;
            }
        }

        private void View_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (this._isUpdating || this._isDisposed || (e.PropertyName != this._property.Name))
            {
                return;
            }

            this._setValue(this._source, this._property.GetValue((TView)sender));
        }

        public override void Dispose()
        {
            if (this._isDisposed)
            {
                return;
            }

            this._isDisposed = true;
            this._sourceHandler?.Detach();
            this._sourceHandler = null;

            if (this._view.TryGetTarget(out var view))
            {
                view.PropertyChanged -= this.View_PropertyChanged;
                view.RemoveBinding(this);
            }
        }

        // Subscribes to the source while holding the binding weakly, and
        // unsubscribes once the binding is gone
        private sealed class WeakPropertyChangedHandler
        {
            private readonly INotifyPropertyChanged _source;
            private readonly WeakReference<Binding> _binding;

            public WeakPropertyChangedHandler(
                INotifyPropertyChanged source,
                Binding binding)
            {
                this._source = source;
                this._binding = new WeakReference<Binding>(binding);
                source.PropertyChanged += this.Source_PropertyChanged;
            }

            public void Detach()
            {
                this._source.PropertyChanged -= this.Source_PropertyChanged;
            }

            private void Source_PropertyChanged(
                object sender,
                PropertyChangedEventArgs e)
            {
                if (this._binding.TryGetTarget(out var binding))
                {
                    binding.Refresh();
                }
                else
                {
                    this.Detach();
                }
            }
        }
    }

    public static class ViewBindingExtensions
    {
        /// <summary>
        /// Binds a view property to a value from a source, one way:
        /// <c>title.Bind(TextBlock.TextProperty, viewModel, vm => vm.Title)</c>.
        /// </summary>
        public static Binding Bind<TView, TValue, TSource>(
            this TView view,
            ViewProperty<TView, TValue> property,
            TSource source,
            Func<TSource, TValue> getValue)
            where TView : View
        {
            return Create(view, property, source, getValue, null);
        }

        /// <summary>
        /// Binds a view property to a value from a source, both ways: changes
        /// to the view (such as typing in a text box) are written back with
        /// <paramref name="setValue"/>.
        /// </summary>
        public static Binding Bind<TView, TValue, TSource>(
            this TView view,
            ViewProperty<TView, TValue> property,
            TSource source,
            Func<TSource, TValue> getValue,
            Action<TSource, TValue> setValue)
            where TView : View
        {
            if (null == setValue)
            {
                throw new ArgumentNullException(nameof(setValue));
            }

            return Create(view, property, source, getValue, setValue);
        }

        private static Binding Create<TView, TValue, TSource>(
            TView view,
            ViewProperty<TView, TValue> property,
            TSource source,
            Func<TSource, TValue> getValue,
            Action<TSource, TValue> setValue)
            where TView : View
        {
            if (null == view)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (null == property)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (null == getValue)
            {
                throw new ArgumentNullException(nameof(getValue));
            }

            return new Binding<TView, TValue, TSource>(view, property, source, getValue, setValue);
        }
    }
}
