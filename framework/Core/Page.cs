using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using XForms.Platform;

namespace XForms
{


    public class Page : View
    {
        private View _content;
        private Color? _backgroundColor;
        private bool _usesSafeArea = true;

        public Page()
        {
        }

        protected override Color GetBackgroundColor()
        {
            return this.BackgroundColor;
        }

        // Pages are views the platforms don't render natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The page's background: the theme's background color unless set.
        /// Pages are opaque, so they can overlap as they slide in and out.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor ?? this.ActualTheme.Colors.Background;
            }

            set
            {
                this._backgroundColor = value;
                this.InvalidateBackground();
            }
        }

        /// <summary>
        /// Whether the platform keeps the page clear of the system bars,
        /// display cutout and soft keyboard (the default). Set it to false to
        /// draw under them (a full-bleed image, say) and lay content out
        /// around <see cref="Window.SafeAreaInsets"/> yourself.
        /// </summary>
        public bool UsesSafeArea
        {
            get
            {
                return this._usesSafeArea;
            }

            set
            {
                if (value != this._usesSafeArea)
                {
                    this._usesSafeArea = value;
                    if (this.HasRenderer)
                    {
                        this.Application.UpdateSafeAreaUse();
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The page's content, which fills it.
        /// </summary>
        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                if (value == this._content)
                {
                    return;
                }

                if (null != this._content)
                {
                    this.RemoveVisualChild(this._content);
                }

                this._content = value;
                if (null != value)
                {
                    this.AddVisualChild(value);
                }
            }
        }

        // Frames slide pages in and out
        internal override bool HasFrameworkAnimator
        {
            get
            {
                return true;
            }
        }



        /// <summary>
        /// Closes the page, when it's shown with
        /// <see cref="Application.ShowModalAsync(Page)"/>.
        /// </summary>
        public Task CloseModalAsync()
        {
            return this.Application.CloseModalAsync(this, null);
        }

        internal protected virtual bool OnNavigatingFrom(
            NavigationEventArgs navigationEventArgs)
        {
            return true;
        }

        internal protected virtual void OnNavigatedTo(
            NavigationEventArgs navigationEventArgs)
        {
        }

        internal protected virtual void OnNavigatedFrom(
            NavigationEventArgs navigationEventArgs)
        {
        }

        // A press on the page itself (not on any of its views) light-dismisses
        // and does nothing else
        internal override bool DispatchPointerEvent(
            PointerEventArgs e)
        {
            e.OriginalSource ??= this;
            return this.Application.FilterPointerInput(this, e) || base.DispatchPointerEvent(e);
        }


    }
}
