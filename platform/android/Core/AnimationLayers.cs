using System;
using Android.Animation;
using Android.Views;
using AndroidContext = Android.Content.Context;
using AndroidRect = Android.Graphics.Rect;
using AndroidView = Android.Views.View;

namespace XForms.Android
{
    /// <summary>
    /// An animator's layer: a view's render properties (translation, scale,
    /// rotation, alpha and clip bounds), set directly or animated with property
    /// animators.
    /// </summary>
    internal sealed class AnimationLayer : IAnimationLayer
    {
        private const int ChannelCount = 10;

        private readonly AndroidView _view;
        private readonly float _density;
        private readonly ValueAnimator[] _animators = new ValueAnimator[ChannelCount];

        // Clip edges, in device-independent pixels
        private readonly float[] _clip = new float[4];
        private Point _transformOrigin = new Point(0.5f, 0.5f);
        private bool _isClipEnabled;

        // The clip is the view's outline while it's enabled; the view's own
        // outline (for ClipToBounds) is restored after
        private ClipOutlineProvider _clipOutline;
        private ViewOutlineProvider _savedOutline;
        private bool _savedClipToOutline;

        public AnimationLayer(
            AndroidView view,
            float density)
        {
            this._view = view;
            this._density = density;
        }

        public AndroidView View
        {
            get
            {
                return this._view;
            }
        }

        public Point TransformOrigin
        {
            set
            {
                this._transformOrigin = value;
                this.UpdatePivot();
            }
        }

        public bool IsClipEnabled
        {
            set
            {
                if (value == this._isClipEnabled)
                {
                    return;
                }

                this._isClipEnabled = value;
                var view = this._view;
                if (value)
                {
                    this._savedOutline = view.OutlineProvider;
                    this._savedClipToOutline = view.ClipToOutline;
                    this._clipOutline ??= new ClipOutlineProvider();
                    view.OutlineProvider = this._clipOutline;
                    view.ClipToOutline = true;
                    this.UpdateClip();
                }
                else
                {
                    view.OutlineProvider = this._savedOutline;
                    view.ClipToOutline = this._savedClipToOutline;
                    this._savedOutline = null;
                }
            }
        }

        /// <summary>
        /// Updates the pivot to the view's size.
        /// </summary>
        public void UpdatePivot()
        {
            this._view.PivotX = this._transformOrigin.X * this._view.Width;
            this._view.PivotY = this._transformOrigin.Y * this._view.Height;
        }

        private void UpdateClip()
        {
            if (!this._isClipEnabled)
            {
                return;
            }

            // NOTE: View.ClipBounds didn't clip on the render thread here (an
            // owner-drawn layout view on API 36); outline clipping does
            this._clipOutline.Rect = new AndroidRect(
                (int)Math.Round(this._clip[0] * this._density),
                (int)Math.Round(this._clip[1] * this._density),
                (int)Math.Round(this._clip[2] * this._density),
                (int)Math.Round(this._clip[3] * this._density));
            this._view.InvalidateOutline();
        }

        private sealed class ClipOutlineProvider : ViewOutlineProvider
        {
            public AndroidRect Rect { get; set; } = new AndroidRect();

            public override void GetOutline(
                AndroidView view,
                global::Android.Graphics.Outline outline)
            {
                var rect = this.Rect;
                if (rect.IsEmpty)
                {
                    // An empty outline doesn't clip; clip to a pixel outside
                    // the view instead, which shows nothing
                    outline.SetRect(-2, -2, -1, -1);
                }
                else
                {
                    outline.SetRect(rect);
                }
            }
        }

        public void SetValue(
            AnimationChannel channel,
            float value)
        {
            this.Cancel(channel);
            this.Apply(channel, value);
        }

        private void Apply(
            AnimationChannel channel,
            float value)
        {
            var view = this._view;
            switch (channel)
            {
                case AnimationChannel.TranslationX: view.TranslationX = value * this._density; break;
                case AnimationChannel.TranslationY: view.TranslationY = value * this._density; break;
                case AnimationChannel.ScaleX: view.ScaleX = value; break;
                case AnimationChannel.ScaleY: view.ScaleY = value; break;
                case AnimationChannel.Rotation: view.Rotation = value; break;
                case AnimationChannel.Opacity: view.Alpha = value; break;
                default:
                    this._clip[channel - AnimationChannel.ClipLeft] = value;
                    this.UpdateClip();
                    break;
            }
        }

