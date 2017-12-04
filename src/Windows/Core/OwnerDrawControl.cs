using System;
using System.Numerics;
using Microsoft.Graphics.Canvas.UI.Composition;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using XForms.Controls;

namespace XForms.Windows
{
    internal sealed class OwnerDrawControl : global::Windows.UI.Xaml.Controls.ContentControl
    {
        private GraphicsManager _graphicsManager;
        private SpriteVisual _drawingVisual;
        private CompositionDrawingSurface _drawingSurface;
        private CompositionSurfaceBrush _compositionSurfaceBrush;
        private float _dpiScaleFactor;
        private Matrix3x2 _dpiTransform;
        private IDrawDelegate _drawDelegate;

        public OwnerDrawControl(
            IDrawDelegate drawDelegate)
        {
            if (null == drawDelegate)
            {
                throw new ArgumentNullException(nameof(drawDelegate));
            }

            this._drawDelegate = drawDelegate;

            this.Loaded += Control_Loaded;
            this.Unloaded += Control_Unloaded;
            this.SizeChanged += Control_SizeChanged;

            // Specify a custom control template that respects the Background property and enables hit testing
            var controlTemplate = (ControlTemplate)global::Windows.UI.Xaml.Application.Current.Resources["OwnerDrawControlTemplate"];
            this.Template = controlTemplate;

            // HACK: Enable hit testing
            this.Background = new global::Windows.UI.Xaml.Media.SolidColorBrush(global::Windows.UI.Colors.Transparent);

            this._graphicsManager = GraphicsManager.Shared;
            this._graphicsManager.Redraw += GraphicsManager_Redraw;
        }

        private void GraphicsManager_Redraw(
            object sender,
            EventArgs e)
        {
            this.Invalidate();
        }

        private void Control_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            // Nothing to do if the drawing surface has already been created
            if (this._drawingSurface != null)
            {
                return;
            }

            this.InitializeDrawingSurface();
        }

        private void Control_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            // NOTE: The unload and load events don't seem to be matched, so 
            // the drawing surface cannot be reliably removed.
            // TODO: Figure out how to reliably release resources
        }

        private void Control_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            // Update the size of the drawing visual
            if (null != this._drawingVisual)
            {
                this._drawingVisual.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
            }

            // Update the size of the drawing surface
            if (null != this._drawingSurface)
            {
                CanvasComposition.Resize(this._drawingSurface, new global::Windows.Foundation.Size(e.NewSize.Width * this._dpiScaleFactor, e.NewSize.Height * this._dpiScaleFactor));
            }

            // Redraw the control
            this.Invalidate();
        }

        private void InitializeDrawingSurface()
        {
            if (!global::Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2))
            {
                throw new NotSupportedException("This version of Windows does not support the required Composition API.");
            }

            // Clean up any previously created resources
            if (null != this._drawingVisual)
            {
                this._drawingVisual.Brush = null;
                this._drawingSurface.Dispose();
                this._drawingSurface = null;
            }
            this._compositionSurfaceBrush?.Dispose();
            this._compositionSurfaceBrush = null;
            this._drawingSurface?.Dispose();
            this._drawingSurface = null;

            // Get the DPI scale factor
            // TODO: Handle DPI changes
            float logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            this._dpiScaleFactor = logicalDpi / 96f;
            this._dpiTransform = Matrix3x2.CreateScale(this._dpiScaleFactor);

            // Get the host visual and compositor
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(this);
            Compositor compositor = hostVisual.Compositor;

            // Get the existing drawing visual from the element visual tree
            this._drawingVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(this);
            if (null == this._drawingVisual)
            {
                // Create a new drawing visual
                this._drawingVisual = compositor.CreateSpriteVisual();

                // Add the drawing visual to the visual tree
                ElementCompositionPreview.SetElementChildVisual(this, this._drawingVisual);
            }

            // Set the initial size of the drawing visual
            this._drawingVisual.Size = new System.Numerics.Vector2((float)this.RenderSize.Width, (float)this.RenderSize.Height);

            // Create a new drawing surface
            // NOTE: The drawing surface object must be a member variable so it is not garbage collected
            var compositionGraphicsDevice = this._graphicsManager.GetCompositionGraphicsDevice(compositor);
            this._drawingSurface = compositionGraphicsDevice.CreateDrawingSurface(
                new global::Windows.Foundation.Size(this.RenderSize.Width * this._dpiScaleFactor, this.RenderSize.Height * this._dpiScaleFactor),
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                DirectXAlphaMode.Premultiplied);

            // Create a surface brush from the drawing surface
            this._compositionSurfaceBrush = compositor.CreateSurfaceBrush(this._drawingSurface);
            this._compositionSurfaceBrush.Stretch = CompositionStretch.Fill;

            // Set the drawing visual brush to the drawing surface
            this._drawingVisual.Brush = this._compositionSurfaceBrush;

            // Draw the control
            this.Invalidate();
        }

        internal void Invalidate()
        {
            if ((null == this._drawingSurface) || (this.Visibility != Visibility.Visible))
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, (float)this._drawingVisual.Size.X, (float)this._drawingVisual.Size.Y);
            if ((bounds.Width > 0) && (bounds.Height > 0))
            {
                using (var drawingSession = CanvasComposition.CreateDrawingSession(this._drawingSurface))
                {
                    drawingSession.Transform = this._dpiTransform;

                    var drawContext = new DrawContext(drawingSession);
                    this._drawDelegate?.InvokeDrawBackground(drawContext, bounds);
                }
            }
        }
    }
}
