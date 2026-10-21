using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// How a page arrives or leaves.
    /// </summary>
    public enum PageTransition
    {
        /// <summary>The platform's usual transition: a slide.</summary>
        Default,

        /// <summary>Slides in from the side, and back out.</summary>
        Slide,

        /// <summary>Slides up from the bottom, and back down (modal pages).</summary>
        SlideUp,

        /// <summary>Fades in, and out.</summary>
        Fade,

        /// <summary>Shows at once.</summary>
        None,
    }

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

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateFrameRenderer(this);
        }

        /// <summary>
        /// Shows the page, keeping the current one in the back stack. Completes
        /// when the transition has, or at once if the current page cancels.
        /// </summary>
        public virtual async Task NavigateAsync(
            Page page,
            PageTransition transition = PageTransition.Default)
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
            PageTransition transition = PageTransition.Default)
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
            PageTransition transition = PageTransition.Default)
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
                await this.PlayAsync(isBack ? leaving : arriving, isBack, transition, slideFromStart);
            }

            if ((null != leaving) && (leaving != this._currentPage))
            {
                leaving.IsVisible = false;
                if (removeLeavingPage)
                {
                    this.Detach(leaving);
                }
            }
        }

        // Moves the page in (arriving) or out (going back) as the transition says
        private async Task PlayAsync(
            Page page,
            bool isBack,
            PageTransition transition,
            bool slideFromStart)
        {
            var animator = page.GetOrCreateAnimator();
            var motion = page.ActualTheme.Motion;
            var easing = isBack ? motion.Accelerate : motion.Decelerate;
            var size = this.Bounds.Size;

            Point offset;
            switch (transition)
            {
                case PageTransition.Fade:
                    animator.Translation = Point.Zero;
                    if (isBack)
                    {
                        await animator.FadeTo(0, motion.Medium, easing);
                    }
                    else
                    {
                        await animator.FadeFrom(0, motion.Medium, easing);
                    }

                    animator.Opacity = 1;
                    return;

                case PageTransition.SlideUp:
                    offset = new Point(0, size.Height);
                    break;

                default:
                    offset = new Point(slideFromStart ? -size.Width : size.Width, 0);
                    break;
            }

            animator.Opacity = 1;
            if (isBack)
            {
                animator.Translation = Point.Zero;
                await animator.TranslateTo(offset, motion.Medium, easing);
                animator.Translation = Point.Zero;
            }
            else
            {
                animator.Translation = offset;
                await animator.TranslateTo(Point.Zero, motion.Medium, easing);
            }
        }

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
