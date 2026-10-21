using System;
using System.Numerics;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// A scale and opacity that an owner-drawn control applies to its own
    /// drawing, animated frame by frame on the UI thread. Unlike an
    /// <see cref="Animator"/>, it needs no native layer, so the control stays
    /// lightweight. Use it for small effects such as press feedback; create
    /// one with <see cref="Controls.UserControl.CreateDrawnAnimator"/>.
    /// </summary>
    public sealed class DrawnAnimator
    {
        private readonly Action _invalidate;
        private readonly Channel _scale = new Channel(1);
        private readonly Channel _opacity = new Channel(1);
        private bool _isFrameRequested;

        internal DrawnAnimator(
            Action invalidate)
        {
            this._invalidate = invalidate ?? throw new ArgumentNullException(nameof(invalidate));
        }

        // One animated number
        private sealed class Channel
        {
            public Channel(float value)
            {
                this.Value = value;
            }

            public float Value;
            public float From;
            public float To;
            public TimeSpan Start;
            public TimeSpan Duration;
            public EasingFunction Easing;
            public TaskCompletionSource<bool> Completion;

            public bool IsRunning => null != this.Completion;
        }

        /// <summary>
        /// The scale, around the center of the control. Setting it stops its
        /// animation.
        /// </summary>
        public float Scale
        {
            get => this._scale.Value;
            set => this.Set(this._scale, value);
        }

        /// <summary>
        /// The opacity of the control's drawing. Setting it stops its animation.
        /// </summary>
        public float Opacity
        {
            get => this._opacity.Value;
            set => this.Set(this._opacity, value);
        }

        public AnimationHandle ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction easing = null)
        {
            return this.Animate(this._scale, scale, duration, easing);
        }

        public AnimationHandle FadeTo(
            float opacity,
            TimeSpan duration,
            EasingFunction easing = null)
        {
            return this.Animate(this._opacity, opacity, duration, easing);
        }

        /// <summary>
        /// Stops both animations where they are.
        /// </summary>
        public void Stop()
        {
            Finish(this._scale, false);
            Finish(this._opacity, false);
        }

        /// <summary>
        /// Applies the scale (around the center of the bounds) and opacity to
        /// the context until the returned scope is disposed.
        /// </summary>
        internal Scope Apply(
            DrawingContext context,
            Rect bounds)
        {
            var scope = new Scope();
            float scale = this._scale.Value;
            if (scale != 1)
            {
                scope.Transform = context.PushTransform(Matrix3x2.CreateScale(scale, new Vector2(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2))));
            }

            float opacity = this._opacity.Value;
            if (opacity != 1)
            {
                scope.Opacity = context.PushOpacity(opacity);
            }

            return scope;
        }

        internal struct Scope : IDisposable
        {
            public DrawingContextState Transform;
            public DrawingContextState Opacity;

            public void Dispose()
            {
                this.Opacity.Dispose();
                this.Transform.Dispose();
            }
        }

        private void Set(
            Channel channel,
            float value)
        {
            Finish(channel, false);
            channel.Value = value;
            this._invalidate();
        }

        private AnimationHandle Animate(
            Channel channel,
            float to,
            TimeSpan duration,
            EasingFunction easing)
        {
            Finish(channel, false);

            channel.From = channel.Value;
            channel.To = to;
            channel.Start = Now;
            channel.Duration = duration;
            channel.Easing = easing ?? EasingFunction.Linear;
            channel.Completion = new TaskCompletionSource<bool>();
            var completion = channel.Completion;

            this.RequestFrame();
            return new AnimationHandle(completion.Task, () =>
            {
                if (channel.Completion == completion)
                {
                    Finish(channel, false);
                }
            });
        }

        private static TimeSpan Now
        {
            get
            {
                return TimeSpan.FromSeconds(System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency);
            }
        }

        private void RequestFrame()
        {
            if (!this._isFrameRequested)
            {
                this._isFrameRequested = true;
                Application.Current.RequestAnimationFrame(this.OnFrame);
            }
        }

        private void OnFrame(
            TimeSpan frameTime)
        {
            this._isFrameRequested = false;

            var now = Now;
            bool isRunning = Step(this._scale, now) | Step(this._opacity, now);
            this._invalidate();

            if (isRunning)
            {
                this.RequestFrame();
            }
        }

        // Moves the channel's animation to the time; returns whether it runs on
        private static bool Step(
            Channel channel,
            TimeSpan now)
        {
            if (!channel.IsRunning)
            {
                return false;
            }

            double duration = Math.Max(1, channel.Duration.TotalMilliseconds);
            float progress = (float)Math.Min(1, (now - channel.Start).TotalMilliseconds / duration);
            channel.Value = channel.From + ((channel.To - channel.From) * channel.Easing.Ease(progress));

            if (progress >= 1)
            {
                channel.Value = channel.To;
                Finish(channel, true);
                return false;
            }

            return true;
        }

        private static void Finish(
            Channel channel,
            bool isFinished)
        {
            var completion = channel.Completion;
            channel.Completion = null;
            completion?.TrySetResult(isFinished);
        }
    }
}
