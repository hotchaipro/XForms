using System;
using System.Threading.Tasks;

namespace XForma
{
    /// <summary>
    /// A page replacing a frame's current one as a gesture moves (see
    /// <see cref="Frame.BeginReplace"/>): it slides in as far as the
    /// <see cref="Progress"/>, then completes, or slides back out. The current
    /// page slides out with it, unless the frame's theme has
    /// <see cref="PageTransition.SlideOver"/>.
    /// </summary>
    public sealed class PageSwitch
    {
        private static readonly EasingFunction DecelerateEasing = new CubicEase(EasingMode.EaseOut);

        private readonly Frame _frame;
        private readonly Animator _animator;
        private readonly Animator _leavingAnimator;
        private readonly Point _offset;
        private float _progress;
        private bool _isEnded;

        internal PageSwitch(
            Frame frame,
            Page page,
            Page leavingPage,
            NavigationMode mode)
        {
            this._frame = frame;
            this.Page = page;
            this.LeavingPage = leavingPage;
            this.Mode = mode;

            // Going back, the page comes from the start side, as tabs do
            var width = frame.Bounds.Width;
            this._offset = new Point((mode == NavigationMode.Back) ? -width : width, 0);

            this._animator = page.GetOrCreateAnimator();
            this._animator.Opacity = 1;
            this._animator.Translation = this._offset;
            PageTransition.ShowEdgeShadow(page);

            if ((frame.ActualTheme.PageTransition ?? PageTransition.Slide).MovesPageBeneath)
            {
                this._leavingAnimator = leavingPage.GetOrCreateAnimator();
                this._leavingAnimator.Opacity = 1;
                this._leavingAnimator.Translation = Point.Zero;
            }
        }

        /// <summary>
        /// The page arriving.
        /// </summary>
        public Page Page { get; }

        internal Page LeavingPage { get; }

        internal NavigationMode Mode { get; }

        /// <summary>
        /// How far the page has slid in: 0 (none of it shows) to 1 (all of it).
        /// </summary>
        public float Progress
        {
            get
            {
                return this._progress;
            }

            set
            {
                if (this._isEnded)
                {
                    return;
                }

                this._progress = Math.Clamp(value, 0, 1);
                this._animator.Translation = new Point(this._offset.X * (1 - this._progress), 0);
                this._leavingAnimator?.Translation = this.LeavingPosition(this._progress);
            }
        }

        /// <summary>
        /// Slides the rest of the way in, in the share of the frame's
        /// <see cref="Frame.TransitionDuration"/> left. The page is the frame's
        /// current one (and hears it's navigated to) as it starts.
        /// </summary>
        public async Task CompleteAsync()
        {
            if (this._isEnded)
            {
                return;
            }

            this._isEnded = true;
            this._frame.OnPageSwitchCompleting(this);
            var duration = this.Remaining(1 - this._progress);
            await Task.WhenAll(
                this._animator.TranslateTo(Point.Zero, duration, DecelerateEasing).Completion,
                this.MoveLeavingPage(this.LeavingPosition(1), duration));
            this._frame.OnPageSwitchEnded(this);
        }

        /// <summary>
        /// Slides back out; the current page stays.
        /// </summary>
        public async Task CancelAsync()
        {
            if (this._isEnded)
            {
                return;
            }

            this._isEnded = true;
            var duration = this.Remaining(this._progress);
            await Task.WhenAll(
                this._animator.TranslateTo(this._offset, duration, DecelerateEasing).Completion,
                this.MoveLeavingPage(Point.Zero, duration));
            this._frame.OnPageSwitchEnded(this);
        }

        // The page leaving stays edge to edge with the page arriving
        private Point LeavingPosition(
            float progress)
        {
            return new Point(-this._offset.X * progress, 0);
        }

        private Task MoveLeavingPage(
            Point position,
            TimeSpan duration)
        {
            return this._leavingAnimator?.TranslateTo(position, duration, DecelerateEasing).Completion ?? Task.CompletedTask;
        }

        private TimeSpan Remaining(
            float share)
        {
            return TimeSpan.FromTicks((long)(this._frame.TransitionDuration.Ticks * Math.Clamp(share, 0, 1)));
        }
    }

    /// <summary>
    /// A frame's current page pulled sideways by a gesture with nowhere to go
    /// (see <see cref="Frame.BeginBounce"/>), such as past the last tab: it
    /// moves by the <see cref="Offset"/>, then springs back.
    /// </summary>
    public sealed class PageBounce
    {
        private static readonly EasingFunction SpringBackEasing = new CubicEase(EasingMode.EaseOut);

        private readonly Frame _frame;
        private readonly Page _page;
        private readonly Animator _animator;
        private float _offset;
        private bool _isEnded;

        internal PageBounce(
            Frame frame,
            Page page)
        {
            this._frame = frame;
            this._page = page;
            this._animator = page.GetOrCreateAnimator();
            this._animator.Translation = Point.Zero;
            PageTransition.ShowEdgeShadow(page);
        }

        /// <summary>
        /// How far the page is pulled, in points: to the right for a positive
        /// offset. The gesture damps it, so the page resists the further it's
        /// pulled.
        /// </summary>
        public float Offset
        {
            get
            {
                return this._offset;
            }

            set
            {
                if (this._isEnded)
                {
                    return;
                }

                this._offset = value;
                this._animator.Translation = new Point(value, 0);
            }
        }

        /// <summary>
        /// Springs the page back into place, in the frame's
        /// <see cref="Frame.TransitionDuration"/>.
        /// </summary>
        public async Task ReleaseAsync()
        {
            if (this._isEnded)
            {
                return;
            }

            this._isEnded = true;
            await this._animator.TranslateTo(Point.Zero, this._frame.TransitionDuration, SpringBackEasing);
            PageTransition.HideEdgeShadow(this._page);

            this._frame.OnPageBounceEnded(this);
        }
    }
}
