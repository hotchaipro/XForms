using System;
using XForms.Input;

namespace XForms.Controls
{
    public class HyperlinkView : ContentControl, ITapGestureDelegate
    {
        private const float TouchDownScale = 0.9f;

        private UriNavigationCommand _command;

        public HyperlinkView()
        {
            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public Uri NavigationUri
        {
            get
            {
                return this._command?.NavigationUri;
            }

            set
            {
                if (null == this._command)
                {
                    this._command = new UriNavigationCommand();
                }

                this._command.NavigationUri = value;
            }
        }

        public new View Content
        {
            get
            {
                return base.Content;
            }

            set
            {
                base.Content = value;
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            this._command?.Execute(null);
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion
    }
}
