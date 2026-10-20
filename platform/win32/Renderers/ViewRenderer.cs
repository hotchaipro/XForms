using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.System.Com;
using XForms.Controls;

namespace XForms.Win32.Renderers
{
    internal abstract class ElementRenderer : IElementRenderer
    {
        protected ElementRenderer(
            Element element)
        {
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// For platform-only views (such as a picker's drop-down) with no XForms element.
        /// </summary>
        protected ElementRenderer()
        {
        }

        public Element Element
        {
            get;
        }

        public virtual object NativeElement
        {
            get
            {
                return this;
            }
        }

        public bool HasDescendant(
            Element element)
        {
            return false;
        }

        public virtual void OnApplyTheme(
            AppTheme theme)
        {
        }
    }

    /// <summary>
    /// A view backed by a DirectComposition visual. Views that draw (labels,
    /// owner-drawn controls, backgrounds) render into a composition surface.
    /// </summary>
    internal unsafe class ViewRenderer : ElementRenderer, IViewRenderer, IControlRenderer
    {
        private readonly bool _isContainer;
        private IDCompositionVisual3* _visual;
        private IDCompositionSurface* _surface;
        private IDCompositionScaleTransform* _scaleTransform;
        private int _surfaceWidth, _surfaceHeight;
        private Rect _frame;
        private Point _translation = Point.Zero;
        private float _scale = 1.0f;
        private Angle _rotation = Angle.Zero;
        private float _opacity = 1.0f;
        private bool _isVisible = true;
        private bool _clipToBounds;
        private Color _backgroundColor = Colors.Transparent;

        protected ViewRenderer(
            View view,
            bool isContainer)
            : base(view)
        {
            this._isContainer = isContainer;
            this.CreateVisual();
        }

        /// <summary>
        /// For platform-only views with no XForms element; they lay out and draw
        /// themselves.
        /// </summary>
        protected ViewRenderer()
        {
            this.CreateVisual();
        }

        private void CreateVisual()
        {
            IDCompositionVisual2* visual;
            this.Host.CompositionDevice->CreateVisual(&visual);

            ((IUnknown*)visual)->QueryInterface(IDCompositionVisual3.IID_Guid, out var visual3).ThrowOnFailure();
            visual->Release();
            this._visual = (IDCompositionVisual3*)visual3;
        }

        protected Win32Host Host
        {
            get
            {
                return Win32Host.Current;
            }
        }

        public IDCompositionVisual2* Visual
        {
            get
            {
                return (IDCompositionVisual2*)this._visual;
            }
        }

        public ViewRenderer ParentRenderer
        {
            get;
            private set;
        }

        public List<ViewRenderer> Children
        {
            get;
        } = new List<ViewRenderer>();

        public Rect Frame
        {
            get
            {
                return this._frame;
            }
        }

        /// <summary>
        /// Offset applied to the children, such as a scroll position.
        /// </summary>
        public virtual Point ChildOffset
        {
            get
            {
                return Point.Zero;
            }
        }

        /// <summary>
        /// The visual the children are added to.
        /// </summary>
        protected virtual IDCompositionVisual2* ChildVisual
        {
            get
            {
                return this.Visual;
            }
        }

        #region Children

        public void InsertChild(
            int index,
            ViewRenderer child)
        {
            child.ParentRenderer?.RemoveChild(child);

            var reference = (index > 0) ? (IDCompositionVisual*)this.Children[index - 1].Visual : null;
            // NOTE: With a reference visual, insertAbove places the child in front of
            // it; without one, insertAbove places the child behind all its siblings
            this.ChildVisual->AddVisual((IDCompositionVisual*)child.Visual, true, reference);

            this.Children.Insert(index, child);
            child.ParentRenderer = this;

            this.Host.RequestLayout();
        }

        public void AddChild(
            ViewRenderer child)
        {
            this.InsertChild(this.Children.Count, child);
        }

        public void RemoveChild(
            ViewRenderer child)
        {
            if (this.Children.Remove(child))
            {
                this.ChildVisual->RemoveVisual((IDCompositionVisual*)child.Visual);
                child.ParentRenderer = null;
                this.Host.RequestLayout();
            }
        }

        public void ClearChildren()
        {
            foreach (var child in this.Children)
            {
                child.ParentRenderer = null;
            }

            this.Children.Clear();
            this.ChildVisual->RemoveAllVisuals();
            this.Host.RequestLayout();
        }

        #endregion Children

        #region Layout

