using System;

namespace XForms.Controls
{
    /// <summary>
    /// Base for content controls whose look comes from their theme, as
    /// <see cref="TemplatedControl"/> for owner-drawn ones: the theme's
    /// <see cref="ControlTemplate"/> gives the control its background and
    /// feedback, and can add parts beneath its content.
    /// </summary>
    public abstract class TemplatedContentControl : ContentControl, ITemplatedView
    {
        private readonly TemplateHost _templateHost;
        private bool _isPressed;

        protected TemplatedContentControl()
        {
            this._templateHost = new TemplateHost(this);
        }

        /// <summary>
        /// Raised when the control's state changes: pressed, pointer over,
        /// focused, or its own.
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// The template giving the control its look, once it's attached.
        /// </summary>
        public ControlTemplate Template
        {
            get
            {
                return this._templateHost.Template;
            }
        }

        /// <summary>
        /// Whether the control is being pressed.
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return this._isPressed;
            }

            protected set
            {
                if (value != this._isPressed)
                {
                    this._isPressed = value;
                    this.OnStateChanged();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The background: the template's.
        /// </summary>
        public override Color BackgroundColor
        {
            get
            {
                return this._templateHost.Background ?? Colors.Transparent;
            }
        }

        /// <summary>
        /// Tells the template, and listeners, that the control's state changed.
        /// </summary>
        protected void OnStateChanged()
        {
            this._templateHost.OnStateChanged();
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPointerEntered(
            PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            this.OnStateChanged();
        }

        protected override void OnPointerExited(
            PointerEventArgs e)
        {
            base.OnPointerExited(e);
            this.OnStateChanged();
        }

        protected override void OnGotFocus()
        {
            base.OnGotFocus();
            this.OnStateChanged();
        }

        protected override void OnLostFocus()
        {
            base.OnLostFocus();
            this.OnStateChanged();
        }

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._templateHost.Apply(theme);
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this._templateHost.Apply(theme);
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            return this._templateHost.Measure(availableSize) ?? base.MeasureOverride(availableSize);
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            if (!this._templateHost.Arrange(finalSize))
            {
                base.ArrangeOverride(finalSize);
            }
        }

        TemplateHost ITemplatedView.TemplateHost
        {
            get
            {
                return this._templateHost;
            }
        }

        void ITemplatedView.AddTemplatePart(
            View part,
            int index)
        {
            this.InsertVisualChild(index, part);
        }

        bool ITemplatedView.RemoveTemplatePart(
            View part)
        {
            return this.RemoveVisualChild(part);
        }

        DrawnAnimator ITemplatedView.CreateTemplateDrawnAnimator()
        {
            return this.CreateDrawnAnimator();
        }

        Animator ITemplatedView.CreateTemplateOwnAnimator()
        {
            return this.CreateOwnAnimator();
        }
    }
}
