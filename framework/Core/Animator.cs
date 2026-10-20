using System;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// A view's render properties: translation, scale, rotation and opacity.
    /// They don't affect layout. Set them directly, or animate them with the
    /// <c>To</c> methods; the platform's compositor plays the animations.
    /// </summary>
    /// <remarks>
    /// A view gets an animator when it's created: the app's through
    /// <see cref="View.Animator"/>, a control's own through
    /// <see cref="Controls.Control.CreateOwnAnimator"/>. An animator belongs to
    /// one view.
    /// NOTE: Until animators get their own nested layers, every animator of a
    /// view drives the view's one native object, so they share its values.
    /// </remarks>
    public sealed class Animator
    {
        private View _view;
        private float _opacity = 1.0f;
        private float _scale = 1.0f;
        private Angle _rotation;
        private Point _translation;

        public Animator()
        {
        }

        private IViewRenderer Native
        {
            get
            {
                return (IViewRenderer)this._view?.CreatedRenderer;
            }
        }

        public float Opacity
        {
            get
            {
                return this.Native?.Opacity ?? this._opacity;
            }

            set
            {
                this._opacity = value;
                this.Native?.Opacity = value;
            }
        }

        public float Scale
        {
            get
            {
                return this.Native?.Scale ?? this._scale;
            }

            set
            {
                this._scale = value;
                this.Native?.Scale = value;
            }
        }

        public Angle Rotation
        {
            get
            {
                return this.Native?.Rotation ?? this._rotation;
            }

            set
            {
                this._rotation = value;
                this.Native?.Rotation = value;
            }
        }

        public Point Translation
        {
            get
            {
                return this.Native?.Translation ?? this._translation;
            }

            set
            {
                this._translation = value;
                this.Native?.Translation = value;
            }
        }

        /// <summary>
        /// Animates the scale from its current value. Before the view is
        /// attached, the scale is set at once.
        /// </summary>
        public Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            this._scale = scale;
            return this.Native?.ScaleTo(scale, duration, ease) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Animates the rotation from its current value. Before the view is
        /// attached, the rotation is set at once.
        /// </summary>
        public Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            this._rotation = angle;
            return this.Native?.RotateTo(angle, duration, ease) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Animates the translation from its current value. Before the view is
        /// attached, the translation is set at once.
        /// </summary>
        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            this._translation = point;
            return this.Native?.TranslateTo(point, duration, ease) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Stops the animations, leaving the values where they are.
        /// </summary>
        public void Stop()
        {
            this.Native?.StopAnimation();
        }

        private TimeSpan? _rotationPeriod;

        /// <summary>
        /// Spins the view continuously, one turn per period (clockwise, around
        /// its center), until <see cref="StopContinuousRotation"/>. The platform
        /// runs the animation off the UI thread where it can.
        /// </summary>
        public void StartContinuousRotation(
            TimeSpan period)
        {
            if (period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(period));
            }

            this._rotationPeriod = period;
            this.Native?.StartContinuousRotation(period);
        }

        public void StopContinuousRotation()
        {
            this._rotationPeriod = null;
            this.Native?.StopContinuousRotation();
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
        /// Applies the values set before the view was attached.
        /// </summary>
        internal void OnRendererCreated()
        {
            var native = this.Native;

            if (this._opacity != 1.0f)
            {
                native.Opacity = this._opacity;
            }

            if (this._scale != 1.0f)
            {
                native.Scale = this._scale;
            }

            if (this._rotation != default(Angle))
            {
                native.Rotation = this._rotation;
            }

            if (this._translation != Point.Zero)
            {
                native.Translation = this._translation;
            }

            if (this._rotationPeriod.HasValue)
            {
                native.StartContinuousRotation(this._rotationPeriod.Value);
            }
        }
    }
}
