using System;
using AndroidContext = Android.Content.Context;

namespace XForms.Android.Renderers
{
    public abstract class ElementRenderer<TNativeElement> : IElementRenderer
        where TNativeElement : class
    {
        private Element _element;
        private TNativeElement _nativeElement;
        private AndroidContext _nativeContext;

        public ElementRenderer(
            global::Android.Content.Context context,
            Element element)
        {
            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (null == element)
            {
                throw new ArgumentNullException(nameof(element));
            }

            this._nativeContext = context;
            this._element = element;
        }

        public Element Element
        {
            get
            {
                return this._element;
            }
        }

        public AndroidContext NativeContext
        {
            get
            {
                return this._nativeContext;
            }
        }

        public TNativeElement NativeElement
        {
            get
            {
                return this._nativeElement;
            }
        }

        object IElementRenderer.NativeElement
        {
            get
            {
                return this._nativeElement;
            }
        }

        protected void SetNativeElement(
            TNativeElement nativeElement)
        {
            if (null == nativeElement)
            {
                throw new ArgumentNullException(nameof(nativeElement));
            }

            var oldElement = this._nativeElement;
            this._nativeElement = nativeElement;

            this.OnNativeElementSet(oldElement, nativeElement);
        }

        protected virtual void OnNativeElementSet(
            TNativeElement oldElement,
            TNativeElement newElement)
        {
        }

        public virtual void OnApplyTheme(
            AppTheme theme)
        {
        }

        public bool HasDescendant(
            Element element)
        {
            return false;
            //throw new NotImplementedException();
        }
    }
}
