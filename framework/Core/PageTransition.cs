using System;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// How a page arrives or leaves a <see cref="Frame"/>: the built-in
    /// slides and fade, or any other a control library or app defines by
    /// overriding <see cref="PlayAsync"/>.
    /// </summary>
    public abstract class PageTransition
    {
        /// <summary>
        /// Slides in from the side, and back out (the default).
        /// </summary>
        public static PageTransition Slide { get; } = new SlideTransition(vertical: false);

        /// <summary>
        /// Slides up from the bottom, and back down (as modal pages do).
        /// </summary>
        public static PageTransition SlideUp { get; } = new SlideTransition(vertical: true);

        /// <summary>
        /// Fades in, and out.
        /// </summary>
        public static PageTransition Fade { get; } = new FadeTransition();

        /// <summary>
        /// Shows the page at once.
        /// </summary>
        public static PageTransition None { get; } = new NoTransition();

        /// <summary>
        /// Moves the page on top in (as a page arrives) or out (as it leaves,
        /// uncovering the one before). The frame hides the page underneath when
        /// the task completes.
        /// </summary>
        public abstract Task PlayAsync(
            PageTransitionContext context);

        private sealed class SlideTransition : PageTransition
        {
            private static readonly EasingFunction DecelerateEasing = new CubicEase(EasingMode.EaseOut);
            private static readonly EasingFunction AccelerateEasing = new CubicEase(EasingMode.EaseIn);

            private readonly bool _vertical;

            public SlideTransition(
                bool vertical)
            {
                this._vertical = vertical;
            }

            public override async Task PlayAsync(
                PageTransitionContext context)
            {
                var size = context.Size;
                var offset = this._vertical
                    ? new Point(0, size.Height)
                    : new Point(context.FromStart ? -size.Width : size.Width, 0);

                var animator = context.Animator;
                animator.Opacity = 1;
                if (context.IsLeaving)
                {
                    animator.Translation = Point.Zero;
                    await animator.TranslateTo(offset, context.Duration, AccelerateEasing);
                    animator.Translation = Point.Zero;
                }
                else
                {
                    animator.Translation = offset;
                    await animator.TranslateTo(Point.Zero, context.Duration, DecelerateEasing);
                }
            }
        }

        private sealed class FadeTransition : PageTransition
        {
            private static readonly EasingFunction DecelerateEasing = new CubicEase(EasingMode.EaseOut);
            private static readonly EasingFunction AccelerateEasing = new CubicEase(EasingMode.EaseIn);

            public override async Task PlayAsync(
                PageTransitionContext context)
            {
                var animator = context.Animator;
                animator.Translation = Point.Zero;
                if (context.IsLeaving)
                {
                    await animator.FadeTo(0, context.Duration, AccelerateEasing);
                }
                else
                {
                    await animator.FadeFrom(0, context.Duration, DecelerateEasing);
                }

                animator.Opacity = 1;
            }
        }

        private sealed class NoTransition : PageTransition
        {
            public override Task PlayAsync(
                PageTransitionContext context)
            {
                return Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// What a <see cref="PageTransition"/> moves, and how far and long.
    /// </summary>
    public sealed class PageTransitionContext
    {
        private Animator _animator;

        internal PageTransitionContext(
            Page page,
            bool isLeaving,
            bool fromStart,
            Size size,
            TimeSpan duration)
        {
            this.Page = page;
            this.IsLeaving = isLeaving;
            this.FromStart = fromStart;
            this.Size = size;
            this.Duration = duration;
        }

        /// <summary>
        /// The page on top: the one arriving, or (going back) the one leaving.
        /// </summary>
        public Page Page { get; }

        /// <summary>
        /// Whether the page is leaving, uncovering the page before it, rather
        /// than arriving over the current one.
        /// </summary>
        public bool IsLeaving { get; }

        /// <summary>
        /// Whether the page arrives from the start side (as when tabs switch
        /// back), rather than the end.
        /// </summary>
        public bool FromStart { get; }

        /// <summary>
        /// The frame's size.
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// How long the transition takes: the frame's
        /// <see cref="Frame.TransitionDuration"/>.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// The page's animator.
        /// </summary>
        public Animator Animator
        {
            get
            {
                return this._animator ??= this.Page.GetOrCreateAnimator();
            }
        }
    }
}
