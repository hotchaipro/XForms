using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// Content that opens a URI when tapped. Its theme's template gives it
    /// its feedback.
    /// </summary>
    public class HyperlinkButton : TemplatedContentControl, ITapGestureDelegate
    {
        private UriNavigationCommand _command;

        public HyperlinkButton()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
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
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
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
