using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// Shows one page at a time, with a back stack. Navigation is
    /// asynchronous: each method completes when its transition has.
    /// </summary>
    public class Frame : View
    {
        private readonly Stack<Page> _backStack = new Stack<Page>();
        private Page _currentPage;
        private PageSwitch _pageSwitch;

        // Every page the frame holds, shown or not (pushed pages, such as
        // tabs, are in neither the back stack nor current), in the order added
        private readonly List<Page> _pages = new List<Page>();
        private PageBounce _pageBounce;

        /// <summary>
        /// The window a root frame shows in, for a window the app opened; null
        /// for the main window's.
        /// </summary>
        internal Window Window
        {
            get;
            set;
        }

        public Frame()
        {
        }

        private Color? _background;

        /// <summary>
        /// The frame's background, behind its pages: where a page isn't (as
        /// it slides, or is pulled past the end), what's behind the frame
        /// shows through a transparent one. Null (the default) for the
        /// platform's (the system background on iOS; none elsewhere).
        /// </summary>
        public Color? Background
        {
            get
            {
                return this._background;
            }

            set
            {
                this._background = value;
                if (value.HasValue && this.HasRenderer)
                {
                    this.Renderer.BackgroundColor = value.Value;
                }

                this.OnPropertyChanged();
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();
            if (this._background.HasValue)
            {
                this.Renderer.BackgroundColor = this._background.Value;
            }
        }

        [PlatformContract]
        internal new IFrameRenderer Renderer
        {
            get
            {
                return (IFrameRenderer)base.Renderer;
            }
        }

        /// <summary>
        /// The page showing, or null.
        /// </summary>
        public Page CurrentPage
        {
            get
            {
                return this._currentPage;
            }
        }

        public virtual bool CanGoBack
        {
            get
            {
                return (this._backStack.Count > 0);
            }
        }

        /// <summary>
        /// Adds a page to the frame without showing it, so it's ready to show
        /// at once (as tabs are).
        /// </summary>
        public void Push(
            Page page)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.IsVisible = false;
            this.Attach(page);
        }

        // The platforms render it natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateFrameRenderer(this);
        }

        /// <summary>
        /// Shows the page, keeping the current one in the back stack. Completes
        /// when the transition has, or at once if the current page cancels.
        /// </summary>
        public virtual async Task NavigateAsync(
            Page page,
            PageTransition transition = null)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var currentPage = this._currentPage;
            if ((null != currentPage) && !currentPage.OnNavigatingFrom(new NavigationEventArgs(NavigationMode.Forward)))
            {
                return;
            }

            if (null != currentPage)
            {
                this._backStack.Push(currentPage);
            }

            await this.ShowAsync(page, currentPage, NavigationMode.Forward, transition, removeLeavingPage: false);
        }

        /// <summary>
        /// Shows the page in place of the current one, without the back stack
        /// (as tabs switch). The mode sets the direction it slides from.
        /// </summary>
        public virtual async Task ReplaceAsync(
            Page page,
            NavigationMode mode = NavigationMode.Forward,
            PageTransition transition = null)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var currentPage = this._currentPage;
            if (page == currentPage)
            {
                return;
            }

            if ((null != currentPage) && !currentPage.OnNavigatingFrom(new NavigationEventArgs(mode)))
            {
                return;
            }

            await this.ShowAsync(page, currentPage, mode, transition, removeLeavingPage: false, slideFromStart: mode == NavigationMode.Back);
        }

        /// <summary>
        /// Starts showing the page in place of the current one (as
        /// <see cref="ReplaceAsync"/> does), following a gesture: it slides in
        /// as far as the switch's <see cref="PageSwitch.Progress"/>, then
        /// completes or goes back. Returns null if there's no current page,
        /// it's the page, the current page cancels, or a switch is under way.
        /// </summary>
        public PageSwitch BeginReplace(
            Page page,
            NavigationMode mode = NavigationMode.Forward)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var currentPage = this._currentPage;
            if ((null == currentPage) || (page == currentPage) || (null != this._pageSwitch) || (null != this._pageBounce))
            {
                return null;
            }

            if (!currentPage.OnNavigatingFrom(new NavigationEventArgs(mode)))
            {
                return null;
            }

            this.Attach(page);
            if (page.IsHotReloadPending)
            {
                page.ApplyHotReload();
            }
            this.Renderer.BringToFront(page);

            this._pageSwitch = new PageSwitch(this, page, currentPage, mode);
            page.IsVisible = true;
            return this._pageSwitch;
        }

        /// <summary>
        /// Starts pulling the current page sideways as a gesture moves where
        /// there's no page to switch to (past the first or last tab): it moves
        /// as far as the bounce's <see cref="PageBounce.Offset"/>, with its edge
        /// shadow, and springs back when released. Returns null if there's no
        /// current page, or a switch or another bounce is under way.
        /// </summary>
        public PageBounce BeginBounce()
        {
            var currentPage = this._currentPage;
            if ((null == currentPage) || (null != this._pageSwitch) || (null != this._pageBounce))
            {
                return null;
            }

            this._pageBounce = new PageBounce(this, currentPage);
            return this._pageBounce;
        }

        // A bounce sprang back
        internal void OnPageBounceEnded(
            PageBounce pageBounce)
        {
            if (this._pageBounce == pageBounce)
            {
                this._pageBounce = null;
            }
        }

        // A switch completes: its page is the current one, as the rest of its
        // slide plays
        internal void OnPageSwitchCompleting(
            PageSwitch pageSwitch)
        {
            this.MakeCurrent(pageSwitch.Page, pageSwitch.LeavingPage, pageSwitch.Mode);
        }

        // A switch ended: the page that isn't current is hidden
        internal void OnPageSwitchEnded(
            PageSwitch pageSwitch)
        {
            if (this._pageSwitch == pageSwitch)
            {
                this._pageSwitch = null;
            }

            var hidden = (pageSwitch.Page == this._currentPage) ? pageSwitch.LeavingPage : pageSwitch.Page;
            this.HidePage(hidden, remove: false);
            PageTransition.HideEdgeShadow(pageSwitch.Page);
        }

        /// <summary>
        /// Goes back to the previous page. Completes with false if there's
        /// none, or the current page cancels.
        /// </summary>
        public virtual async Task<bool> GoBackAsync(
            PageTransition transition = null)
        {
            if (!this.CanGoBack)
            {
                return false;
            }

            var currentPage = this._currentPage;
            if ((null != currentPage) && !currentPage.OnNavigatingFrom(new NavigationEventArgs(NavigationMode.Back)))
            {
                return false;
            }

            var previousPage = this._backStack.Pop();
            await this.ShowAsync(previousPage, currentPage, NavigationMode.Back, transition, removeLeavingPage: true);
            return true;
        }

        // Hot reload: the pages in the back stack are built again when they're
        // shown again (the current one is, as the frame's child)
        protected override void OnHotReload()
        {
            base.OnHotReload();
            foreach (var page in this._backStack)
            {
                page.IsHotReloadPending = true;
            }
        }

        // Shows the arriving page with the transition, then hides (or removes)
        // the leaving one
        private async Task ShowAsync(
            Page arriving,
            Page leaving,
            NavigationMode mode,
            PageTransition transition,
            bool removeLeavingPage,
            bool slideFromStart = false)
        {
            bool isBack = (mode == NavigationMode.Back) && !slideFromStart;

            this.Attach(arriving);
            if (arriving.IsHotReloadPending)
            {
                arriving.ApplyHotReload();
            }
            if (!isBack)
            {
                // An arriving page goes on top; going back uncovers it instead
                this.Renderer.BringToFront(arriving);
            }

            arriving.IsVisible = true;
            this.MakeCurrent(arriving, leaving, mode);

            if ((null != leaving) && (transition != PageTransition.None))
            {
                var context = new PageTransitionContext(isBack ? leaving : arriving, isBack, slideFromStart, this.Bounds.Size, this.TransitionDuration);
                await (transition ?? PageTransition.Slide).PlayAsync(context);
            }

            if ((null != leaving) && (leaving != this._currentPage))
            {
                this.HidePage(leaving, removeLeavingPage);
            }
        }

        // The arriving page is the current one, and the pages hear so
        private void MakeCurrent(
            Page arriving,
            Page leaving,
            NavigationMode mode)
        {
            this._currentPage = arriving;
            if (null == this.Window)
            {
                this.Application.UpdateSafeAreaUse();
            }

            leaving?.OnNavigatedFrom(new NavigationEventArgs(mode));

            // Typing on a page that leaves ends: its keyboard goes with it
            if ((null != leaving) && HasFocusedTextBox(leaving))
            {
                this.Application.Renderer.EndTextEntry();
            }
            arriving.OnNavigatedTo(new NavigationEventArgs(mode));
        }

        // Hides a page that's no longer shown, and puts it back (a transition
        // leaves a leaving page where it ended: off-screen, or faded out)
        private void HidePage(
            Page page,
            bool remove)
        {
            page.IsVisible = false;
            var animator = page.Animator;
            if (null != animator)
            {
                animator.Translation = Point.Zero;
                animator.Scale = 1;
                animator.Rotation = default;
                animator.Opacity = 1;
            }
            PageTransition.HideEdgeShadow(page);

            if (remove)
            {
                this.Detach(page);
                page.OnRemovedFromBackStack();
            }
        }

        private static bool HasFocusedTextBox(
            View view)
        {
            if ((view is Primitives.TextBox textBox) && textBox.IsFocused)
            {
                return true;
            }

            var children = view.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                if (HasFocusedTextBox(children[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The pages in the back stack and the current one, oldest first.
        /// </summary>
        internal IEnumerable<Page> GetStack()
        {
            var pages = new List<Page>(this._backStack);
            pages.Reverse();
            if (null != this._currentPage)
            {
                pages.Add(this._currentPage);
            }

            return pages;
        }

        /// <summary>
        /// How long a page takes to arrive or leave (see <see cref="PageTransition"/>).
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get;
            set;
        } = TimeSpan.FromMilliseconds(250);

        private void Attach(
            Page page)
        {
            bool isNew = (page.Parent != this);
            page.AttachTo(this);
            this.Renderer.AddPage(page);
            if (!this._pages.Contains(page))
            {
                this._pages.Add(page);
            }

            if (isNew)
            {
                page.NotifyAttached();
            }
        }

        private void Detach(
            Page page)
        {
            this.Renderer.RemovePage(page);
            this._pages.Remove(page);
            page.NotifyDetached();
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var currentPage = this._currentPage;
            if (null != currentPage)
            {
                yield return currentPage;
            }
        }

        // Every page the frame holds keeps its native objects (the back
        // stack's, and pushed pages such as tabs), so they all restyle too,
        // and are ready when they're shown
        internal override IEnumerable<Element> GetThemedChildElements()
        {
            return this._pages.ToArray();
        }
    }
}
