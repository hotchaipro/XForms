using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// A render property of an animator's layer, as one number.
    /// </summary>
    public enum AnimationChannel
    {
        TranslationX = 0,
        TranslationY,
        ScaleX,
        ScaleY,
        /// <summary>Degrees, clockwise.</summary>
        Rotation,
        Opacity,
        ClipLeft,
        ClipTop,
        ClipRight,
        ClipBottom,
    }

    /// <summary>
    /// When and how an animation plays.
    /// </summary>
    public sealed class AnimationTiming
    {
        public TimeSpan Duration { get; init; }

        public TimeSpan Delay { get; init; }

        public EasingFunction Easing { get; init; } = EasingFunction.Linear;

        /// <summary>
        /// Plays again and again until stopped.
        /// </summary>
        public bool Loop { get; init; }

        /// <summary>
        /// Each play runs forward, then backward.
        /// </summary>
        public bool AutoReverse { get; init; }

        /// <summary>
        /// The progress (0 to 1, before easing) of one play at a time since the
        /// animation started, or null before its delay ends.
        /// </summary>
        internal float? Progress(
            TimeSpan elapsed)
        {
            var active = elapsed - this.Delay;
            if (active < TimeSpan.Zero)
            {
                return null;
            }

            double duration = Math.Max(1, this.Duration.TotalMilliseconds);
            double plays = active.TotalMilliseconds / duration;
            double cycle = this.AutoReverse ? 2 : 1;

            if (!this.Loop && (plays >= cycle))
            {
                return this.AutoReverse ? 0 : 1;
            }

            double position = plays % cycle;
            return (float)((position <= 1) ? position : 2 - position);
        }

        internal TimeSpan TotalDuration
        {
            get
            {
                return this.Delay + TimeSpan.FromTicks(this.Duration.Ticks * (this.AutoReverse ? 2 : 1));
            }
        }
    }

    /// <summary>
    /// One animator's native layer: a platform's compositor layer, or a view
    /// whose render properties it owns. Platforms play animations on it.
    /// </summary>
    public interface IAnimationLayer
    {
        /// <summary>
        /// Sets the channel's value now, ending any animation of it.
        /// </summary>
        void SetValue(AnimationChannel channel, float value);

        /// <summary>
        /// Animates the channel from one value to another.
        /// </summary>
        void Animate(AnimationChannel channel, float from, float to, AnimationTiming timing);

        /// <summary>
        /// The point that scale and rotation are around, relative to the
        /// layer's size: (0.5, 0.5) is the center.
        /// </summary>
        Point TransformOrigin { set; }

        /// <summary>
        /// Whether the clip channels limit what the layer shows.
        /// </summary>
        bool IsClipEnabled { set; }
    }

    /// <summary>
    /// The result of starting an animation: await it for whether it finished
    /// (true) or was interrupted (false), or cancel it.
    /// </summary>
    public sealed class AnimationHandle
    {
        internal static readonly AnimationHandle Completed = new AnimationHandle(Task.FromResult(true), null);

        private readonly Action _cancel;

        internal AnimationHandle(
            Task<bool> completion,
            Action cancel)
        {
            this.Completion = completion;
            this._cancel = cancel;
        }

        /// <summary>
        /// Completes with true when the animation finishes, or false when it's
        /// stopped or replaced first. A looping animation completes only when
        /// stopped.
        /// </summary>
        public Task<bool> Completion
        {
            get;
        }

        public bool IsRunning
        {
            get
            {
                return !this.Completion.IsCompleted;
            }
        }

        /// <summary>
        /// Stops the animation where it is.
        /// </summary>
        public void Cancel()
        {
            this._cancel?.Invoke();
        }

        public TaskAwaiter<bool> GetAwaiter()
        {
            return this.Completion.GetAwaiter();
        }
    }

    /// <summary>
    /// A view's render properties: translation, scale, rotation, opacity and
    /// clip, around a transform origin. They don't affect layout. Set them
    /// directly, or animate them with the <c>To</c> and <c>From</c> methods;
    /// the platform's compositor plays the animations.
    /// </summary>
    /// <remarks>
    /// A view gets an animator when it's created: the app's through
    /// <see cref="View.Animator"/>, a control's own through
    /// <see cref="Controls.Control.CreateOwnAnimator"/>. Each animator has its
    /// own native layer, nested: the app's outside, a control's own inside.
    /// Setting a value stops that value's animation.
    /// </remarks>
    public sealed class Animator
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private const int ChannelCount = 10;

        private readonly float[] _values = { 0, 0, 1, 1, 0, 1, 0, 0, 0, 0 };
        private readonly Running[] _running = new Running[ChannelCount];
        private View _view;
        private IAnimationLayer _layer;
        private Point _transformOrigin = new Point(0.5f, 0.5f);
        private bool _isClipEnabled;

        public Animator()
        {
        }

        // An animation in progress on a channel
        private sealed class Running
        {
            public float From;
            public float To;
            public AnimationTiming Timing;
            public TimeSpan Start;
            public TaskCompletionSource<bool> Completion;
        }

        #region Values

        public Point Translation
        {
            get => new Point(this.Get(AnimationChannel.TranslationX), this.Get(AnimationChannel.TranslationY));
            set
            {
                this.Set(AnimationChannel.TranslationX, value.X);
                this.Set(AnimationChannel.TranslationY, value.Y);
            }
        }

        /// <summary>
        /// A uniform scale (both axes).
        /// </summary>
        public float Scale
        {
            get => this.Get(AnimationChannel.ScaleX);
            set
            {
                this.Set(AnimationChannel.ScaleX, value);
                this.Set(AnimationChannel.ScaleY, value);
            }
        }

        public float ScaleX
        {
            get => this.Get(AnimationChannel.ScaleX);
            set => this.Set(AnimationChannel.ScaleX, value);
        }

        public float ScaleY
        {
            get => this.Get(AnimationChannel.ScaleY);
            set => this.Set(AnimationChannel.ScaleY, value);
        }

        public Angle Rotation
        {
            get => Angle.FromDegrees(this.Get(AnimationChannel.Rotation));
            set => this.Set(AnimationChannel.Rotation, (float)value.Degrees);
        }

        public float Opacity
        {
            get => this.Get(AnimationChannel.Opacity);
            set => this.Set(AnimationChannel.Opacity, value);
        }

        /// <summary>
        /// The part of the view shown (in its own coordinates), or null (the
        /// default) for all of it.
        /// </summary>
        public Rect? Clip
        {
            get
            {
                if (!this._isClipEnabled)
                {
                    return null;
                }

                float left = this.Get(AnimationChannel.ClipLeft);
                float top = this.Get(AnimationChannel.ClipTop);
                return new Rect(left, top, this.Get(AnimationChannel.ClipRight) - left, this.Get(AnimationChannel.ClipBottom) - top);
            }

            set
            {
                this.SetClipEnabled(value.HasValue);
                if (value.HasValue)
                {
                    this.Set(AnimationChannel.ClipLeft, value.Value.X);
                    this.Set(AnimationChannel.ClipTop, value.Value.Y);
                    this.Set(AnimationChannel.ClipRight, value.Value.Right);
                    this.Set(AnimationChannel.ClipBottom, value.Value.Bottom);
                }
            }
        }

        /// <summary>
        /// The point scale and rotation are around, relative to the view's
        /// size: (0.5, 0.5), the center, by default.
        /// </summary>
        public Point TransformOrigin
        {
            get => this._transformOrigin;
            set
            {
                this._transformOrigin = value;
                if (null != this._layer)
                {
                    this._layer.TransformOrigin = value;
                }
            }
        }

        #endregion Values

        #region Animations

        public AnimationHandle TranslateTo(Point point, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default, bool loop = false, bool autoReverse = false)
        {
            var timing = Timing(duration, easing, delay, loop, autoReverse);
            return Combine(
                this.Animate(AnimationChannel.TranslationX, this.Get(AnimationChannel.TranslationX), point.X, timing),
                this.Animate(AnimationChannel.TranslationY, this.Get(AnimationChannel.TranslationY), point.Y, timing));
        }

        /// <summary>
        /// Animates the translation from the point to its current value, as for
        /// a layout change: <c>TranslateFrom(oldPosition - newPosition)</c>.
        /// </summary>
        public AnimationHandle TranslateFrom(Point point, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default)
        {
            var timing = Timing(duration, easing, delay, false, false);
            var target = this.Translation;
            return Combine(
                this.Animate(AnimationChannel.TranslationX, point.X, target.X, timing),
                this.Animate(AnimationChannel.TranslationY, point.Y, target.Y, timing));
        }

        /// <summary>
        /// Animates the translation from an offset back to where it's heading,
        /// continuing from where it is now, for a layout change.
        /// </summary>
        internal AnimationHandle TranslateByOffset(Point offset, TimeSpan duration, EasingFunction easing)
        {
            var timing = Timing(duration, easing, default, false, false);
            float toX = this._values[(int)AnimationChannel.TranslationX];
            float toY = this._values[(int)AnimationChannel.TranslationY];
            float fromX = this.Get(AnimationChannel.TranslationX) + offset.X;
            float fromY = this.Get(AnimationChannel.TranslationY) + offset.Y;
            return Combine(
                this.Animate(AnimationChannel.TranslationX, fromX, toX, timing),
                this.Animate(AnimationChannel.TranslationY, fromY, toY, timing));
        }

        public AnimationHandle ScaleTo(float scale, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default, bool loop = false, bool autoReverse = false)
        {
            var timing = Timing(duration, easing, delay, loop, autoReverse);
            return Combine(
                this.Animate(AnimationChannel.ScaleX, this.Get(AnimationChannel.ScaleX), scale, timing),
                this.Animate(AnimationChannel.ScaleY, this.Get(AnimationChannel.ScaleY), scale, timing));
        }

        public AnimationHandle ScaleFrom(float scale, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default)
        {
            var timing = Timing(duration, easing, delay, false, false);
            return Combine(
                this.Animate(AnimationChannel.ScaleX, scale, this.Get(AnimationChannel.ScaleX), timing),
                this.Animate(AnimationChannel.ScaleY, scale, this.Get(AnimationChannel.ScaleY), timing));
        }

        public AnimationHandle RotateTo(Angle angle, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default, bool loop = false, bool autoReverse = false)
        {
            return this.Animate(AnimationChannel.Rotation, this.Get(AnimationChannel.Rotation), (float)angle.Degrees, Timing(duration, easing, delay, loop, autoReverse));
        }

        /// <summary>
        /// Animates the rotation between two angles, for example a looping
        /// spin: <c>RotateFrom(Angle.Zero, Angle.FromDegrees(360), period, loop: true)</c>.
        /// </summary>
        public AnimationHandle RotateFrom(Angle from, Angle to, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default, bool loop = false, bool autoReverse = false)
        {
            return this.Animate(AnimationChannel.Rotation, (float)from.Degrees, (float)to.Degrees, Timing(duration, easing, delay, loop, autoReverse));
        }

        public AnimationHandle FadeTo(float opacity, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default, bool loop = false, bool autoReverse = false)
        {
            return this.Animate(AnimationChannel.Opacity, this.Get(AnimationChannel.Opacity), opacity, Timing(duration, easing, delay, loop, autoReverse));
        }

        public AnimationHandle FadeFrom(float opacity, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default)
        {
            return this.Animate(AnimationChannel.Opacity, opacity, this.Get(AnimationChannel.Opacity), Timing(duration, easing, delay, false, false));
        }

        /// <summary>
        /// Animates the clip to a rectangle (in the view's coordinates).
        /// </summary>
        public AnimationHandle RevealTo(Rect clip, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default)
        {
            var timing = Timing(duration, easing, delay, false, false);
            var from = this.Clip ?? this.FullBounds();
            this.SetClipEnabled(true);
            return Combine(
                this.Animate(AnimationChannel.ClipLeft, from.X, clip.X, timing),
                this.Animate(AnimationChannel.ClipTop, from.Y, clip.Y, timing),
                this.Animate(AnimationChannel.ClipRight, from.Right, clip.Right, timing),
                this.Animate(AnimationChannel.ClipBottom, from.Bottom, clip.Bottom, timing));
        }

        /// <summary>
        /// Reveals the whole view, growing the clip from a rectangle (such as
        /// an empty one at an edge); the clip is removed when it completes.
        /// </summary>
        public AnimationHandle RevealFrom(Rect clip, TimeSpan duration, EasingFunction easing = null, TimeSpan delay = default)
        {
            var timing = Timing(duration, easing, delay, false, false);
            var to = this.FullBounds();
            this.SetClipEnabled(true);
            var handle = Combine(
                this.Animate(AnimationChannel.ClipLeft, clip.X, to.X, timing),
                this.Animate(AnimationChannel.ClipTop, clip.Y, to.Y, timing),
                this.Animate(AnimationChannel.ClipRight, clip.Right, to.Right, timing),
                this.Animate(AnimationChannel.ClipBottom, clip.Bottom, to.Bottom, timing));

            handle.Completion.ContinueWith(
                (task) =>
                {
                    if (task.Result)
                    {
                        this.SetClipEnabled(false);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);

            return handle;
        }

        /// <summary>
        /// Stops every animation, leaving the values where they are.
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < ChannelCount; i += 1)
            {
                this.StopChannel((AnimationChannel)i);
            }
        }

        #endregion Animations

        #region Engine

        private static AnimationTiming Timing(TimeSpan duration, EasingFunction easing, TimeSpan delay, bool loop, bool autoReverse)
        {
            return new AnimationTiming()
            {
                Duration = duration,
                Easing = easing ?? EasingFunction.Linear,
                Delay = delay,
                Loop = loop,
                AutoReverse = autoReverse,
            };
        }

        private static AnimationHandle Combine(params AnimationHandle[] handles)
        {
            // Completes on the UI thread, where the channels complete
            var completion = new TaskCompletionSource<bool>();
            int remaining = handles.Length;
            bool isFinished = true;
            foreach (var handle in handles)
            {
                handle.Completion.ContinueWith(
                    (task) =>
                    {
                        isFinished &= task.Result;
                        remaining -= 1;
                        if (0 == remaining)
                        {
                            completion.TrySetResult(isFinished);
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
            }

            return new AnimationHandle(completion.Task, () =>
            {
                foreach (var handle in handles)
                {
                    handle.Cancel();
                }
            });
        }

        private Rect FullBounds()
        {
            var bounds = this._view?.Bounds ?? new Rect(0, 0, 0, 0);
            return new Rect(0, 0, bounds.Width, bounds.Height);
        }

        private void SetClipEnabled(
            bool isEnabled)
        {
            if (isEnabled && !this._isClipEnabled)
            {
                // Start from the whole view
                var full = this.FullBounds();
                this._values[(int)AnimationChannel.ClipLeft] = full.X;
                this._values[(int)AnimationChannel.ClipTop] = full.Y;
                this._values[(int)AnimationChannel.ClipRight] = full.Right;
                this._values[(int)AnimationChannel.ClipBottom] = full.Bottom;
            }

            this._isClipEnabled = isEnabled;
            if (null != this._layer)
            {
                this._layer.IsClipEnabled = isEnabled;
                for (int i = (int)AnimationChannel.ClipLeft; i <= (int)AnimationChannel.ClipBottom; i += 1)
                {
                    this._layer.SetValue((AnimationChannel)i, this._values[i]);
                }
            }
        }

        // The channel's value now, including any animation in progress
        private float Get(
            AnimationChannel channel)
        {
            var running = this._running[(int)channel];
            if (null == running)
            {
                return this._values[(int)channel];
            }

            var progress = running.Timing.Progress(Clock.Elapsed - running.Start);
            if (null == progress)
            {
                return running.From;
            }

            return running.From + ((running.To - running.From) * running.Timing.Easing.Ease(progress.Value));
        }

        private void Set(
            AnimationChannel channel,
            float value)
        {
            this.StopChannel(channel);
            this._values[(int)channel] = value;
            this._layer?.SetValue(channel, value);
        }

        private void StopChannel(
            AnimationChannel channel)
        {
            var running = this._running[(int)channel];
            if (null == running)
            {
                return;
            }

            float value = this.Get(channel);
            this._running[(int)channel] = null;
            this._values[(int)channel] = value;
            this._layer?.SetValue(channel, value);
            running.Completion.TrySetResult(false);
        }

        private AnimationHandle Animate(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            this.StopChannel(channel);

            var running = new Running()
            {
                From = from,
                To = to,
                Timing = timing,
                Start = Clock.Elapsed,
                // Completed on the UI thread, so continuations that run
                // synchronously can touch views
                Completion = new TaskCompletionSource<bool>(),
            };

            this._running[(int)channel] = running;
            this._values[(int)channel] = timing.AutoReverse ? from : to;

            if (null != this._layer)
            {
                this._layer.Animate(channel, from, to, timing);
            }

            if (!timing.Loop)
            {
                // Settle on the final value when it ends, unless replaced first
                var application = Application.Current;
                Task.Delay(timing.TotalDuration).ContinueWith(
                    (task) => application.Dispatcher.Post(() =>
                    {
                        if (this._running[(int)channel] == running)
                        {
                            this._running[(int)channel] = null;
                            this._layer?.SetValue(channel, this._values[(int)channel]);
                            running.Completion.TrySetResult(true);
                        }
                    }),
                    TaskScheduler.Default);
            }

            return new AnimationHandle(running.Completion.Task, () =>
            {
                if (this._running[(int)channel] == running)
                {
                    this.StopChannel(channel);
                }
            });
        }

        internal void Attach(
            View view)
        {
            if (null != this._view)
            {
                throw new InvalidOperationException("An Animator belongs to one view.");
            }

            this._view = view;
        }

        /// <summary>
        /// Gives the animator its native layer, once the view is attached, and
        /// applies its values and any animations started before then.
        /// </summary>
        internal void Bind(
            IAnimationLayer layer)
        {
            this._layer = layer;
            layer.TransformOrigin = this._transformOrigin;
            layer.IsClipEnabled = this._isClipEnabled;

            for (int i = 0; i < ChannelCount; i += 1)
            {
                var channel = (AnimationChannel)i;
                var running = this._running[i];
                if (null != running)
                {
                    // Continue an animation started before attach, from now
                    var elapsed = Clock.Elapsed - running.Start;
                    var remaining = new AnimationTiming()
                    {
                        Duration = running.Timing.Duration,
                        Easing = running.Timing.Easing,
                        Delay = (running.Timing.Delay > elapsed) ? running.Timing.Delay - elapsed : TimeSpan.Zero,
                        Loop = running.Timing.Loop,
                        AutoReverse = running.Timing.AutoReverse,
                    };
                    layer.Animate(channel, running.From, running.To, remaining);
                }
                else if (this._values[i] != DefaultValue(channel))
                {
                    layer.SetValue(channel, this._values[i]);
                }
            }
        }

        private static float DefaultValue(
            AnimationChannel channel)
        {
            return ((channel == AnimationChannel.ScaleX) || (channel == AnimationChannel.ScaleY) || (channel == AnimationChannel.Opacity)) ? 1 : 0;
        }

        #endregion Engine
    }
}
