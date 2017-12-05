using System;

namespace XForms.iOS.Renderers
{
    public abstract class ElementRenderer<TNativeElement> : IElementRenderer
        where TNativeElement : class
    {
        private Element _element;
        private TNativeElement _nativeElement;

        public ElementRenderer(
            Element element)
        {
            if (null == element)
            {
                throw new ArgumentNullException(nameof(element));
            }

            this._element = element;
        }

        public Element Element
        {
            get
            {
                return this._element;
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
