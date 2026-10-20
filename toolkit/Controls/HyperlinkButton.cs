using System;
using XForms.Input;
using XForms.Controls;

namespace XForms.Toolkit
{
    public class HyperlinkButton : ContentControl, ITapGestureDelegate
    {
        // Animates the control itself (press feedback, spinning)
        private readonly Animator _ownAnimator;

        private const float TouchDownScale = 0.9f;

        private UriNavigationCommand _command;

        public HyperlinkButton()
        {
            this._ownAnimator = this.CreateOwnAnimator();

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
            this._ownAnimator.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._ownAnimator.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
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
