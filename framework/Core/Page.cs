using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using XForma.Platform;

namespace XForma
{


    public class Page : View
    {
        private View? _content;
        private Color? _backgroundColor;
        private bool _usesSafeArea = true;
        private Color? _bottomSafeAreaColor;

        // The views reaching into the safe area, and on which edges
        private Dictionary<View, SafeAreaEdges>? _extendingViews;
        private SafeAreaEdges _extendedEdges;

        public Page()
        {
            // Drawn first, beneath the content
            this.AddVisualChild(this._topBand);
            this.AddVisualChild(this._bottomBand);
        }

        // The page's own bands in the safe area: the title bar's accent behind
        // the status bar, and the bottom color behind the home indicator or
        // navigation bar, where no view of the page reaches. They're the
        // page's, so they come and go with it as it slides
        private readonly SafeAreaBand _topBand = new SafeAreaBand();
        private readonly SafeAreaBand _bottomBand = new SafeAreaBand();

        private sealed class SafeAreaBand : View
        {
            private Color _color;

            public SafeAreaBand()
            {
                this.IsHitTestVisible = false;
                this.IsVisible = false;
            }

            // A native view of its own: a nested page's bands are outside the
            // page's bounds, where its layer can't draw
            internal override bool IsLightweightType
            {
                get
                {
                    return false;
                }
            }

            public Color Color
            {
                get
                {
                    return this._color;
                }

                set
                {
                    if (value != this._color)
                    {
                        this._color = value;
                        this.InvalidateBackground();
                    }
                }
            }

            protected override Color GetBackgroundColor()
            {
                return this._color;
            }
        }

        // The bands go in the insets, outside the content's area (a nested
        // page's, in the main page's insets it sits against)
        internal override void OnContentArranged(
            Size finalSize)
        {
            var insets = this.SafeAreaPadding;
            if (Thickness.Zero == insets)
            {
                insets = this.NestedReach;
            }

            float width = finalSize.Width + insets.Left + insets.Right;
            this._topBand.Arrange(new Rect(-insets.Left, -insets.Top, width, insets.Top));
            this._bottomBand.Arrange(new Rect(-insets.Left, finalSize.Height, width, insets.Bottom));
            this.UpdateBands();
        }

        // The bands show where the page keeps clear of the system's bars and
        // no view of its own reaches. A nested page (a tab's) fills the main
        // page's insets it sits against with its background, whatever its
        // views do, so it reaches the screen's edges as it slides.
        private void UpdateBands()
        {
            var insets = this.SafeAreaPadding;
            var application = this.Application;
            if (Thickness.Zero == insets)
            {
                var reach = this.NestedReach;
                this._topBand.Color = this.BackgroundColor;
                this._topBand.IsVisible = 0 < reach.Top;
                this._bottomBand.Color = this.BackgroundColor;
                this._bottomBand.IsVisible = 0 < reach.Bottom;
                return;
            }

            this._topBand.Color = application?.TitleBar.AccentColor ?? this.ActualTheme.Colors.Background;
            this._topBand.IsVisible = (0 < insets.Top) && !this._extendedEdges.HasFlag(SafeAreaEdges.Top);

            var bottomColor = this._bottomSafeAreaColor;
            this._bottomBand.Color = bottomColor ?? Colors.Transparent;
            this._bottomBand.IsVisible = (0 < insets.Bottom) && bottomColor.HasValue && !this._extendedEdges.HasFlag(SafeAreaEdges.Bottom);
        }

