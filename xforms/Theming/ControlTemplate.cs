using System;

namespace XForms.Controls
{
    /// <summary>
    /// How a kind of control looks: its drawing, the child views (parts) it
    /// adds, its layout of them, and its response to state changes. A theme
    /// holds one for each control type (see
    /// <see cref="ThemeTemplates"/>); a control uses its theme's.
    /// </summary>
    /// <remarks>
    /// As WPF's and Avalonia's <c>ControlTemplate</c>, but in code: small
    /// enough (draw, add parts, react to state) for markup to express later.
    /// </remarks>
    public abstract class ControlTemplate
    {
        private protected ControlTemplate()
        {
        }

        /// <summary>
        /// The type of control the template is for.
        /// </summary>
        public abstract Type ControlType { get; }

        internal abstract void ApplyCore(View control, TemplateContext context);

        internal abstract void UnapplyCore(View control, TemplateContext context);

        internal abstract void RenderCore(View control, DrawingContext drawingContext);

        internal abstract Size? MeasureCore(View control, Size availableSize);

        internal abstract bool ArrangeCore(View control, Size finalSize);

        internal abstract void OnStateChangedCore(View control, TemplateContext context);

        internal abstract Color? BackgroundCore(View control);

        internal abstract void OnThemeChangedCore(View control, TemplateContext context);
    }

    /// <summary>
    /// How a <typeparamref name="TControl"/> looks.
    /// </summary>
    public abstract class ControlTemplate<TControl> : ControlTemplate
        where TControl : View
    {
        public sealed override Type ControlType
        {
            get
            {
                return typeof(TControl);
            }
        }

        /// <summary>
        /// Sets the control up for this look: adds its parts and creates its
        /// animators, through the context. Called as the control is first
        /// attached (before its native object exists, so the template can
        /// create the control's own animators), and when its theme brings a
        /// different template. Keep per-control state on the control's parts
        /// or in <see cref="TemplateContext.State"/>: one template serves every
        /// control of its type.
        /// </summary>
        protected virtual void Apply(
            TControl control,
            TemplateContext context)
        {
        }

        /// <summary>
        /// Undoes <see cref="Apply"/>, when the control's theme brings a
        /// different template: removes the parts it added.
        /// </summary>
        protected virtual void Unapply(
            TControl control,
            TemplateContext context)
        {
        }

        /// <summary>
        /// Draws the control (beneath its parts and content), for a control
        /// that draws itself (such as a <see cref="TemplatedControl"/>).
        /// </summary>
        protected virtual void Render(
            TControl control,
            DrawingContext drawingContext)
        {
        }

        /// <summary>
        /// Called when the control's theme changes but keeps this template
        /// (such as a new mode or accent color), to restyle its parts.
        /// </summary>
        protected virtual void OnThemeChanged(
            TControl control,
            TemplateContext context)
        {
        }

        /// <summary>
        /// The control's background, or null for none (transparent).
        /// </summary>
        protected virtual Color? Background(
            TControl control)
        {
            return null;
        }

        /// <summary>
        /// The size the control needs, within its padding; or null for the
        /// default (its content's and parts' size).
        /// </summary>
        protected virtual Size? Measure(
            TControl control,
            Size availableSize)
        {
            return null;
        }

        /// <summary>
        /// Arranges the control's parts and content within its padding, and
        /// returns true; or returns false for the default (each in the whole
        /// area).
        /// </summary>
        protected virtual bool Arrange(
            TControl control,
            Size finalSize)
        {
            return false;
        }

        /// <summary>
        /// Called when the control's state changes (pressed, pointer over,
        /// focused, enabled, or its own, such as checked), to give feedback.
        /// The control redraws itself afterward.
        /// </summary>
        protected virtual void OnStateChanged(
            TControl control,
            TemplateContext context)
        {
        }

        internal sealed override void ApplyCore(View control, TemplateContext context) => this.Apply((TControl)control, context);

        internal sealed override void UnapplyCore(View control, TemplateContext context) => this.Unapply((TControl)control, context);

        internal sealed override void RenderCore(View control, DrawingContext drawingContext) => this.Render((TControl)control, drawingContext);

        internal sealed override Size? MeasureCore(View control, Size availableSize) => this.Measure((TControl)control, availableSize);

        internal sealed override bool ArrangeCore(View control, Size finalSize) => this.Arrange((TControl)control, finalSize);

        internal sealed override void OnStateChangedCore(View control, TemplateContext context) => this.OnStateChanged((TControl)control, context);

        internal sealed override Color? BackgroundCore(View control) => this.Background((TControl)control);

        internal sealed override void OnThemeChangedCore(View control, TemplateContext context) => this.OnThemeChanged((TControl)control, context);
    }

    /// <summary>
    /// What a template can do to its control that only the control itself
    /// could: add and remove parts, create animators, and invalidate.
    /// </summary>
    public sealed class TemplateContext
    {
        private readonly ITemplatedView _control;

        internal TemplateContext(
            ITemplatedView control)
        {
            this._control = control;
        }

        /// <summary>
        /// Anything the template keeps for this control (a part, an animator),
        /// since one template serves every control of its type. Cleared when
        /// the control's template changes.
        /// </summary>
        public object State
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a child view to the control, beneath its content.
        /// </summary>
        public void AddPart(
            View part)
        {
            this._control.TemplateHost.AddPart(part);
        }

        /// <summary>
        /// Removes a part the template added.
        /// </summary>
        public void RemovePart(
            View part)
        {
            this._control.TemplateHost.RemovePart(part);
        }

        /// <summary>
        /// Creates a scale and opacity applied to the control's drawing (see
        /// <see cref="DrawnAnimator"/>), for feedback such as a press.
        /// </summary>
        public DrawnAnimator CreateDrawnAnimator()
        {
            return this._control.CreateTemplateDrawnAnimator();
        }

        /// <summary>
        /// Creates one of the control's own animators (a native layer). Only
        /// in <c>Apply</c> as the control is first attached: it throws once
        /// the control's native object exists.
        /// </summary>
        public Animator CreateOwnAnimator()
        {
            return this._control.CreateTemplateOwnAnimator();
        }

        /// <summary>
        /// Redraws the control.
        /// </summary>
        public void InvalidateVisual()
        {
            ((View)this._control).InvalidateVisual();
        }

        /// <summary>
        /// Measures and arranges the control again.
        /// </summary>
        public void InvalidateMeasure()
        {
            ((View)this._control).InvalidateMeasure();
        }
    }
}
