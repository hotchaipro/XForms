using System;
using System.Collections.Generic;
using XForms.Controls;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// The renderer of a lightweight view: a view with no native object of its
    /// own, which draws into the surface of its nearest native ancestor, its
    /// layer. The framework lays it out, draws it, hit-tests it and routes its
    /// input. Native views inside it are hosted by the layer.
    /// </summary>
    /// <remarks>
    /// Android and iOS use these. Win32 has its own lightweight renderers, in
    /// its renderer tree.
    /// </remarks>
    internal abstract class LightweightRenderer : IViewRenderer, IControlRenderer
    {
        private bool _isVisible = true;
        private Color _backgroundColor = Colors.Transparent;

        protected LightweightRenderer(
            View view)
        {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public View View
        {
            get;
        }

        public Element Element
        {
            get
            {
                return this.View;
            }
        }

        public object NativeElement
        {
            get
            {
                return null;
            }
        }

        public Rect Frame
        {
            get;
            private set;
        }

        public bool ClipsToBounds
        {
            get;
            private set;
        }

        // The native ancestor this view draws into, if it's attached to one
        protected IViewRenderer Layer
        {
            get
            {
                return this.View.FindLayer();
            }
        }

        public virtual void OnThemeChanged(
            Theme theme)
        {
        }

        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                this._isVisible = value;
                this.View.UpdateHostedVisibility();
                this.Invalidate();
            }
        }

        public bool IsHitTestVisible
        {
            get;
            set;
        } = true;

        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;
                this.Invalidate();
            }
        }

        public bool ClipToBounds
        {
            set
            {
                this.ClipsToBounds = value;
                this.Invalidate();
            }
        }

        public virtual Size MeasureNative(
            Size availableSize)
        {
            return this.View.MeasureContent(availableSize);
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            this.Frame = frame;
            this.View.LayoutContent(frame.Size);
            this.Invalidate();
        }

        public void InvalidateMeasure()
        {
            // The layer lays its content out again
            this.Layer?.InvalidateMeasure();
        }

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            throw new InvalidOperationException("A lightweight view has no animation layers.");
        }

        public bool Focus()
        {
            // NOTE: Keyboard focus needs a native object on Android and iOS:
            // the layer's takes it
            return this.View.LayerView?.FocusLightweight(this.View) ?? false;
        }

        public void AddHostedView(
            IViewRenderer child,
            int index)
        {
            throw new InvalidOperationException("A lightweight view hosts no native views.");
        }

        public void RemoveHostedView(
            IViewRenderer child)
        {
            throw new InvalidOperationException("A lightweight view hosts no native views.");
        }

        public void InvalidateLayerContent()
        {
            this.Invalidate();
        }

        /// <summary>
        /// Draws the view again: its layer draws its content again.
        /// </summary>
        public void Invalidate()
        {
            this.Layer?.InvalidateLayerContent();
        }

        /// <summary>
        /// Draws the view's own content at the origin, within its size.
        /// </summary>
        public virtual void Paint(
            DrawingContext context)
        {
            if (!this._backgroundColor.IsTransparent)
            {
                context.FillRectangle(new Rect(0, 0, this.Frame.Width, this.Frame.Height), this._backgroundColor);
            }
        }
    }

    /// <summary>
    /// A lightweight panel. The framework keeps its children; it only draws
    /// them, in their drawing order.
    /// </summary>
    internal sealed class LightweightPanelRenderer : LightweightRenderer, IPanelRenderer
    {
        public LightweightPanelRenderer(
            Panel panel)
            : base(panel)
        {
        }

        public void ClearChildren() => this.Invalidate();

        public void AddChild(IViewRenderer childRenderer) => this.Invalidate();

        public void InsertChild(int index, IViewRenderer childRenderer) => this.Invalidate();

        public void RemoveChildAt(int index) => this.Invalidate();

        public void ReplaceChild(int index, IViewRenderer childRenderer) => this.Invalidate();
    }

    /// <summary>
    /// A lightweight owner-drawn or content control.
    /// </summary>
    internal sealed class LightweightControlRenderer : LightweightRenderer, IUserControlRenderer, IContentControlRenderer
    {
        public LightweightControlRenderer(
            Control control)
            : base(control)
        {
        }

        void IUserControlRenderer.Invalidate() => this.Invalidate();

        void IUserControlRenderer.SetContent(IViewRenderer content) => this.Invalidate();

        void IContentControlRenderer.SetContent(IViewRenderer content) => this.Invalidate();

        public override void Paint(
            DrawingContext context)
        {
            if (this.View is IDrawDelegate drawDelegate)
            {
                // An owner-drawn control paints its own background
                drawDelegate.InvokeRender(context, new Rect(0, 0, this.Frame.Width, this.Frame.Height));
            }
            else
            {
                base.Paint(context);
            }
        }
    }

    /// <summary>
    /// A lightweight text block, laid out and drawn with the platform's text
    /// engine through <see cref="TextLayout"/>.
    /// </summary>
    internal sealed class LightweightTextBlockRenderer : LightweightRenderer, ITextBlockRenderer
    {
        private string _text = string.Empty;
        private bool _wordWrap;
        private bool _textTrimming;
        private Color _foregroundColor = Colors.Black;
        private float _fontSize = 15.0f;
        private FontStyle _fontStyle = FontStyle.Normal;
        private FontWeight _fontWeight = FontWeight.Normal;
        private bool _strikethrough;
        private string _fontFamily;
        private int _maxLines;
        private IReadOnlyList<TextSpan> _spans;
        private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
        private TextAlignment _verticalTextAlignment = TextAlignment.Start;

        // The layout for the last width it was made for
        private TextLayout _layout;
        private float _layoutWidth = float.NaN;
        private float _layoutHeight = float.NaN;

        public LightweightTextBlockRenderer(
            TextBlock textBlock)
            : base(textBlock)
        {
        }

        public string Text
        {
            get { return this._text; }
            set { this._text = value ?? string.Empty; this.OnTextChanged(); }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get { return this._horizontalTextAlignment; }
            set { this._horizontalTextAlignment = value; this.OnTextChanged(); }
        }

        public TextAlignment VerticalTextAlignment
        {
            get { return this._verticalTextAlignment; }
            set { this._verticalTextAlignment = value; this.Invalidate(); }
        }

        public bool WordWrap
        {
            get { return this._wordWrap; }
            set { this._wordWrap = value; this.OnTextChanged(); }
        }

        public bool TextTrimming
        {
            get { return this._textTrimming; }
            set { this._textTrimming = value; this.OnTextChanged(); }
        }

        public Color ForegroundColor
        {
            get { return this._foregroundColor; }
            set { this._foregroundColor = value; this.Invalidate(); }
        }

        public float FontSize
        {
            get { return this._fontSize; }
            set { this._fontSize = value; this.OnTextChanged(); }
        }

        public FontStyle FontStyle
        {
            get { return this._fontStyle; }
            set { this._fontStyle = value; this.OnTextChanged(); }
        }

        public FontWeight FontWeight
        {
            get { return this._fontWeight; }
            set { this._fontWeight = value; this.OnTextChanged(); }
        }

        public bool Strikethrough
        {
            get { return this._strikethrough; }
            set { this._strikethrough = value; this.OnTextChanged(); }
        }

        public string FontFamily
        {
            set { this._fontFamily = value; this.OnTextChanged(); }
        }

        public int MaxLines
        {
            set { this._maxLines = value; this.OnTextChanged(); }
        }

        public IReadOnlyList<TextSpan> Spans
        {
            set { this._spans = value; this.OnTextChanged(); }
        }

        private void OnTextChanged()
        {
            this.DisposeLayout();
            this.InvalidateMeasure();
            this.Invalidate();
        }

        private void DisposeLayout()
        {
            this._layout?.Dispose();
            this._layout = null;
            this._layoutWidth = float.NaN;
            this._layoutHeight = float.NaN;
        }

        private TextLayout GetLayout(
            float maxWidth,
            float maxHeight)
        {
            if ((null != this._layout) && (maxWidth == this._layoutWidth) && (maxHeight == this._layoutHeight))
            {
                return this._layout;
            }

            this.DisposeLayout();

            var format = new TextFormat()
            {
                FontFamily = this._fontFamily,
                FontSize = this._fontSize,
                FontWeight = this._fontWeight,
                FontStyle = this._fontStyle,
                HorizontalAlignment = (this._horizontalTextAlignment == TextAlignment.Center) ? HorizontalAlignment.Center
                    : (this._horizontalTextAlignment == TextAlignment.End) ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left,
                TextWrapping = this._wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                TextTrimming = this._textTrimming ? XForms.TextTrimming.CharacterEllipsis : XForms.TextTrimming.None,
                MaxLines = this._maxLines,
            };

            var spans = this._spans;
            if (this._strikethrough && (this._text.Length > 0))
            {
                var all = new List<TextSpan>(spans ?? Array.Empty<TextSpan>());
                all.Add(new TextSpan(0, this._text.Length, strikethrough: true));
                spans = all;
            }

            this._layout = new TextLayout(this._text, format, maxWidth, maxHeight, spans);
            this._layoutWidth = maxWidth;
            this._layoutHeight = maxHeight;
            return this._layout;
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return this.GetLayout(availableSize.Width, availableSize.Height).Size;
        }

        public override void Paint(
            DrawingContext context)
        {
            base.Paint(context);

            // NOTE: A little slack, so text arranged at exactly its measured
            // width isn't wrapped or trimmed by rounding
            var frame = this.Frame;
            var layout = this.GetLayout(frame.Width + 1, frame.Height + 1);

            float y = 0;
            float extra = frame.Height - layout.Size.Height;
            if (extra > 0)
            {
                if (this._verticalTextAlignment == TextAlignment.Center)
                {
                    y = extra / 2;
                }
                else if (this._verticalTextAlignment == TextAlignment.End)
                {
                    y = extra;
                }
            }

            context.DrawText(layout, new Point(0, y), this._foregroundColor);
        }
    }

    /// <summary>
    /// Draws a layer's lightweight descendants into its surface.
    /// </summary>
    internal static class LightweightPainter
    {
        /// <summary>
        /// Draws the lightweight views inside the layer's view (in drawing
        /// order, each within its ancestors' positions), beneath the layer's
        /// native children. The context draws in the layer's coordinates.
        /// </summary>
        public static void PaintDescendants(
            View layerView,
            DrawingContext context)
        {
            if (!layerView.HasLightweightChildren)
            {
                return;
            }

            PaintChildren(layerView, context);
        }

        private static void PaintChildren(
            View view,
            DrawingContext context)
        {
            var children = view.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!(child.CreatedRenderer is LightweightRenderer renderer) || !renderer.IsVisible)
                {
                    continue;
                }

                var frame = renderer.Frame;
                var translation = context.PushTransform(System.Numerics.Matrix3x2.CreateTranslation(frame.X, frame.Y));
                var clip = renderer.ClipsToBounds ? context.PushClip(new Rect(0, 0, frame.Width, frame.Height)) : default;

                var previous = context.LightweightBounds;
                context.LightweightBounds = new Rect(0, 0, frame.Width, frame.Height);
                try
                {
                    renderer.Paint(context);
                }
                finally
                {
                    context.LightweightBounds = previous;
                }

                PaintChildren(child, context);

                clip.Dispose();
                translation.Dispose();
            }
        }
    }
}