        /// <summary>
        /// For a page in a frame inside another page (a tab's): how far it
        /// reaches into that page's safe-area insets, on the edges it sits
        /// flush against (what it fills, with its background, as far as the
        /// screen's edges). Zero for a page with insets of its own.
        /// </summary>
        internal Thickness NestedReach
        {
            get
            {
                if ((Thickness.Zero != this.SafeAreaPadding) || !(this.Parent is Frame frame))
                {
                    return Thickness.Zero;
                }

                // The page fills its frame (which the platform places it in):
                // the frame's place in the main page, which has the insets
                Page? main = null;
                float x = 0, y = 0;
                for (View view = frame; view.Parent is View parent; view = parent)
                {
                    x += view.Bounds.X;
                    y += view.Bounds.Y;
                    if ((parent is Page page) && (Thickness.Zero != page.SafeAreaPadding))
                    {
                        main = page;
                        break;
                    }
                }

                if (null == main)
                {
                    return Thickness.Zero;
                }

                const float Tolerance = 0.5f;
                var insets = main.SafeAreaPadding;
                // (A page's frame is the platform's to place: its size is the
                // one it last laid out at)
                var mainSize = main.ContentArrangeSize;
                var size = frame.Bounds.Size;
                return new Thickness(
                    ((0 < insets.Left) && (Math.Abs(x - insets.Left) < Tolerance)) ? insets.Left : 0,
                    ((0 < insets.Top) && (Math.Abs(y - insets.Top) < Tolerance)) ? insets.Top : 0,
                    ((0 < insets.Right) && (Math.Abs((x + size.Width) - (mainSize.Width - insets.Right)) < Tolerance)) ? insets.Right : 0,
                    ((0 < insets.Bottom) && (Math.Abs((y + size.Height) - (mainSize.Height - insets.Bottom)) < Tolerance)) ? insets.Bottom : 0);
            }
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

                    this.InvalidateMeasure();

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The color under the system's bar at the bottom of the screen (the
        /// home indicator on iOS, the navigation bar on Android) while the page
        /// uses the safe area. Set it to the color of a bar docked at the
        /// page's bottom, so the bar reaches the screen's edge with its
        /// buttons clear of the system's. Null (the default): the page's
        /// background.
        /// </summary>
        /// <summary>
        /// A page shown in the app's main frame keeps its content clear of the
        /// system's bars (its insets are padding), unless it doesn't use the
        /// safe area.
        /// </summary>
        internal override Thickness SafeAreaPadding
        {
            get
            {
                if (this._usesSafeArea && (this.Parent is Frame frame) && (null == frame.Parent) && (null != this.Application) && (frame == this.Application.RootFrame))
                {
                    return this.Application.MainWindow?.PageInsets ?? Thickness.Zero;
                }

                return Thickness.Zero;
            }
        }

        /// <summary>
        /// The safe area's edges that views on the page reach into (see
        /// <see cref="View.ExtendsIntoSafeArea"/>): the page shows no band
        /// there.
        /// </summary>
        internal SafeAreaEdges ExtendedEdges
        {
            get
            {
                return this._extendedEdges;
            }
        }

        /// <summary>
        /// How far the page's views reach past its bounds into the safe area
        /// (a nested page's views reach into the main page's): what the page
        /// draws beyond them, which its edge shadow follows as it slides.
        /// </summary>
        internal Thickness DrawnOutset
        {
            get
            {
                // A nested page fills what it reaches with its background
                var reach = this.NestedReach;
                if (null == this._extendingViews)
                {
                    return reach;
                }

                var size = this.Bounds.Size;
                float left = reach.Left, top = reach.Top, right = reach.Right, bottom = reach.Bottom;
                foreach (var view in this._extendingViews.Keys)
                {
                    // The view's (extended) frame, in the page's coordinates
                    var rect = view.Bounds;
                    for (var parent = view.Parent as View; (null != parent) && (parent != this); parent = parent.Parent as View)
                    {
                        rect = new Rect(rect.X + parent.Bounds.X, rect.Y + parent.Bounds.Y, rect.Width, rect.Height);
                    }

                    left = Math.Max(left, -rect.X);
                    top = Math.Max(top, -rect.Y);
                    right = Math.Max(right, rect.Right - size.Width);
                    bottom = Math.Max(bottom, rect.Bottom - size.Height);
                }

                return new Thickness(left, top, right, bottom);
            }
        }

        internal void SetExtendedEdges(
            View view,
            SafeAreaEdges edges)
        {
            if (SafeAreaEdges.None == edges)
            {
                this._extendingViews?.Remove(view);
            }
            else
            {
                this._extendingViews ??= new Dictionary<View, SafeAreaEdges>();
                this._extendingViews[view] = edges;
            }

            var union = SafeAreaEdges.None;
            if (null != this._extendingViews)
            {
                foreach (var pair in this._extendingViews)
                {
                    union |= pair.Value;
                }
            }

            if (union != this._extendedEdges)
            {
                this._extendedEdges = union;
                this.UpdateBands();
            }
        }

        public Color? BottomSafeAreaColor
        {
            get
            {
                return this._bottomSafeAreaColor;
            }

            set
            {
                if (value != this._bottomSafeAreaColor)
                {
                    this._bottomSafeAreaColor = value;
                    this.UpdateBands();
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
        public View? Content
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
        /// Builds the page's views, and returns its content. XForma calls it
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
        protected virtual View? Build()
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
        public string? NavigationPath
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

        private object? _result;

        // Completed when the page leaves the back stack, for whoever navigated
        // to it waiting for a result
        internal TaskCompletionSource<object>? ResultCompletion
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
