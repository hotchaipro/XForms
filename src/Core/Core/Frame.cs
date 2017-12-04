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

        public new IFrameRenderer Renderer
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

            this.Renderer.Push(page);
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
                    currentPage.Translation = XForms.Point.Zero;
                    currentPage.TranslateTo(new XForms.Point(-this.Size.Width, 0), TimeSpan.FromMilliseconds(150), new CubicEase(EasingMode.EaseOut)).ContinueWith(
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
                newPage.Translation = XForms.Point.Zero;
                newPage.IsVisible = true;
            }
            else
            {
                // New page animates into the frame
                newPage.Translation = new XForms.Point(-this.Size.Width, 0);
                newPage.IsVisible = true;
                newPage.TranslateTo(XForms.Point.Zero, TimeSpan.FromMilliseconds(150), new CubicEase(EasingMode.EaseIn)).ContinueWith(
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
            this.Renderer.SetContent(page, hideCurrentPage);
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