        public void Animate(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            this.Cancel(channel);

            ValueAnimator animator;
            string property = PropertyName(channel);
            if (null != property)
            {
                // The view's own property: the render thread can draw it without
                // the view redrawing
                float scale = ((channel == AnimationChannel.TranslationX) || (channel == AnimationChannel.TranslationY)) ? this._density : 1;
                animator = ObjectAnimator.OfFloat(this._view, property, from * scale, to * scale);
            }
            else
            {
                animator = ValueAnimator.OfFloat(from, to);
                animator.Update += (sender, e) => this.Apply(channel, (float)e.Animation.AnimatedValue);
            }

            animator.SetDuration((long)Math.Max(1, timing.Duration.TotalMilliseconds));
            animator.StartDelay = (long)timing.Delay.TotalMilliseconds;
            animator.SetInterpolator(new EasingInterpolator(timing.Easing ?? EasingFunction.Linear));
            if (timing.Loop || timing.AutoReverse)
            {
                animator.RepeatCount = timing.Loop ? ValueAnimator.Infinite : 1;
                animator.RepeatMode = timing.AutoReverse ? ValueAnimatorRepeatMode.Reverse : ValueAnimatorRepeatMode.Restart;
            }

            this._animators[(int)channel] = animator;
            animator.Start();
        }

        private void Cancel(
            AnimationChannel channel)
        {
            var animator = this._animators[(int)channel];
            if (null != animator)
            {
                this._animators[(int)channel] = null;
                animator.Cancel();
                animator.Dispose();
            }
        }

        private static string PropertyName(
            AnimationChannel channel)
        {
            switch (channel)
            {
                case AnimationChannel.TranslationX: return "translationX";
                case AnimationChannel.TranslationY: return "translationY";
                case AnimationChannel.ScaleX: return "scaleX";
                case AnimationChannel.ScaleY: return "scaleY";
                case AnimationChannel.Rotation: return "rotation";
                case AnimationChannel.Opacity: return "alpha";
                default: return null;
            }
        }

        /// <summary>
        /// An easing function as an interpolator, sampled into a table once so
        /// each frame only interpolates between samples.
        /// </summary>
        private sealed class EasingInterpolator : Java.Lang.Object, ITimeInterpolator
        {
            private const int Samples = 64;
            private readonly float[] _table = new float[Samples + 1];

            public EasingInterpolator(
                EasingFunction easing)
            {
                for (int i = 0; i <= Samples; i += 1)
                {
                    this._table[i] = easing.Ease((float)i / Samples);
                }
            }

            public float GetInterpolation(
                float input)
            {
                float position = Math.Max(0, Math.Min(1, input)) * Samples;
                int index = Math.Min(Samples - 1, (int)position);
                float fraction = position - index;
                return this._table[index] + ((this._table[index + 1] - this._table[index]) * fraction);
            }
        }
    }

    /// <summary>
    /// A view that wraps another, filling it, so an outer animator's render
    /// properties apply on top of the inner one's.
    /// </summary>
    internal sealed class AnimationLayerView : ViewGroup
    {
        public AnimationLayerView(
            AndroidContext context)
            : base(context)
        {
            // A scaled or moved child may draw outside the wrapper
            this.SetClipChildren(false);
            this.SetClipToPadding(false);
        }

        protected override void OnMeasure(
            int widthMeasureSpec,
            int heightMeasureSpec)
        {
            var child = (0 < this.ChildCount) ? this.GetChildAt(0) : null;
            if (null != child)
            {
                child.Measure(widthMeasureSpec, heightMeasureSpec);
                this.SetMeasuredDimension(child.MeasuredWidth, child.MeasuredHeight);
            }
            else
            {
                this.SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
            }
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            var child = (0 < this.ChildCount) ? this.GetChildAt(0) : null;
            if (null != child)
            {
                int width = right - left;
                int height = bottom - top;
                child.Measure(
                    MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
                child.Layout(0, 0, width, height);
            }
        }
    }
}
