using System;

namespace XForms.Controls
{
    /// <summary>
    /// Base for controls whose look comes from their theme: the control says
    /// what it does and what state it's in, and its theme's
    /// <see cref="ControlTemplate"/> draws it, adds its parts and gives
    /// feedback.
    /// </summary>
    /// <remarks>
    /// The template is the theme's (<see cref="ThemeTemplates"/>), chosen by
    /// the app in <c>Application.CreateTheme</c>. A control whose theme has no
    /// template for it throws when it's attached.
    /// </remarks>
    public abstract class TemplatedControl : UserControl, ITemplatedView
    {
        private readonly TemplateHost _templateHost;
        private bool _isPressed;
        private bool _isEnabled = true;

        protected TemplatedControl()
        {
            this._templateHost = new TemplateHost(this);
        }

        /// <summary>
        /// Raised when the control's state changes: pressed, pointer over,
        /// focused, enabled, or its own (such as checked).
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// The template drawing the control, once it's attached.
        /// </summary>
        public ControlTemplate Template
        {
            get
            {
                return this._templateHost.Template;
            }
        }

        TemplateHost ITemplatedView.TemplateHost
        {
            get
            {
                return this._templateHost;
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
        /// Whether the control responds to the user.
        /// </summary>
        public virtual bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                if (value != this._isEnabled)
                {
                    this._isEnabled = value;
                    this.OnStateChanged();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tells the template, and listeners, that the control's state changed,
        /// and redraws it. Subclasses call it for their own state (such as
        /// checked or selected).
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

        // The theme is known, and the native object doesn't exist yet: the
        // template can create the control's own animators
        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._templateHost.Apply(theme);
        }

        // After a hot reload edit: the template is applied again
        protected override void OnHotReload()
        {
            base.OnHotReload();
            this._templateHost.Reapply(this.ActualTheme);
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this._templateHost.Apply(theme);
        }

        // The background (transparent unless the control gives it one), then
        // the template's drawing
        protected override void Render(
            DrawingContext drawingContext)
        {
            base.Render(drawingContext);
            this._templateHost.Render(drawingContext);
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
