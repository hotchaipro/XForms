using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// An animator's layer: a view's Core Animation layer, with its transform
    /// (translation, scale, rotation), opacity and a mask for the clip. Core
    /// Animation plays the animations in the render server.
    /// </summary>
    internal sealed class AnimationLayer : IAnimationLayer
    {
        // Keyframes an easing curve is sampled into
        private const int Samples = 30;

        private readonly UIView _view;
        private readonly float[] _clip = new float[4];
        private CALayer _mask;

        public AnimationLayer(
            UIView view)
        {
            this._view = view;
        }

        public UIView View
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
                // The view's renderer keeps its position when the anchor moves
                this._view.Layer.AnchorPoint = new CGPoint(value.X, value.Y);
            }
        }

        public bool IsClipEnabled
        {
            set
            {
                if (value && (null == this._mask))
                {
                    this._mask = new CALayer()
                    {
                        AnchorPoint = CGPoint.Empty,
                        BackgroundColor = UIColor.Black.CGColor,
                    };
                    this.UpdateMask();
                    this._view.Layer.Mask = this._mask;
                }
                else if (!value && (null != this._mask))
                {
                    this._view.Layer.Mask = null;
                    this._mask.Dispose();
                    this._mask = null;
                }
            }
        }

        private void UpdateMask()
        {
            if (null == this._mask)
            {
                return;
            }

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            this._mask.RemoveAllAnimations();
            this._mask.Position = new CGPoint(this._clip[0], this._clip[1]);
            this._mask.Bounds = new CGRect(0, 0, Math.Max(0, this._clip[2] - this._clip[0]), Math.Max(0, this._clip[3] - this._clip[1]));
            CATransaction.Commit();
        }

        public void SetValue(
            AnimationChannel channel,
            float value)
        {
            if (channel >= AnimationChannel.ClipLeft)
            {
                this._clip[channel - AnimationChannel.ClipLeft] = value;
                this.UpdateMask();
                return;
            }

            string keyPath = KeyPath(channel);
            var layer = this._view.Layer;

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            layer.RemoveAnimation(keyPath);
            layer.SetValueForKeyPath(NSNumber.FromDouble(NativeValue(channel, value)), new NSString(keyPath));
            CATransaction.Commit();
        }

        public void Animate(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            if (channel >= AnimationChannel.ClipLeft)
            {
                this.AnimateClip(channel, from, to, timing);
                return;
            }

            string keyPath = KeyPath(channel);
            var layer = this._view.Layer;

            var animation = CreateAnimation(keyPath, timing, (u) => NativeValue(channel, from + ((to - from) * u)));

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            layer.SetValueForKeyPath(NSNumber.FromDouble(NativeValue(channel, timing.AutoReverse ? from : to)), new NSString(keyPath));
            layer.AddAnimation(animation, keyPath);
            CATransaction.Commit();
        }

        // The mask's edges, as its position and size
        private void AnimateClip(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            if (null == this._mask)
            {
                return;
            }

            int edge = channel - AnimationChannel.ClipLeft;
            var start = (float[])this._clip.Clone();
            start[edge] = from;
            this._clip[edge] = timing.AutoReverse ? from : to;
            var end = (float[])start.Clone();
            end[edge] = to;

            float Edge(int index, double u) => (float)(start[index] + ((end[index] - start[index]) * u));

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            if ((edge == 0) || (edge == 1))
            {
                string keyPath = (edge == 0) ? "position.x" : "position.y";
                this._mask.AddAnimation(CreateAnimation(keyPath, timing, (u) => Edge(edge, u)), keyPath);
            }

            string sizePath = ((edge == 0) || (edge == 2)) ? "bounds.size.width" : "bounds.size.height";
            int low = ((edge == 0) || (edge == 2)) ? 0 : 1;
            this._mask.AddAnimation(CreateAnimation(sizePath, timing, (u) => Math.Max(0, Edge(low + 2, u) - Edge(low, u))), sizePath);

            this._mask.Position = new CGPoint(this._clip[0], this._clip[1]);
            this._mask.Bounds = new CGRect(0, 0, Math.Max(0, this._clip[2] - this._clip[0]), Math.Max(0, this._clip[3] - this._clip[1]));
            CATransaction.Commit();
        }

        /// <summary>
        /// A keyframe animation of the easing curve, after the delay, reversed
        /// if it auto-reverses, and repeated if it loops.
        /// </summary>
        private static CAKeyFrameAnimation CreateAnimation(
            string keyPath,
            AnimationTiming timing,
            Func<double, double> valueAt)
        {
            var easing = timing.Easing ?? EasingFunction.Linear;
            var values = new NSObject[Samples + 1];
            var times = new NSNumber[Samples + 1];
            for (int i = 0; i <= Samples; i += 1)
            {
                float u = (float)i / Samples;
                values[i] = NSNumber.FromDouble(valueAt(easing.Ease(u)));
                times[i] = NSNumber.FromDouble(u);
            }

            var animation = CAKeyFrameAnimation.FromKeyPath(keyPath);
            animation.Values = values;
            animation.KeyTimes = times;
            animation.Duration = Math.Max(0.001, timing.Duration.TotalSeconds);
            animation.AutoReverses = timing.AutoReverse;
            animation.RepeatCount = timing.Loop ? float.PositiveInfinity : 0;
            animation.FillMode = CAFillMode.Backwards;
            if (timing.Delay > TimeSpan.Zero)
            {
                animation.BeginTime = CAAnimation.CurrentMediaTime() + timing.Delay.TotalSeconds;
            }

            return animation;
        }

        private static string KeyPath(
            AnimationChannel channel)
        {
            switch (channel)
            {
                case AnimationChannel.TranslationX: return "transform.translation.x";
                case AnimationChannel.TranslationY: return "transform.translation.y";
                case AnimationChannel.ScaleX: return "transform.scale.x";
                case AnimationChannel.ScaleY: return "transform.scale.y";
                case AnimationChannel.Rotation: return "transform.rotation.z";
                default: return "opacity";
            }
        }

        private static double NativeValue(
            AnimationChannel channel,
            double value)
        {
            return (channel == AnimationChannel.Rotation) ? value * Math.PI / 180 : value;
        }
    }
}
