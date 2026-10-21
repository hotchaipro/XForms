using System;
using System.Globalization;
using System.Runtime.Versioning;
using XForms.Web.Renderers;

namespace XForms.Web
{
    /// <summary>
    /// A view's animation layers, composed onto its element: one CSS transform
    /// (translation, rotation and scale around the innermost layer's origin),
    /// opacity and clip. Animations run on animation frames.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class AnimationLayerStack
    {
        private readonly ViewRenderer _renderer;
        private readonly AnimationLayer[] _layers;

        public AnimationLayerStack(
            ViewRenderer renderer,
            int count)
        {
            this._renderer = renderer;
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

        private static string Number(
            float value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        public void Update()
        {
            float x = 0, y = 0, scaleX = 1, scaleY = 1, rotation = 0, opacity = 1;
            float clipLeft = float.NegativeInfinity, clipTop = float.NegativeInfinity;
            float clipRight = float.PositiveInfinity, clipBottom = float.PositiveInfinity;
            bool isClipping = false;
            var origin = new Point(0.5f, 0.5f);

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

            int id = this._renderer.Id;
            Js.SetStyle(id, "transform", $"translate({Number(x)}px,{Number(y)}px) rotate({Number(rotation)}deg) scale({Number(scaleX)},{Number(scaleY)})");
            Js.SetStyle(id, "transform-origin", $"{Number(origin.X * 100)}% {Number(origin.Y * 100)}%");
            Js.SetStyle(id, "opacity", (opacity >= 1) ? null : Number(Math.Max(0, opacity)));

            if (isClipping)
            {
                var size = this._renderer.Frame;
                float left = Math.Max(0, clipLeft);
                float top = Math.Max(0, clipTop);
                float right = Math.Max(0, size.Width - Math.Min(size.Width, clipRight));
                float bottom = Math.Max(0, size.Height - Math.Min(size.Height, clipBottom));
                Js.SetStyle(id, "clip-path", $"inset({Number(top)}px {Number(right)}px {Number(bottom)}px {Number(left)}px)");
            }
            else
            {
                Js.SetStyle(id, "clip-path", null);
            }
        }

        private sealed class AnimationLayer : IAnimationLayer
        {
            private const int ChannelCount = 10;
            private readonly AnimationLayerStack _stack;
            private readonly Animation[] _animations = new Animation[ChannelCount];

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
                this._animations[(int)channel] = null;
                this.Values[(int)channel] = value;
                this._stack.Update();
            }

            public void Animate(
                AnimationChannel channel,
                float from,
                float to,
                AnimationTiming timing)
            {
                var animation = new Animation(this, channel, from, to, timing);
                this._animations[(int)channel] = animation;
                this.Values[(int)channel] = from;
                this._stack.Update();
                animation.Start();
            }

            // One channel's animation, stepped on animation frames
            private sealed class Animation
            {
                private readonly AnimationLayer _layer;
                private readonly AnimationChannel _channel;
                private readonly float _from;
                private readonly float _to;
                private readonly AnimationTiming _timing;
                private TimeSpan? _start;

                public Animation(
                    AnimationLayer layer,
                    AnimationChannel channel,
                    float from,
                    float to,
                    AnimationTiming timing)
                {
                    this._layer = layer;
                    this._channel = channel;
                    this._from = from;
                    this._to = to;
                    this._timing = timing;
                }

                public void Start()
                {
                    WebHost.Current.RequestFrame(this.OnFrame);
                }

                private void OnFrame(
                    TimeSpan time)
                {
                    // Replaced or set since
                    if (this._layer._animations[(int)this._channel] != this)
                    {
                        return;
                    }

                    this._start ??= time;
                    double elapsed = (time - this._start.Value - this._timing.Delay).TotalMilliseconds;
                    double duration = Math.Max(1, this._timing.Duration.TotalMilliseconds);
                    bool isDone = false;
                    double progress;

                    if (elapsed < 0)
                    {
                        progress = 0;
                    }
                    else if (this._timing.Loop || this._timing.AutoReverse)
                    {
                        double cycles = elapsed / duration;
                        if (!this._timing.Loop && (cycles >= 2))
                        {
                            cycles = 2;
                            isDone = true;
                        }

                        progress = cycles % 1;
                        if (this._timing.AutoReverse && (((int)cycles % 2) == 1))
                        {
                            progress = 1 - progress;
                        }

                        if (isDone)
                        {
                            progress = 0;
                        }
                    }
                    else
                    {
                        progress = Math.Min(1, elapsed / duration);
                        isDone = progress >= 1;
                    }

                    float eased = (this._timing.Easing ?? EasingFunction.Linear).Ease((float)progress);
                    this._layer.Values[(int)this._channel] = this._from + ((this._to - this._from) * eased);
                    this._layer._stack.Update();

                    if (isDone)
                    {
                        this._layer._animations[(int)this._channel] = null;
                    }
                    else
                    {
                        WebHost.Current.RequestFrame(this.OnFrame);
                    }
                }
            }
        }
    }
}
