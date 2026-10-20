using System;
using System.Collections.Generic;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.System.Com;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// An animator's layer: a composition visual wrapping the view's own
    /// visual (or an inner layer), with a translate, scale and rotate transform,
    /// opacity and a rectangle clip, all animated on the compositor.
    /// </summary>
    internal sealed unsafe class AnimationLayer : IAnimationLayer
    {
        // Segments an easing curve is sampled into
        private const int Segments = 24;

        private readonly Win32Host _host;
        private readonly float[] _values = { 0, 0, 1, 1, 0, 1, 0, 0, 0, 0 };
        private IDCompositionVisual3* _visual;
        private IDCompositionTranslateTransform* _translate;
        private IDCompositionScaleTransform* _scale;
        private IDCompositionRotateTransform* _rotate;
        private IDCompositionRectangleClip* _clip;
        private Point _transformOrigin = new Point(0.5f, 0.5f);
        private Size _size;
        private bool _isClipEnabled;

        public AnimationLayer(
            Win32Host host)
        {
            this._host = host;

            var device = host.CompositionDevice;
            IDCompositionVisual2* visual;
            device->CreateVisual(&visual);
            ((IUnknown*)visual)->QueryInterface(IDCompositionVisual3.IID_Guid, out var visual3).ThrowOnFailure();
            visual->Release();
            this._visual = (IDCompositionVisual3*)visual3;

            IDCompositionScaleTransform* scale;
            device->CreateScaleTransform(&scale);
            this._scale = scale;

            IDCompositionRotateTransform* rotate;
            device->CreateRotateTransform(&rotate);
            this._rotate = rotate;

            IDCompositionTranslateTransform* translate;
            device->CreateTranslateTransform(&translate);
            this._translate = translate;

            // Scale, then rotate, around the origin; then translate
            var transforms = stackalloc IDCompositionTransform*[3];
            transforms[0] = (IDCompositionTransform*)scale;
            transforms[1] = (IDCompositionTransform*)rotate;
            transforms[2] = (IDCompositionTransform*)translate;

            IDCompositionTransform* group;
            device->CreateTransformGroup(transforms, 3, &group);
            this._visual->SetTransform(group);
            group->Release();

            // Start from the identity, whatever the compositor's defaults
            for (int i = (int)AnimationChannel.TranslationX; i <= (int)AnimationChannel.Opacity; i += 1)
            {
                this.SetValue((AnimationChannel)i, this._values[i]);
            }
        }

        public IDCompositionVisual2* Visual
        {
            get
            {
                return (IDCompositionVisual2*)this._visual;
            }
        }

        /// <summary>
        /// The static translation, in DIPs, for hit testing.
        /// </summary>
        public Point Translation
        {
            get
            {
                return new Point(this._values[(int)AnimationChannel.TranslationX], this._values[(int)AnimationChannel.TranslationY]);
            }
        }

        public Point TransformOrigin
        {
            set
            {
                this._transformOrigin = value;
                this.UpdateOrigin();
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
                if (value)
                {
                    IDCompositionRectangleClip* clip;
                    this._host.CompositionDevice->CreateRectangleClip(&clip);
                    this._clip = clip;
                    for (int i = (int)AnimationChannel.ClipLeft; i <= (int)AnimationChannel.ClipBottom; i += 1)
                    {
                        this.SetValue((AnimationChannel)i, this._values[i]);
                    }

                    this._visual->SetClip((IDCompositionClip*)clip);
                }
                else
                {
                    this._visual->SetClip((IDCompositionClip*)null);
                    this._clip->Release();
                    this._clip = null;
                }

                this._host.RequestCommit();
            }
        }

        /// <summary>
        /// The view's size, in DIPs, for the transform origin.
        /// </summary>
        public void SetSize(
            Size size)
        {
            this._size = size;
            this.UpdateOrigin();
        }

        private void UpdateOrigin()
        {
            float scale = this._host.Scale;
            float x = this._transformOrigin.X * this._size.Width * scale;
            float y = this._transformOrigin.Y * this._size.Height * scale;
            this._scale->SetCenterX(x);
            this._scale->SetCenterY(y);
            this._rotate->SetCenterX(x);
            this._rotate->SetCenterY(y);
            this._host.RequestCommit();
        }

        public void SetValue(
            AnimationChannel channel,
            float value)
        {
            this._values[(int)channel] = value;
            float device = value * this.DeviceScale(channel);

            switch (channel)
            {
                case AnimationChannel.TranslationX: this._translate->SetOffsetX(device); break;
                case AnimationChannel.TranslationY: this._translate->SetOffsetY(device); break;
                case AnimationChannel.ScaleX: this._scale->SetScaleX(device); break;
                case AnimationChannel.ScaleY: this._scale->SetScaleY(device); break;
                case AnimationChannel.Rotation: this._rotate->SetAngle(device); break;
                case AnimationChannel.Opacity: this._visual->SetOpacity(device); break;
                case AnimationChannel.ClipLeft: if (null != this._clip) { this._clip->SetLeft(device); } break;
                case AnimationChannel.ClipTop: if (null != this._clip) { this._clip->SetTop(device); } break;
                case AnimationChannel.ClipRight: if (null != this._clip) { this._clip->SetRight(device); } break;
                case AnimationChannel.ClipBottom: if (null != this._clip) { this._clip->SetBottom(device); } break;
            }

            this._host.RequestCommit();
        }

        public void Animate(
            AnimationChannel channel,
            float from,
            float to,
            AnimationTiming timing)
        {
            this._values[(int)channel] = timing.AutoReverse ? from : to;

            float scale = this.DeviceScale(channel);
            var animation = CreateAnimation(this._host, from * scale, to * scale, timing);

            switch (channel)
            {
                case AnimationChannel.TranslationX: this._translate->SetOffsetX(animation); break;
                case AnimationChannel.TranslationY: this._translate->SetOffsetY(animation); break;
                case AnimationChannel.ScaleX: this._scale->SetScaleX(animation); break;
                case AnimationChannel.ScaleY: this._scale->SetScaleY(animation); break;
                case AnimationChannel.Rotation: this._rotate->SetAngle(animation); break;
                case AnimationChannel.Opacity: this._visual->SetOpacity(animation); break;
                case AnimationChannel.ClipLeft: if (null != this._clip) { this._clip->SetLeft(animation); } break;
                case AnimationChannel.ClipTop: if (null != this._clip) { this._clip->SetTop(animation); } break;
                case AnimationChannel.ClipRight: if (null != this._clip) { this._clip->SetRight(animation); } break;
                case AnimationChannel.ClipBottom: if (null != this._clip) { this._clip->SetBottom(animation); } break;
            }

            animation->Release();
            this._host.RequestCommit();
        }

        // Translation and clip are in DIPs; the compositor works in pixels
        private float DeviceScale(
            AnimationChannel channel)
        {
            switch (channel)
            {
                case AnimationChannel.TranslationX:
                case AnimationChannel.TranslationY:
                case AnimationChannel.ClipLeft:
                case AnimationChannel.ClipTop:
                case AnimationChannel.ClipRight:
                case AnimationChannel.ClipBottom:
                    return this._host.Scale;

                default:
                    return 1;
            }
        }

        /// <summary>
        /// A compositor animation function: the easing curve as cubic Hermite
        /// segments, after the delay, reversed if it auto-reverses, and
        /// repeated if it loops.
        /// </summary>
        internal static IDCompositionAnimation* CreateAnimation(
            Win32Host host,
            float from,
            float to,
            AnimationTiming timing)
        {
            IDCompositionAnimation* animation;
            host.CompositionDevice->CreateAnimation(&animation);

            double delay = timing.Delay.TotalSeconds;
            double duration = Math.Max(0.001, timing.Duration.TotalSeconds);
            var easing = timing.Easing ?? EasingFunction.Linear;

            if (delay > 0)
            {
                animation->AddCubic(0, from, 0, 0, 0);
            }

            AddEased(animation, delay, duration, from, to, easing, reverse: false);
            double cycle = duration;
            if (timing.AutoReverse)
            {
                AddEased(animation, delay + duration, duration, from, to, easing, reverse: true);
                cycle = duration * 2;
            }

            if (timing.Loop)
            {
                animation->AddRepeat(delay + cycle, cycle);
            }
            else
            {
                animation->End(delay + cycle, timing.AutoReverse ? from : to);
            }

            return animation;
        }

        private static void AddEased(
            IDCompositionAnimation* animation,
            double begin,
            double duration,
            float from,
            float to,
            EasingFunction easing,
            bool reverse)
        {
            float delta = to - from;
            double h = duration / Segments;

            // The value and its rate of change (per second) at a sample
            float Value(int i)
            {
                float u = (float)i / Segments;
                return from + (delta * easing.Ease(reverse ? 1 - u : u));
            }

            float Slope(int i)
            {
                const float Step = 0.001f;
                float u = (float)i / Segments;
                if (reverse)
                {
                    u = 1 - u;
                }

                float before = easing.Ease(Math.Max(0, u - Step));
                float after = easing.Ease(Math.Min(1, u + Step));
                float width = Math.Min(1, u + Step) - Math.Max(0, u - Step);
                float perUnit = delta * (after - before) / width;
                return (float)((reverse ? -perUnit : perUnit) / duration);
            }

            for (int i = 0; i < Segments; i += 1)
            {
                float p0 = Value(i), p1 = Value(i + 1);
                float m0 = Slope(i), m1 = Slope(i + 1);
                float c = (float)(((3 * (p1 - p0) / h) - (2 * m0) - m1) / h);
                float d = (float)(((2 * (p0 - p1) / h) + m0 + m1) / (h * h));
                animation->AddCubic(begin + (i * h), p0, m0, c, d);
            }
        }

        public void Release()
        {
            if (null != this._clip) { this._clip->Release(); }
            this._clip = null;
            this._translate->Release();
            this._scale->Release();
            this._rotate->Release();
            this._visual->Release();
        }
    }
}
