using System;
using System.Collections.Generic;

namespace XForms
{
    public interface IElementRenderer
    {
        Element Element
        {
            get;
        }

        object NativeElement
        {
            get;
        }

        bool HasDescendant(Element element);

        /// <summary>
        /// The element's effective theme, when its native object is created and
        /// whenever the theme changes.
        /// </summary>
        void OnThemeChanged(Theme theme);
    }

    public abstract class Element
    {
        public interface IAttachedProperty
        {
        }

        public class AttachedProperty<TValue> : IAttachedProperty
        {
            public AttachedProperty()
            {
            }

            public void SetValue(
                Element element,
                TValue value)
            {
                element.SetProperty(this, value);
            }

            public bool TryGetValue(
                Element element,
                out TValue value)
            {
                object objectValue;
                bool success = element.TryGetProperty(this, out objectValue);
                if (success)
                {
                    value = (TValue)objectValue;
                }
                else
                {
                    value = default(TValue);
                }

                return success;
            }

            public TValue GetValue(
                Element element,
                TValue defaultValue = default(TValue))
            {
                TValue value;

                object objectValue;
                bool success = element.TryGetProperty(this, out objectValue);
                if (success)
                {
                    value = (TValue)objectValue;
                }
                else
                {
                    value = defaultValue;
                }

                return value;
            }
        }

        private Dictionary<IAttachedProperty, object> _properties;

        private IElementRenderer _renderer;

        // The theme last given to OnThemeChanged
        private Theme _deliveredTheme;

        protected Element()
        {
            this._properties = new Dictionary<IAttachedProperty, object>();
        }

        protected Application Application
        {
            get
            {
                return Application.Current;
            }
        }

        /// <summary>
        /// The element's effective theme: its own (a view's
        /// <see cref="View.Theme"/>), or else its parent's, or else the
        /// application's.
        /// </summary>
        public Theme ActualTheme
        {
            get
            {
                return this.LocalTheme ?? this.ParentElement?.ActualTheme ?? Application.Current.Theme;
            }
        }

        // The theme set on the element itself, if any
        internal virtual Theme LocalTheme
        {
            get
            {
                return null;
            }
        }

        // The element whose theme this one inherits
        internal virtual Element ParentElement
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The element's native object, created the first time it's needed: when
        /// the element is attached to a parent (or shown). Every constructor and
        /// object initializer has run by then.
        /// </summary>
        internal IElementRenderer Renderer
        {
            get
            {
                if (null == this._renderer)
                {
                    this.CreateRendererNow();
                }

                return this._renderer;
            }
        }

        /// <summary>
        /// The native object if it has been created, or null. Property setters
        /// push values to it; until it exists, the element keeps them.
        /// </summary>
        internal IElementRenderer CreatedRenderer
        {
            get
            {
                return this._renderer;
            }
        }

        /// <summary>
        /// Whether the element's native object has been created, which happens
        /// when it's first attached.
        /// </summary>
        internal bool HasRenderer
        {
            get
            {
                return (null != this._renderer);
            }
        }

        private void CreateRendererNow()
        {
            var renderer = this.CreateRenderer();
            this._renderer = renderer;

            var theme = this.ActualTheme;
            this._deliveredTheme = theme;
            renderer.OnThemeChanged(theme);
            this.OnRendererCreated();

            // Every constructor has run, and the native object exists
            this.OnThemeChanged(theme);
        }

        /// <summary>
        /// Gives the element and its descendants their effective themes, if
        /// they changed: after the application's theme changes, a view's
        /// <see cref="View.Theme"/> is set, or an element moves.
        /// </summary>
        internal void UpdateTheme()
        {
            if (null == this._renderer)
            {
                // Its theme is delivered when its native object is created
                return;
            }

            var theme = this.ActualTheme;
            if (!theme.Equals(this._deliveredTheme))
            {
                this._deliveredTheme = theme;
                this._renderer.OnThemeChanged(theme);
                this.OnThemeChanged(theme);
            }

            foreach (var child in this.GetThemedChildElements())
            {
                child.UpdateTheme();
            }
        }

        /// <summary>
        /// Lays out and draws the element and its descendants again, at the
        /// system's new text size.
        /// </summary>
        internal void OnTextScaleChanged()
        {
            if (null == this._renderer)
            {
                return;
            }

            this.OnTextScaleChangedCore();
            foreach (var child in this.GetThemedChildElements())
            {
                child.OnTextScaleChanged();
            }
        }

        internal virtual void OnTextScaleChangedCore()
        {
        }

        /// <summary>
        /// The children that follow the element's theme: its children, and
        /// any it keeps without showing (such as a frame's back stack).
        /// </summary>
        internal virtual IEnumerable<Element> GetThemedChildElements()
        {
            return this.GetChildElements();
        }

        /// <summary>
        /// Called once the native object exists, to push the values the element
        /// kept until then.
        /// </summary>
        internal virtual void OnRendererCreated()
        {
        }

        /// <summary>
        /// Whether the element is this element or one of its descendants.
        /// </summary>
        public bool HasDescendant(
            Element element)
        {
            if (element == this)
            {
                return true;
            }

            return this._renderer?.HasDescendant(element) ?? false;
        }

        private void SetProperty(
            IAttachedProperty property,
            object value)
        {
            this._properties[property] = value;
        }

        private bool TryGetProperty(
            IAttachedProperty property,
            out object value)
        {
            return this._properties.TryGetValue(property, out value);
        }



        protected abstract IElementRenderer CreateRenderer();

        internal abstract IEnumerable<Element> GetChildElements();

        /// <summary>
        /// Called with the element's effective theme once it's attached (every
        /// constructor has run, and its native object exists), and whenever
        /// that theme changes. Never called from a constructor.
        /// </summary>
        protected virtual void OnThemeChanged(
            Theme theme)
        {
        }
    }
}