        public virtual Size MeasureNative(
            Size availableSize)
        {
            if (this._isContainer)
            {
                return ((View)this.Element).MeasureContent(availableSize);
            }

            return Size.Empty;
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            bool isResized = (frame.Width != this._frame.Width) || (frame.Height != this._frame.Height);

            this._frame = frame;
            this.UpdateOffset();

            if (isResized)
            {
                this.UpdateTransform();
                this.UpdateClip();
                this.OnResized();
            }

            if (this._isContainer)
            {
                ((View)this.Element).LayoutContent(frame.Size);
            }
        }

        public virtual void InvalidateMeasure()
        {
            this.Host.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                this._clipToBounds = value;
                this.UpdateClip();
            }
        }

        private void UpdateClip()
        {
            if (!this._clipToBounds)
            {
                // Scrolling views set their own clip
                return;
            }

            float scale = this.Host.Scale;
            this._visual->SetClip(new D2D_RECT_F() { left = 0, top = 0, right = this._frame.Width * scale, bottom = this._frame.Height * scale });
        }

        protected virtual void OnResized()
        {
            if (null != this._spin)
            {
                // Keep spinning around the center
                float scale = this.Host.Scale;
                this._spin->SetCenterX(this._frame.Width * scale / 2);
                this._spin->SetCenterY(this._frame.Height * scale / 2);
            }

            if (this.HasContent)
            {
                this.InvalidateRender();
            }
        }

