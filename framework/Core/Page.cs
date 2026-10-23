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
        /// The page's content, which fills it: what <see cref="Build"/>
        /// returns.
        /// </summary>
        public View Content
        {
            get
            {
                return this._content;
            }

            protected set
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

        private bool _isBuilt;

        /// <summary>
        /// Builds the page's views, and returns its content. XForms calls it
        /// once, as the page is first shown (before its native object exists),
        /// and again after an edit while the app runs with hot reload.
        /// </summary>
        /// <remarks>
        /// The page's views are built here, not in its constructor, which sets
        /// up only its state (a view model, the item it shows). A field that
        /// refers to a view is assigned here, so after a rebuild it refers to
        /// the new view. Subscriptions to objects outside the page (the
        /// window, a view model) belong in the constructor or
        /// <see cref="View.OnAttached"/>, which run once.
        /// </remarks>
        protected virtual View Build()
        {
            return null;
        }

        /// <summary>
        /// Whether <see cref="Build"/> has run: the page's views exist.
        /// </summary>
        protected bool IsBuilt
        {
            get
            {
                return this._isBuilt;
            }
        }

        // Built again after a hot reload edit; for a page in the back stack,
        // when it's shown again
        internal bool IsHotReloadPending
        {
            get;
            set;
        }

        protected override void OnHotReload()
        {
            base.OnHotReload();
            this.IsHotReloadPending = false;
            if (this._isBuilt)
            {
                var saved = SaveUserState(this.Content);
                var content = this.Build();
                if (null != content)
                {
                    this.Content = content;
                    RestoreUserState(saved, content);
                }
            }
        }

        internal override void OnBuilding()
        {
            if (this._isBuilt)
            {
                return;
            }

            this._isBuilt = true;
            var content = this.Build();
            if (null != content)
            {
                this.Content = content;
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
        /// The path the page was made from (see <see cref="Routes"/>), such as
        /// <c>/item/42</c>; null for a page made otherwise (one shown in a
        /// <see cref="Frame"/> inside a page).
        /// </summary>
        public string NavigationPath
        {
            get;
            internal set;
        }

        /// <summary>
        /// Closes the page, when it's shown with
        /// <see cref="Application.ShowModalAsync(string)"/>.
        /// </summary>
        public Task CloseModalAsync()
        {
            return this.Application.CloseModalAsync(this);
        }

        private object _result;

        // Completed when the page leaves the back stack, for whoever navigated
        // to it waiting for a result
        internal TaskCompletionSource<object> ResultCompletion
        {
            get;
            set;
        }

        /// <summary>
        /// Sets what the page gives back when it goes (by Back, or closing),
        /// for <see cref="Application.NavigateAsync{TResult}"/> and
        /// <see cref="Application.ShowModalAsync{TResult}"/>.
        /// </summary>
        protected void SetResult(
            object result)
        {
            this._result = result;
        }

        // The page has left the back stack: it won't show again
        internal void OnRemovedFromBackStack()
        {
            var completion = this.ResultCompletion;
            this.ResultCompletion = null;
            completion?.TrySetResult(this._result);
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
