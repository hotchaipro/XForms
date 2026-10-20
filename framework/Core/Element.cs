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

        void OnApplyTheme(AppTheme theme);
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

        private object _bindingContext;

        private Dictionary<IAttachedProperty, object> _properties;

        private IElementRenderer _renderer;

        protected Element()
        {
            this._properties = new Dictionary<IAttachedProperty, object>();

            // TODO: Use a weak delegate
            Application.Current.ThemeChanged += Application_ThemeChanged;

            // NOTE: The theme is applied before any derived constructor runs, so
            // values set by constructors and initializers take precedence. No
            // native object exists yet: it's created when the element is attached.
            this.OnApplyTheme(Application.Current.Theme);
        }

        protected Application Application
        {
            get
            {
                return Application.Current;
            }
        }

        protected AppTheme Theme
        {
            get
            {
                return Application.Current.Theme;
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

            renderer.OnApplyTheme(this.Theme);
            this.OnRendererCreated();
        }

        /// <summary>
        /// Called once the native object exists, to push the values the element
        /// kept until then.
        /// </summary>
        internal virtual void OnRendererCreated()
        {
        }

        public object BindingContext
        {
            get
            {
                return _bindingContext;
            }

            set
            {
                if (value != this._bindingContext)
                {
                    _bindingContext = value;
                    this.OnBindingContextChanged();
                }
            }
        }

        //private void ApplyBinding()
        //{
        //    // f(element, bindingContext)

        //    // element.Property = bindingContext.Property

        //    // + event

        //    // bindingContext.PropertyChanged += (element, value) => { element.Property = value; }
        //}

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

        protected virtual void OnBindingContextChanged()
        {
        }

        protected abstract IElementRenderer CreateRenderer();

        internal abstract IEnumerable<Element> GetChildElements();

        private void Application_ThemeChanged(
            object sender,
            AppTheme theme)
        {
            this.OnApplyTheme(theme);
        }

        protected virtual void OnApplyTheme(
            AppTheme theme)
        {
            this._renderer?.OnApplyTheme(theme);
        }
    }
}