        public Point AbsoluteOrigin()
        {
            float x = 0, y = 0;
            for (var renderer = this; null != renderer; renderer = renderer.ParentRenderer)
            {
                x += renderer._frame.X + renderer._translation.X;
                y += renderer._frame.Y + renderer._translation.Y;

                var parent = renderer.ParentRenderer;
                if (null != parent)
                {
                    var offset = parent.ChildOffset;
                    x += offset.X;
                    y += offset.Y;
                }
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Finds the deepest visible view under the point (in the parent's coordinates).
        /// </summary>
        public ViewRenderer HitTest(
            Point point)
        {
            if (!this.IsEffectivelyVisible)
            {
                return null;
            }

            float x = point.X - this._frame.X - this._translation.X;
            float y = point.Y - this._frame.Y - this._translation.Y;

            if ((x < 0) || (y < 0) || (x >= this._frame.Width) || (y >= this._frame.Height))
            {
                return null;
            }

            var offset = this.ChildOffset;
            var childPoint = new Point(x - offset.X, y - offset.Y);

            for (int i = this.Children.Count - 1; i >= 0; i -= 1)
            {
                var hit = this.Children[i].HitTest(childPoint);
                if (null != hit)
                {
                    return hit;
                }
            }

            return this;
        }

        private bool IsEffectivelyVisible
        {
            get
            {
                return this._isVisible && (this._opacity > 0);
            }
        }

        public bool IsShownOnScreen
        {
            get
            {
                for (var renderer = this; null != renderer; renderer = renderer.ParentRenderer)
                {
                    if (!renderer._isVisible)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void UpdateOffset()
        {
            float scale = this.Host.Scale;

            // NOTE: Snap to device pixels so drawn content stays crisp. A parent's
            // child offset (scrolling) is applied by the parent's own visuals.
            this._visual->SetOffsetX((float)Math.Round((this._frame.X + this._translation.X) * scale));
            this._visual->SetOffsetY((float)Math.Round((this._frame.Y + this._translation.Y) * scale));
        }

        #endregion Layout

        #region Drawing

        protected virtual bool HasContent
        {
            get
            {
                return !this._backgroundColor.IsTransparent;
            }
        }

        public void InvalidateRender()
        {
            this.Host.RequestRender(this);
        }

        public void InvalidateRenderRecursive()
        {
            this._surfaceWidth = 0;
            this._surfaceHeight = 0;
            this.InvalidateRender();
            this.UpdateOffset();

            foreach (var child in this.Children)
            {
                child.InvalidateRenderRecursive();
            }
        }

        /// <summary>
        /// Draws the content into the visual's surface.
        /// </summary>
        public void Render()
        {
            float scale = this.Host.Scale;
            int width = (int)Math.Ceiling(this._frame.Width * scale);
            int height = (int)Math.Ceiling(this._frame.Height * scale);

            if ((!this.HasContent) || (width <= 0) || (height <= 0))
            {
                if (null != this._surface)
                {
                    this._visual->SetContent(null);
                    this._surface->Release();
                    this._surface = null;
                    this._surfaceWidth = 0;
                    this._surfaceHeight = 0;
                }

                return;
            }

            if ((null == this._surface) || (width != this._surfaceWidth) || (height != this._surfaceHeight))
            {
                if (null != this._surface)
                {
                    this._surface->Release();
                }

                IDCompositionSurface* surface;
                this.Host.CompositionDevice->CreateSurface(
                    (uint)width,
                    (uint)height,
                    DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                    DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED,
                    &surface);

                this._surface = surface;
                this._surfaceWidth = width;
                this._surfaceHeight = height;
                this._visual->SetContent((IUnknown*)surface);
            }

            this._surface->BeginDraw(null, ID2D1DeviceContext.IID_Guid, out var contextPointer, out var surfaceOffset);
            var context = (ID2D1DeviceContext*)contextPointer;
            try
            {
                // Work in device-independent pixels; the surface offset is in device pixels
                context->SetDpi(96, 96);
                context->SetTransform(new D2D_MATRIX_3X2_F()
                {
                    Anonymous = new D2D_MATRIX_3X2_F._Anonymous_e__Union()
                    {
                        Anonymous1 = new D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct()
                        {
                            m11 = scale,
                            m22 = scale,
                            dx = surfaceOffset.X,
                            dy = surfaceOffset.Y,
                        },
                    },
                });

                var drawContext = new DrawContext(context, new Rect(0, 0, this._frame.Width, this._frame.Height));
                drawContext.Clear(this._backgroundColor);
                this.DrawContent(drawContext);
                drawContext.Release();
            }
            finally
            {
                this._surface->EndDraw();
                context->Release();
            }
        }

        protected virtual void DrawContent(
            DrawContext drawContext)
        {
        }

        #endregion Drawing

        #region Properties

        public virtual bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                this._isVisible = value;
                this.UpdateOpacity();
                this.OnVisibilityChanged();
            }
        }

        protected virtual void OnVisibilityChanged()
        {
            // Text entry windows follow their views' visibility on the next update
            this.Host.RequestTextBoxPositionUpdate();
        }

        public bool IsHitTestVisible
        {
            get;
            set;
        } = true;

        public virtual Color BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;
                this.InvalidateRender();
            }
        }

        public float Opacity
        {
            get
            {
                return this._opacity;
            }

            set
            {
                this._opacity = value;
                this.UpdateOpacity();
            }
        }

        private void UpdateOpacity()
        {
            this._visual->SetOpacity(this._isVisible ? this._opacity : 0);
            this.Host.RequestCommit();
        }

        public float Scale
        {
            get
            {
                return this._scale;
            }

            set
            {
                this._scale = value;
                this.UpdateTransform();
            }
        }

        public Angle Rotation
        {
            get
            {
                return this._rotation;
            }

            set
            {
                // NOTE: The prototype stores rotation but does not apply it
                this._rotation = value;
            }
        }

        public Point Translation
        {
            get
            {
                return this._translation;
            }

            set
            {
                this._translation = value;
                this.UpdateOffset();
                this.Host.RequestCommit();
            }
        }

        private void EnsureScaleTransform()
        {
            if (null == this._scaleTransform)
            {
                IDCompositionScaleTransform* scaleTransform;
                this.Host.CompositionDevice->CreateScaleTransform(&scaleTransform);
                this._scaleTransform = scaleTransform;
                this._visual->SetTransform((IDCompositionTransform*)scaleTransform);
            }
        }

        private void UpdateTransform()
        {
            if ((null == this._scaleTransform) && (this._scale == 1.0f))
            {
                return;
            }

            this.EnsureScaleTransform();

            float scale = this.Host.Scale;
            this._scaleTransform->SetCenterX(this._frame.Width * scale / 2);
            this._scaleTransform->SetCenterY(this._frame.Height * scale / 2);
            this._scaleTransform->SetScaleX(this._scale);
            this._scaleTransform->SetScaleY(this._scale);
            this.Host.RequestCommit();
        }

        #endregion Properties

        #region Animation

        // Animations run on the compositor; the task completes after the duration
        private int _animationGeneration;

        public Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            this.EnsureScaleTransform();
            this.UpdateTransform();

            var animation = this.CreateAnimation(this._scale, scale, duration, ease);
            this._scaleTransform->SetScaleX(animation);
            this._scaleTransform->SetScaleY(animation);
            animation->Release();

            this._scale = scale;
            return this.CompleteAfter(duration, this.UpdateTransform);
        }

        public Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            this._rotation = angle;
            return Task.CompletedTask;
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            float scale = this.Host.Scale;
            float baseX = this._frame.X * scale;
            float baseY = this._frame.Y * scale;

            var animationX = this.CreateAnimation(baseX + (this._translation.X * scale), baseX + (point.X * scale), duration, ease);
            var animationY = this.CreateAnimation(baseY + (this._translation.Y * scale), baseY + (point.Y * scale), duration, ease);
            this._visual->SetOffsetX(animationX);
            this._visual->SetOffsetY(animationY);
            animationX->Release();
            animationY->Release();

            this._translation = point;
            return this.CompleteAfter(duration, this.UpdateOffset);
        }

