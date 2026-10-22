using System;

namespace XForms.Controls
{
    /// <summary>
    /// What a view that takes its look from a template does for it: the
    /// protected parts of the view that its template's context reaches.
    /// </summary>
    internal interface ITemplatedView
    {
        TemplateHost TemplateHost { get; }

        void AddTemplatePart(View part, int index);

        bool RemoveTemplatePart(View part);

        DrawnAnimator CreateTemplateDrawnAnimator();

        Animator CreateTemplateOwnAnimator();
    }

    /// <summary>
    /// Resolves a view's template from its theme, applies it, and passes it
    /// the view's drawing, layout and state changes: for
    /// <see cref="TemplatedControl"/>, and for views that can't derive from it
    /// (such as <see cref="ListViewItem"/>, an <c>ItemContainer</c>).
    /// </summary>
    internal sealed class TemplateHost
    {
        private readonly View _view;
        private ControlTemplate _template;
        private int _partCount;

        public TemplateHost(
            View view)
        {
            this._view = view;
            this.Context = new TemplateContext((ITemplatedView)view);
        }

        public TemplateContext Context
        {
            get;
        }

        public ControlTemplate Template
        {
            get
            {
                return this._template;
            }
        }

        // From the view's OnAttaching (before its native object exists, so
        // the template can create its own animators) and OnThemeChanged
        public void Apply(
            Theme theme)
        {
            var template = theme.GetTemplate(this._view.GetType())
                ?? throw new InvalidOperationException(
                    $"{this._view.GetType().Name} has no template in its theme. Choose a theme with templates for XForms.Controls in Application.CreateTheme, such as XForms.Themes.Simple's SimpleTheme.Apply(base.CreateTheme(mode, accentColor)).");

            if (template == this._template)
            {
                template.OnThemeChangedCore(this._view, this.Context);
                return;
            }

            if (null != this._template)
            {
                this._template.UnapplyCore(this._view, this.Context);
                this.Context.State = null;
            }

            this._template = template;
            template.ApplyCore(this._view, this.Context);
            template.OnStateChangedCore(this._view, this.Context);
            this._view.InvalidateMeasure();
            this._view.InvalidateVisual();
        }

        /// <summary>
        /// The template's background for the view, or null.
        /// </summary>
        public Color? Background
        {
            get
            {
                return this._template?.BackgroundCore(this._view);
            }
        }

        public void OnStateChanged()
        {
            this._template?.OnStateChangedCore(this._view, this.Context);
            this._view.InvalidateVisual();
        }

        public void Render(
            DrawingContext drawingContext)
        {
            this._template?.RenderCore(this._view, drawingContext);
        }

        public Size? Measure(
            Size availableSize)
        {
            return this._template?.MeasureCore(this._view, availableSize);
        }

        public bool Arrange(
            Size finalSize)
        {
            return this._template?.ArrangeCore(this._view, finalSize) ?? false;
        }

        // The template's parts go beneath the view's own children
        public void AddPart(
            View part)
        {
            ((ITemplatedView)this._view).AddTemplatePart(part, this._partCount);
            this._partCount += 1;
        }

        public void RemovePart(
            View part)
        {
            if (((ITemplatedView)this._view).RemoveTemplatePart(part))
            {
                this._partCount -= 1;
            }
        }
    }
}
