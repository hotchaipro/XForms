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
            this._currentPage = arriving;
            if (null == this.Window)
            {
                this.Application.UpdateSafeAreaUse();
            }

            leaving?.OnNavigatedFrom(new NavigationEventArgs(mode));
            arriving.OnNavigatedTo(new NavigationEventArgs(mode));

            if ((null != leaving) && (transition != PageTransition.None))
            {
                var context = new PageTransitionContext(isBack ? leaving : arriving, isBack, slideFromStart, this.Bounds.Size, this.TransitionDuration);
                await (transition ?? PageTransition.Slide).PlayAsync(context);
            }

            if ((null != leaving) && (leaving != this._currentPage))
            {
                leaving.IsVisible = false;
                if (removeLeavingPage)
                {
                    this.Detach(leaving);
                    leaving.OnRemovedFromBackStack();
                }
            }
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
            if (isNew)
            {
                page.NotifyAttached();
            }
        }

        private void Detach(
            Page page)
        {
            this.Renderer.RemovePage(page);
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

        // Pages in the back stack keep their native objects, so they restyle
        // too, and are ready when they come back
        internal override IEnumerable<Element> GetThemedChildElements()
        {
            foreach (var page in this._backStack)
            {
                yield return page;
            }

            var currentPage = this._currentPage;
            if (null != currentPage)
            {
                yield return currentPage;
            }
        }
    }
}
