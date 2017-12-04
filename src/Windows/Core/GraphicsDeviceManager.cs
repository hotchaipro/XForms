using System;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Windows.Graphics.Display;
using Windows.UI.Composition;

namespace XForms.Windows
{
    internal sealed class GraphicsManager
    {
        private CanvasDevice _canvasDevice;
        private CompositionGraphicsDevice _graphicsDevice;

        public static readonly GraphicsManager Shared = new GraphicsManager();

        public event EventHandler ReloadResources;
        public event EventHandler Redraw;

        private GraphicsManager()
        {
            DisplayInformation.DisplayContentsInvalidated += DisplayInformation_DisplayContentsInvalidated;

            // TODO: Handle DPI change
            // DisplayInformation.GetForCurrentView().DpiChanged += DisplayInformation_DpiChanged;
        }

        public CanvasDevice GetCanvasDevice()
        {
            if (null == this._canvasDevice)
            {
                var canvasDevice = CanvasDevice.GetSharedDevice();
                canvasDevice.DeviceLost += CanvasDevice_DeviceLost;
                this._canvasDevice = canvasDevice;
            }

            return this._canvasDevice;
        }

        public CompositionGraphicsDevice GetCompositionGraphicsDevice(
            Compositor compositor)
        {
            if (null == compositor)
            {
                throw new ArgumentNullException(nameof(compositor));
            }

            CanvasDevice canvasDevice = this._canvasDevice;
            if (null == canvasDevice)
            {
                canvasDevice = GetCanvasDevice();
            }

            if (null == this._graphicsDevice)
            {
                this._graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);
                this._graphicsDevice.RenderingDeviceReplaced += GraphicsDevice_RenderingDeviceReplaced;
            }

            return this._graphicsDevice;
        }

        public void ResumeDevice()
        {
            this.EnsureGraphicsResources();
        }

        private void DisplayInformation_DisplayContentsInvalidated(
            DisplayInformation sender,
            object args)
        {
            this.EnsureGraphicsResources();
        }

        private void EnsureGraphicsResources()
        {
            // NOTE: GetSharedDevice raises the DeviceLost event if the device is invalid
            CanvasDevice.GetSharedDevice();

            // Redraw graphics resources on resume
            this.RedrawGraphicsResources();
        }

        private void CanvasDevice_DeviceLost(
            CanvasDevice sender,
            object args)
        {
            sender.DeviceLost -= this.CanvasDevice_DeviceLost;

            var canvasDevice = CanvasDevice.GetSharedDevice();
            canvasDevice.DeviceLost += this.CanvasDevice_DeviceLost;
            this._canvasDevice = canvasDevice;

            if (null != this._graphicsDevice)
            {
                // NOTE: This raises the RenderingDeviceReplaced event
                CanvasComposition.SetCanvasDevice(this._graphicsDevice, canvasDevice);
            }
        }

        private void GraphicsDevice_RenderingDeviceReplaced(
            CompositionGraphicsDevice sender,
            RenderingDeviceReplacedEventArgs args)
        {
            this.ReloadGraphicsResources();
        }

        private async void ReloadGraphicsResources()
        {
            await Task.Factory.StartNew(() =>
            {
                this.ReloadResources?.Invoke(this, EventArgs.Empty);
            }, TaskCreationOptions.LongRunning);
            this.RedrawGraphicsResources();
        }

        private void RedrawGraphicsResources()
        {
            this.Redraw?.Invoke(this, EventArgs.Empty);
        }
    }
}