        private IDCompositionRotateTransform* _spin;

        public void StartContinuousRotation(
            TimeSpan period)
        {
            if (null == this._spin)
            {
                IDCompositionRotateTransform* rotation;
                this.Host.CompositionDevice->CreateRotateTransform(&rotation);
                this._spin = rotation;

                // NOTE: The rotation replaces any scale transform while it runs
                this._visual->SetTransform((IDCompositionTransform*)rotation);
            }

            float scale = this.Host.Scale;
            this._spin->SetCenterX(this._frame.Width * scale / 2);
            this._spin->SetCenterY(this._frame.Height * scale / 2);

            // One turn per period, repeated forever, on the compositor
            double seconds = period.TotalSeconds;
            IDCompositionAnimation* animation;
            this.Host.CompositionDevice->CreateAnimation(&animation);
            animation->AddCubic(0, 0, (float)(360 / seconds), 0, 0);
            animation->AddRepeat(seconds, seconds);
            this._spin->SetAngle(animation);
            animation->Release();

            this.Host.RequestCommit();
        }

        public void StopContinuousRotation()
        {
            if (null != this._spin)
            {
                this._visual->SetTransform((IDCompositionTransform*)null);
                this._spin->Release();
                this._spin = null;

                if (null != this._scaleTransform)
                {
                    this._visual->SetTransform((IDCompositionTransform*)this._scaleTransform);
                }

                this.Host.RequestCommit();
            }
        }

        public bool Focus()
        {
            var focus = this.Host.Focus;
            focus.SetFocus(this, focus.IsRingVisible);
            return true;
        }

        public void StopAnimation()
        {
            this._animationGeneration += 1;
            this.UpdateOffset();
            this.UpdateTransform();
            this.Host.RequestCommit();
        }

        private IDCompositionAnimation* CreateAnimation(
            float from,
            float to,
            TimeSpan duration,
            EasingFunction ease)
        {
            IDCompositionAnimation* animation;
            this.Host.CompositionDevice->CreateAnimation(&animation);

            double seconds = Math.Max(0.001, duration.TotalSeconds);
            float delta = to - from;
            float t = (float)seconds;

            var mode = (ease as CubicEase)?.EasingMode ?? ((ease is SpringEase) ? EasingMode.EaseOut : (EasingMode?)null);

            switch (mode)
            {
                case EasingMode.EaseIn:
                    // from + delta * u^3
                    animation->AddCubic(0, from, 0, 0, delta / (t * t * t));
                    break;

                case EasingMode.EaseOut:
                    // from + delta * (1 - (1 - u)^3) = from + delta * (3u - 3u^2 + u^3)
                    animation->AddCubic(0, from, 3 * delta / t, -3 * delta / (t * t), delta / (t * t * t));
                    break;

                case EasingMode.EaseInOut:
                    {
                        // Ease in over the first half, ease out over the second
                        float half = t / 2;
                        float halfDelta = delta / 2;
                        animation->AddCubic(0, from, 0, 0, halfDelta / (half * half * half));
                        animation->AddCubic(half, from + halfDelta, 3 * halfDelta / half, -3 * halfDelta / (half * half), halfDelta / (half * half * half));
                        break;
                    }

                default:
                    animation->AddCubic(0, from, delta / t, 0, 0);
                    break;
            }

            animation->End(seconds, to);
            return animation;
        }

        private Task CompleteAfter(
            TimeSpan duration,
            Action settle)
        {
            int generation = ++this._animationGeneration;

            this.Host.RequestCommit();

            var completion = new TaskCompletionSource<object>();

            Task.Delay(duration).ContinueWith(
                (task) => this.Host.Post(() =>
                {
                    // Replace the finished animation with the final static value
                    if (generation == this._animationGeneration)
                    {
                        settle();
                        this.Host.RequestCommit();
                    }

                    completion.TrySetResult(null);
                }),
                TaskScheduler.Default);

            return completion.Task;
        }

        #endregion Animation
    }
}
