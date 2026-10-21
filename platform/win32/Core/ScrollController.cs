using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Win32.Graphics.DirectComposition;

namespace XForms.Win32
{
    /// <summary>
    /// The content a <see cref="ScrollController"/> scrolls.
    /// </summary>
    internal unsafe interface IScrollContent
    {
        /// <summary>
        /// The visual that moves as the content scrolls.
        /// </summary>
        IDCompositionVisual2* ContentVisual { get; }

        /// <summary>
        /// Whether the content scrolls sideways. The viewport and content
        /// "heights" are then widths.
        /// </summary>
        bool IsHorizontal { get; }

        float ViewportHeight { get; }

        float ContentHeight { get; }

        /// <summary>
        /// Called before the content scrolls across the range of offsets, so a
        /// virtualized list can realize the rows that will come into view.
        /// </summary>
        void OnScrollRange(float fromOffset, float toOffset);

        /// <summary>
        /// Called when the content comes to rest.
        /// </summary>
        void OnScrollSettled();
    }

    /// <summary>
    /// Content that refreshes when pulled down past its top (with touch).
    /// </summary>
    internal interface IPullToRefresh
    {
        bool IsPullEnabled { get; }

        /// <summary>
        /// The content is pulled this far (in DIPs) past its top.
        /// </summary>
        void OnPull(float distance);

        void OnPullReleased();
    }

    /// <summary>
    /// Scrolling (vertical, or horizontal) that runs on the compositor: wheel and touchpad steps
    /// glide, drags track the finger, and flings decelerate. The offset is
    /// evaluated from the same curve the compositor runs, so hit testing and
    /// realization stay in step with what is on screen.
    /// </summary>
    internal sealed unsafe class ScrollController
    {
        private const double WheelDurationSeconds = 0.15;
        private const double MinimumFlingSeconds = 0.3;
        private const double MaximumFlingSeconds = 2.5;
        private const float MinimumFlingVelocity = 150; // dips per second

        private readonly IScrollContent _content;
        private float _from;
        private float _to;
        private double _startTime;
        private double _duration;
        private int _generation;

        public ScrollController(
            IScrollContent content)
        {
            this._content = content ?? throw new ArgumentNullException(nameof(content));
        }

        private static double Now
        {
            get
            {
                return Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
            }
        }

        private float MaximumOffset
        {
            get
            {
                return Math.Max(0, this._content.ContentHeight - this._content.ViewportHeight);
            }
        }

        public bool IsHorizontal
        {
            get
            {
                return this._content.IsHorizontal;
            }
        }

        public bool IsAnimating
        {
            get
            {
                return (this._duration > 0) && ((Now - this._startTime) < this._duration);
            }
        }

        /// <summary>
        /// The offset currently on screen.
        /// </summary>
        public float Offset
        {
            get
            {
                if (!this.IsAnimating)
                {
                    return this._to;
                }

                // Ease out (cubic), as the compositor runs it
                double u = (Now - this._startTime) / this._duration;
                double eased = 1 - Math.Pow(1 - u, 3);
                return (float)(this._from + ((this._to - this._from) * eased));
            }
        }

        /// <summary>
        /// Where the content will come to rest.
        /// </summary>
        public float TargetOffset
        {
            get
            {
                return this._to;
            }
        }

        public void ScrollBy(
            float delta)
        {
            // Successive wheel steps add up from where the last one was heading
            this.AnimateTo(this._to + delta, WheelDurationSeconds);
        }

        public void ScrollTo(
            float offset,
            bool animated)
        {
            if (animated)
            {
                this.AnimateTo(offset, 0.3);
            }
            else
            {
                this.SetOffset(offset);
            }
        }

        /// <summary>
        /// Moves the content immediately, such as while it follows a finger.
        /// </summary>
        public void SetOffset(
            float offset)
        {
            this._generation += 1;
            this._duration = 0;

            float from = this.Offset;
            this._from = this._to = this.Clamp(offset);

            this._content.OnScrollRange(Math.Min(from, this._to), Math.Max(from, this._to));
            this.ApplyStaticOffset();
        }

        /// <summary>
        /// Keeps the offset within the content after the content changes size.
        /// </summary>
        public void Coerce()
        {
            if (!this.IsAnimating)
            {
                this.SetOffset(this._to);
            }
        }

        /// <summary>
        /// Lets the content glide to a stop after a drag released at the velocity
        /// (in dips per second, positive when the content moves up).
        /// </summary>
        public void Fling(
            float velocity)
        {
            if (Math.Abs(velocity) < MinimumFlingVelocity)
            {
                this._content.OnScrollSettled();
                return;
            }

            // An ease-out cubic starts at 3 * distance / duration, so match that
            // to the release velocity
            double duration = Math.Clamp(Math.Abs(velocity) / 2000.0, MinimumFlingSeconds, MaximumFlingSeconds);
            float distance = (float)(velocity * duration / 3);

            this.AnimateTo(this._to + distance, duration);
        }

        private void AnimateTo(
            float offset,
            double duration)
        {
            float from = this.Offset;
            float to = this.Clamp(offset);

            if (Math.Abs(to - from) < 0.5f)
            {
                this.SetOffset(to);
                this._content.OnScrollSettled();
                return;
            }

            this._from = from;
            this._to = to;
            this._startTime = Now;
            this._duration = duration;
            int generation = ++this._generation;

            // Realize everything the content will pass over before it moves
            this._content.OnScrollRange(Math.Min(from, to), Math.Max(from, to));

            var host = Win32Host.Current;
            float scale = host.Scale;
            float t = (float)duration;
            float start = -from * scale;
            float delta = -(to - from) * scale;

            IDCompositionAnimation* animation;
            host.CompositionDevice->CreateAnimation(&animation);
            animation->AddCubic(0, start, 3 * delta / t, -3 * delta / (t * t), delta / (t * t * t));
            animation->End(duration, start + delta);
            if (this._content.IsHorizontal)
            {
                this._content.ContentVisual->SetOffsetX(animation);
            }
            else
            {
                this._content.ContentVisual->SetOffsetY(animation);
            }
            animation->Release();

            host.Flush();

            Task.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(
                (task) => host.Post(() =>
                {
                    if (generation == this._generation)
                    {
                        this._duration = 0;
                        this.ApplyStaticOffset();
                        this._content.OnScrollSettled();
                    }
                }),
                TaskScheduler.Default);
        }

        private void ApplyStaticOffset()
        {
            var host = Win32Host.Current;
            float offset = (float)Math.Round(-this._to * host.Scale);
            if (this._content.IsHorizontal)
            {
                this._content.ContentVisual->SetOffsetX(offset);
            }
            else
            {
                this._content.ContentVisual->SetOffsetY(offset);
            }

            host.Flush();
        }

        private float Clamp(
            float offset)
        {
            return Math.Max(0, Math.Min(offset, this.MaximumOffset));
        }
    }
}
