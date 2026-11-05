using System.Collections.Generic;

namespace XForms.Controls
{
    /// <summary>
    /// A view's property (a width, a margin) that its theme's style gives,
    /// unless the app set its own: the style's value applies while the property
    /// still has the value it started with or the style last gave it.
    /// </summary>
    internal struct StyledValue<T>
    {
        private T _styled;

        public StyledValue(
            T initial)
        {
            this._styled = initial;
        }

        public T Apply(
            T current,
            T styled)
        {
            if (EqualityComparer<T>.Default.Equals(current, this._styled))
            {
                this._styled = styled;
                return styled;
            }

            return current;
        }
    }
}
