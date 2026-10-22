using System;

namespace XForms
{
    /// <summary>
    /// A view property that can be bound, such as <c>TextBlock.TextProperty</c>:
    /// its name and compiled accessors. There's no property engine behind it;
    /// the view's own property stores the value and raises
    /// <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    public sealed class ViewProperty<TView, TValue>
        where TView : View
    {
        private readonly Func<TView, TValue> _getValue;
        private readonly Action<TView, TValue> _setValue;

        public ViewProperty(
            string name,
            Func<TView, TValue> getValue,
            Action<TView, TValue> setValue)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this._getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            this._setValue = setValue ?? throw new ArgumentNullException(nameof(setValue));
        }

        /// <summary>
        /// The property's name, as the view reports it in PropertyChanged.
        /// </summary>
        public string Name
        {
            get;
        }

        public TValue GetValue(
            TView view)
        {
            return this._getValue(view);
        }

        public void SetValue(
            TView view,
            TValue value)
        {
            this._setValue(view, value);
        }

        public override string ToString()
        {
            return typeof(TView).Name + "." + this.Name;
        }
    }
}
