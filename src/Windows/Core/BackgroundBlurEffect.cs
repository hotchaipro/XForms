using System;
using Microsoft.Graphics.Canvas.Effects;
using global::Windows.UI.Composition;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Hosting;

namespace XForms.Windows
{
    internal sealed class BackgroundBlurEffect
    {
        private UIElement _foregroundElement;
        private GaussianBlurEffect _blurEffect;
        private CompositionEffectBrush _effectBrush;
        private CompositionBrush _backdropBrush;
        private SpriteVisual _blurVisual;
        private ExpressionAnimation _bindSizeAnimation;

        public BackgroundBlurEffect(
            UIElement foregroundElement)
        {
            if (null == foregroundElement)
            {
                throw new ArgumentNullException(nameof(foregroundElement));
            }

            this._foregroundElement = foregroundElement;
        }

        public void Apply(
            float blurAmount)
        {
            if (global::Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("global::Windows.UI.Composition.Compositor", "CreateBackdropBrush"))
            {
                this.ApplyInternal(blurAmount);
            }
            else
            {
                this.ApplyFallback();
            }
        }

        private void ApplyInternal(
            float blurAmount)
        {
            // SOURCE: https://msdn.microsoft.com/en-us/windows/uwp/graphics/using-the-visual-layer-with-xaml

            // Get the host visual and compositor
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(this._foregroundElement);
            Compositor compositor = hostVisual.Compositor;

            // Create a blur effect
            this._blurEffect = new GaussianBlurEffect
            {
                BlurAmount = blurAmount,
                BorderMode = EffectBorderMode.Hard,
                //Source = new CompositionEffectSourceParameter("backdropBrush"),
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = global::Windows.UI.Color.FromArgb(255, 0xee, 0xee, 0xee),
                    }
                }
            };

            //  Create an instance of the effect
            using (var effectFactory = compositor.CreateEffectFactory(this._blurEffect))
            {
                this._effectBrush = effectFactory.CreateBrush();
            }

            // Set the effect source to a CompositionBackdropBrush
            this._backdropBrush = compositor.CreateBackdropBrush();
            this._effectBrush.SetSourceParameter("backdropBrush", this._backdropBrush);

            // Get any existing child sprite visual
            this._blurVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(this._foregroundElement);
            if (null == this._blurVisual)
            {
                // Create a sprite visual to contain the effect
                this._blurVisual = compositor.CreateSpriteVisual();
                this._blurVisual.Brush = this._effectBrush;

                // Add the sprite visual as a child of the host visual in the visual tree
                ElementCompositionPreview.SetElementChildVisual(this._foregroundElement, this._blurVisual);
            }
            else
            {
                this._blurVisual.Brush = this._effectBrush;
            }

            // Make sure size of the host and effect visuals always stay in sync
            this._bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            this._bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);
            this._blurVisual.StartAnimation("Size", this._bindSizeAnimation);
        }

        private void ApplyFallback()
        {
            //// Get the host visual and compositor
            //Visual hostVisual = ElementCompositionPreview.GetElementVisual(this._foregroundElement);
            //var compositor = hostVisual.Compositor;

            //// Create a color brush
            //this._backdropBrush = compositor.CreateColorBrush(global::Windows.UI.Color.FromArgb(0x80, 0xff, 0xff, 0xff));

            //// Get any existing child visual
            //this._blurVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(this._foregroundElement);
            //if (null == this._blurVisual)
            //{
            //    // Create a sprite visual to show the effect
            //    _blurVisual = compositor.CreateSpriteVisual();
            //    _blurVisual.Brush = this._backdropBrush;

            //    // Add the sprite visual as a child of the host visual in the visual tree
            //    ElementCompositionPreview.SetElementChildVisual(this._foregroundElement, _blurVisual);
            //}
            //else
            //{
            //    _blurVisual.Brush = this._backdropBrush;
            //}
            //this._blurVisual.Size = new System.Numerics.Vector2(
            //    (float)this._foregroundElement.RenderSize.Width,
            //    (float)this._foregroundElement.RenderSize.Height);

            this._foregroundElement.Opacity = 0.05;
        }

        public void Remove()
        {
            // Remove effect

            if (null != this._foregroundElement)
            {
                this._foregroundElement.Opacity = 1.0f;
            }

            if (null != this._blurVisual)
            {
                this._blurVisual.Brush = null;
                this._blurVisual?.StopAnimation("Size");
            }

            this._bindSizeAnimation?.Dispose();
            this._bindSizeAnimation = null;

            this._effectBrush?.Dispose();
            this._effectBrush = null;

            this._backdropBrush?.Dispose();
            this._backdropBrush = null;

            this._blurEffect?.Dispose();
            this._blurEffect = null;

            // NOTE: No API to remove the child visual
            //this._blurVisual?.Dispose();
            //this._blurVisual = null;
        }
    }
}
