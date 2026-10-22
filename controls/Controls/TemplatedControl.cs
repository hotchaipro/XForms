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
    public abstract class TemplatedControl : UserControl
    {
        private ControlTemplate _template;
        private int _partCount;
        private bool _isPressed;
        private bool _isEnabled = true;

        protected TemplatedControl()
        {
            this.TemplateContext = new TemplateContext(this);
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
                return this._template;
            }
        }

        internal TemplateContext TemplateContext
        {
            get;
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
            this._template?.OnStateChangedCore(this);
            this.InvalidateVisual();
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
            this.ApplyTemplate(theme);
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.ApplyTemplate(theme);
        }

        private void ApplyTemplate(
            Theme theme)
        {
            var template = theme.GetTemplate(this.GetType())
                ?? throw new InvalidOperationException(
                    $"{this.GetType().Name} has no template in its theme. Choose a theme with templates for XForms.Controls in Application.CreateTheme, such as XForms.Themes.Simple's SimpleTheme.Create(mode, accent).");

            if (template == this._template)
            {
                return;
            }

            if (null != this._template)
            {
                this._template.UnapplyCore(this, this.TemplateContext);
                this.TemplateContext.State = null;
            }

            this._template = template;
            template.ApplyCore(this, this.TemplateContext);
            template.OnStateChangedCore(this);
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        protected override void Render(
            DrawingContext drawingContext)
        {
            this._template?.RenderCore(this, drawingContext);
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            return this._template?.MeasureCore(this, availableSize) ?? base.MeasureOverride(availableSize);
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            if (!(this._template?.ArrangeCore(this, finalSize) ?? false))
            {
                base.ArrangeOverride(finalSize);
            }
        }

        // The template's parts go beneath the content
        internal void AddPart(
            View part)
        {
            this.InsertVisualChild(this._partCount, part);
            this._partCount += 1;
        }

        internal void RemovePart(
            View part)
        {
            if (this.RemoveVisualChild(part))
            {
                this._partCount -= 1;
            }
        }

        internal DrawnAnimator CreateTemplateDrawnAnimator()
        {
            return this.CreateDrawnAnimator();
        }

        internal Animator CreateTemplateOwnAnimator()
        {
            return this.CreateOwnAnimator();
        }
    }
}
