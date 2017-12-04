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

        protected Element()
        {
            this._properties = new Dictionary<IAttachedProperty, object>();
            this.Renderer = this.CreateRenderer();

            // TODO: Use a weak delegate
            Application.Current.ThemeChanged += Application_ThemeChanged;

            this.OnLoaded();
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

        public virtual IElementRenderer Renderer
        {
            get;
            private set;
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

        internal bool HasDescendant(
            Element element)
        {
            return this.Renderer.HasDescendant(element);
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

        protected virtual void OnLoaded()
        {
        }

        private void Application_ThemeChanged(
            object sender,
            AppTheme theme)
        {
            this.OnApplyTheme(theme);
        }

        protected virtual void OnApplyTheme(
            AppTheme theme)
        {
            this.Renderer.OnApplyTheme(theme);
        }
    }
}
