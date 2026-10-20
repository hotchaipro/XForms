using System;
using System.Collections.Generic;

namespace XForms
{
    public interface IFrameRenderer : IViewRenderer
    {
        void SetContent(Page newPage, bool hideCurrentPage);

        void Push(Page page);
    }

    public class Frame : View
    {
        private Page _currentPage;
        private Stack<Page> _backStack;

        public Frame()
        {
            this._backStack = new Stack<Page>();
        }

        internal new IFrameRenderer Renderer
        {
            get
            {
                return (IFrameRenderer)base.Renderer;
            }
        }

        public void Push(
            Page page)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.IsVisible = false;
            page.AttachTo(this);
            this.Renderer.Push(page);
            page.NotifyAttached();
        }

        public virtual bool CanNavigateBack
        {
            get
            {
                return (this._backStack.Count > 0);
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateFrameRenderer(this);
        }

        public virtual void NavigateTo(
            Page newPage)
        {
            if (null == newPage)
            {
                throw new ArgumentNullException(nameof(newPage));
            }

            Page currentPage = this._currentPage;
            if (null != currentPage)
            {
                if (!currentPage.OnNavigatingFrom(new NavigationEventArgs(NavigationMode.Forward)))
                {
                    // Navigation canceled
                    return;
                }

                this._backStack.Push(currentPage);
            }

            this.SetContent(newPage);

            currentPage?.OnNavigatedFrom(new NavigationEventArgs(NavigationMode.Forward));
            newPage.OnNavigatedTo(new NavigationEventArgs(NavigationMode.Forward));

            this._currentPage = newPage;
        }

        public virtual void NavigateBack()
        {
            if (!this.CanNavigateBack)
            {
                throw new InvalidOperationException();
            }

            Page currentPage = this._currentPage;
            if (null != currentPage)
            {
                if (!currentPage.OnNavigatingFrom(new NavigationEventArgs(NavigationMode.Back)))
                {
                    // Navigation canceled
                    return;
                }
            }

            Page newPage = this._backStack.Pop();

            this.SetContent(newPage);

            currentPage?.OnNavigatedFrom(new NavigationEventArgs(NavigationMode.Back));
            newPage.OnNavigatedTo(new NavigationEventArgs(NavigationMode.Back));

            this._currentPage = newPage;
        }

        public virtual void NavigateInPlace(
            Page newPage,
            NavigationMode navigationMode)
        {
            if (null == newPage)
            {
                throw new ArgumentNullException(nameof(newPage));
            }

            Page currentPage = this._currentPage;
            if (null != currentPage)
            {
                if (!currentPage.OnNavigatingFrom(new NavigationEventArgs(NavigationMode.Forward)))
                {
                    // Navigation canceled
                    return;
                }

                if (navigationMode == NavigationMode.Forward)
                {
                    // Current page animates out of the frame
                    var currentAnimator = currentPage.GetOrCreateAnimator();
                    currentAnimator.Translation = XForms.Point.Zero;
                    currentAnimator.TranslateTo(new XForms.Point(-this.Bounds.Size.Width, 0), TimeSpan.FromMilliseconds(150), new CubicEase(EasingMode.EaseOut)).Completion.ContinueWith(
                        (task) => this.Application.InvokeOnUIThreadAsync(() => { currentPage.IsVisible = false; }));
                }
                else
                {
                    // Current page stays in position and is overlapped by the new page
                }
            }

            this.SetContent(newPage, hideCurrentPage: false);

            if (navigationMode == NavigationMode.Forward)
            {
                // New page stays in position and is uncovered as the current page moves out of the frame
                newPage.GetOrCreateAnimator().Translation = XForms.Point.Zero;
                newPage.IsVisible = true;
            }
            else
            {
                // New page animates into the frame
                var newAnimator = newPage.GetOrCreateAnimator();
                newAnimator.Translation = new XForms.Point(-this.Bounds.Size.Width, 0);
                newPage.IsVisible = true;
                newAnimator.TranslateTo(XForms.Point.Zero, TimeSpan.FromMilliseconds(150), new CubicEase(EasingMode.EaseIn)).Completion.ContinueWith(
                        (task) => this.Application.InvokeOnUIThreadAsync(() => { if (null != currentPage) currentPage.IsVisible = false; }));
            }

            currentPage?.OnNavigatedFrom(new NavigationEventArgs(NavigationMode.Forward));
            newPage.OnNavigatedTo(new NavigationEventArgs(NavigationMode.Forward));

            this._currentPage = newPage;
        }

        private void SetContent(
            Page page,
            bool hideCurrentPage = true)
        {
            bool isNew = (page.Parent != this);
            page.AttachTo(this);
            this.Renderer.SetContent(page, hideCurrentPage);
            if (isNew)
            {
                page.NotifyAttached();
            }
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var currentPage = this._currentPage;
            if (null != currentPage)
            {
                yield return currentPage;
            }
        }
    }
}
