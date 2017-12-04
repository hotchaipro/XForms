using System;
using Windows.UI.Xaml;

namespace XForms.Windows.Renderers
{
    public abstract class ElementRenderer : IElementRenderer
    {
        private Element _element;
        private DependencyObject _nativeElement;

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

        public object NativeElement
        {
            get
            {
                return this._nativeElement;
            }
        }

        public bool HasDescendant(
            Element element)
        {
            if (null == element)
            {
                return false;
            }

            var nativeDescendant = (DependencyObject)element.Renderer.NativeElement;

            return HasDescendant(this._nativeElement, nativeDescendant);
        }

        private static bool HasDescendant(
            DependencyObject parent,
            DependencyObject nativeDescendant)
        {
            if (parent == nativeDescendant)
            {
                return true;
            }

            bool isDescendant = false;

            int childrenCount = global::Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; ++i)
            {
                var child = global::Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (HasDescendant(child, nativeDescendant))
                {
                    isDescendant = true;
                    break;
                }
            }

            return isDescendant;
        }

        protected void SetNativeElement(
            DependencyObject nativeElement)
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
            DependencyObject oldElement,
            DependencyObject newElement)
        {
        }

        public virtual void OnApplyTheme(
            AppTheme theme)
        {
        }
    }
}
