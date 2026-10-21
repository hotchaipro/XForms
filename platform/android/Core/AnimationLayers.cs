using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Views;
using AndroidRect = Android.Graphics.Rect;
using AndroidView = Android.Views.View;

namespace XForms.Android
{
    /// <summary>
    /// A view's animation layers, composed onto the view's own render
    /// properties: translation, scale, rotation, alpha, and an outline clip.
    /// </summary>
    /// <remarks>
    /// Layers aren't wrapper views: a parent clips each child to the child's
    /// own bounds, so a wrapper would cut off an inner layer that moves out of
    /// it. Android's property animators run on the UI thread either way.
    /// </remarks>
    internal sealed class AnimationLayerStack
    {
        private readonly AndroidView _view;
        private readonly float _density;
        private readonly AnimationLayer[] _layers;

        // The clip is the view's outline while a layer clips; the view's own
        // outline (for ClipToBounds) is restored after
        private ClipOutlineProvider _clipOutline;
        private ViewOutlineProvider _savedOutline;
        private bool _savedClipToOutline;
        private bool _isClipping;

        public AnimationLayerStack(
            AndroidView view,
            float density,
            int count)
        {
            this._view = view;
            this._density = density;
            this._layers = new AnimationLayer[count];
            for (int i = 0; i < count; i += 1)
            {
                this._layers[i] = new AnimationLayer(this);
            }
        }

        public IAnimationLayer[] Layers
        {
            get
            {
                return this._layers;
            }
        }

        /// <summary>
        /// Applies the layers' values, outermost first: a layer's translation
        /// moves with the scale and rotation of the layers outside it, and
        /// scales, rotations, opacities and clips combine.
        /// </summary>
        public void Update()
        {
            float x = 0, y = 0, scaleX = 1, scaleY = 1, rotation = 0, opacity = 1;
            float clipLeft = float.NegativeInfinity, clipTop = float.NegativeInfinity;
            float clipRight = float.PositiveInfinity, clipBottom = float.PositiveInfinity;
            bool isClipping = false;
            Point origin = new Point(0.5f, 0.5f);

            foreach (var layer in this._layers)
            {
                var values = layer.Values;
                double radians = rotation * Math.PI / 180;
                float cos = (float)Math.Cos(radians), sin = (float)Math.Sin(radians);
                float tx = values[(int)AnimationChannel.TranslationX];
                float ty = values[(int)AnimationChannel.TranslationY];
                x += scaleX * ((cos * tx) - (sin * ty));
                y += scaleY * ((sin * tx) + (cos * ty));

                scaleX *= values[(int)AnimationChannel.ScaleX];
                scaleY *= values[(int)AnimationChannel.ScaleY];
                rotation += values[(int)AnimationChannel.Rotation];
                opacity *= values[(int)AnimationChannel.Opacity];

                if (layer.IsClipEnabledValue)
                {
                    isClipping = true;
                    clipLeft = Math.Max(clipLeft, values[(int)AnimationChannel.ClipLeft]);
                    clipTop = Math.Max(clipTop, values[(int)AnimationChannel.ClipTop]);
                    clipRight = Math.Min(clipRight, values[(int)AnimationChannel.ClipRight]);
                    clipBottom = Math.Min(clipBottom, values[(int)AnimationChannel.ClipBottom]);
                }

                origin = layer.Origin;
            }

            var view = this._view;
            view.TranslationX = x * this._density;
            view.TranslationY = y * this._density;
            view.ScaleX = scaleX;
            view.ScaleY = scaleY;
            view.Rotation = rotation;
            view.Alpha = opacity;
            view.PivotX = origin.X * view.Width;
            view.PivotY = origin.Y * view.Height;

            this.UpdateClip(isClipping, clipLeft, clipTop, clipRight, clipBottom);
        }

        private void UpdateClip(
            bool isClipping,
            float left,
            float top,
            float right,
            float bottom)
        {
            var view = this._view;
            if (isClipping != this._isClipping)
            {
                this._isClipping = isClipping;
                if (isClipping)
                {
                    this._savedOutline = view.OutlineProvider;
                    this._savedClipToOutline = view.ClipToOutline;
                    this._clipOutline ??= new ClipOutlineProvider();
                    view.OutlineProvider = this._clipOutline;
                    view.ClipToOutline = true;
                }
                else
                {
                    view.OutlineProvider = this._savedOutline;
                    view.ClipToOutline = this._savedClipToOutline;
                    this._savedOutline = null;
                }
            }

            if (isClipping)
            {
                // NOTE: View.ClipBounds didn't clip an owner-drawn layout view
                // on API 36; outline clipping does
                this._clipOutline.Rect = new AndroidRect(
                    (int)Math.Round(left * this._density),
                    (int)Math.Round(top * this._density),
                    (int)Math.Round(right * this._density),
                    (int)Math.Round(bottom * this._density));
                view.InvalidateOutline();
            }
        }

        private sealed class ClipOutlineProvider : ViewOutlineProvider
        {
            public AndroidRect Rect { get; set; } = new AndroidRect();

            public override void GetOutline(
                AndroidView view,
                global::Android.Graphics.Outline outline)
            {
                var rect = this.Rect;
                if ((rect.Width() <= 0) || (rect.Height() <= 0))
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
    }

    /// <summary>
    /// One animator's layer: its channels' values, set directly or animated by
    /// value animators, composed by its stack.
    /// </summary>
    internal sealed class AnimationLayer : IAnimationLayer
    {
        private const int ChannelCount = 10;

        private readonly AnimationLayerStack _stack;
        private readonly ValueAnimator[] _animators = new ValueAnimator[ChannelCount];

        public AnimationLayer(
            AnimationLayerStack stack)
        {
            this._stack = stack;
        }

        // The channels' values now, in device-independent pixels and degrees
        public float[] Values { get; } = { 0, 0, 1, 1, 0, 1, 0, 0, 0, 0 };

        public Point Origin { get; private set; } = new Point(0.5f, 0.5f);

        public bool IsClipEnabledValue { get; private set; }

        public Point TransformOrigin
        {
            set
            {
                this.Origin = value;
                this._stack.Update();
            }
        }

        public bool IsClipEnabled
        {
            set
            {
                this.IsClipEnabledValue = value;
                this._stack.Update();
            }
        }

        public void SetValue(
            AnimationChannel channel,
            float value)
        {
            this.Cancel(channel);
            this.Values[(int)channel] = value;
            this._stack.Update();
        }

        public void Animate(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            this.Cancel(channel);

            var animator = ValueAnimator.OfFloat(from, to);
            animator.Update += (sender, e) =>
            {
                this.Values[(int)channel] = (float)e.Animation.AnimatedValue;
                this._stack.Update();
            };

            animator.SetDuration((long)Math.Max(1, timing.Duration.TotalMilliseconds));
            animator.StartDelay = (long)timing.Delay.TotalMilliseconds;
            animator.SetInterpolator(new EasingInterpolator(timing.Easing ?? EasingFunction.Linear));
            if (timing.Loop || timing.AutoReverse)
            {
                animator.RepeatCount = timing.Loop ? ValueAnimator.Infinite : 1;
                animator.RepeatMode = timing.AutoReverse ? ValueAnimatorRepeatMode.Reverse : ValueAnimatorRepeatMode.Restart;
            }

            // Start where it starts (a delayed animation holds its first value)
            this.Values[(int)channel] = from;
            this._stack.Update();

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
}
