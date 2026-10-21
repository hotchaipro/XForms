using System;
using System.Threading.Tasks;
using Android.Views;
using Android.Views.Animations;
using Android.Animation;
using Android.Graphics.Drawables;
using AndroidView = global::Android.Views.View;

namespace XForms.Android.Renderers
{
    public abstract class ViewRenderer<TNativeElement> : ElementRenderer<TNativeElement>, IViewRenderer
        where TNativeElement : AndroidView
    {
        private AndroidView _nativeView;
        private AnimationLayerStack _layers;

        public ViewRenderer(
            global::Android.Content.Context context,
            View view)
            : base(context, view)
        {
        }

        private IViewDelegate ViewDelegate
        {
            get
            {
                return (IViewDelegate)this.Element;
            }
        }

        protected float PixelsToDevicePixels(
            float pixels)
        {
            return NativeConversions.PixelsToDevicePixels(this.NativeContext, pixels);
        }

        protected float DevicePixelsToPixels(
            float dp)
        {
            return NativeConversions.DevicePixelsToPixels(this.NativeContext, dp);
        }

        public virtual bool IsVisible
        {
            get
            {
                bool isVisible;

                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    isVisible = (nativeView.Visibility == global::Android.Views.ViewStates.Visible);
                }
                else
                {
                    isVisible = true;
                }

                return isVisible;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Visibility = (value ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);

                    nativeView.RequestLayout();
                }
            }
        }

        public virtual Size MeasureNative(
            Size availableSize)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return Size.Empty;
            }

            nativeView.Measure(
                this.ToMeasureSpec(availableSize.Width),
                this.ToMeasureSpec(availableSize.Height));

            return new Size(
                this.PixelsToDevicePixels(nativeView.MeasuredWidth),
                this.PixelsToDevicePixels(nativeView.MeasuredHeight));
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return;
            }

            // Round each edge (not the size) so adjacent views meet without gaps
            int left = (int)Math.Round(this.DevicePixelsToPixels(frame.X));
            int top = (int)Math.Round(this.DevicePixelsToPixels(frame.Y));
            int right = (int)Math.Round(this.DevicePixelsToPixels(frame.X + frame.Width));
            int bottom = (int)Math.Round(this.DevicePixelsToPixels(frame.Y + frame.Height));

            // Never lose a pixel to edge rounding: a view arranged at exactly its
            // measured size (text in particular) would otherwise wrap or clip
            right = Math.Max(right, left + (int)Math.Ceiling(this.DevicePixelsToPixels(frame.Width) - 0.01f));
            bottom = Math.Max(bottom, top + (int)Math.Ceiling(this.DevicePixelsToPixels(frame.Height) - 0.01f));

            // NOTE: Always finish with an exact measure at the final size: views
            // such as ListView lay out their content using the mode of the last
            // measure, not just its size. Android caches measurements per spec,
            // so this costs nothing when the view hasn't changed.
            nativeView.Measure(
                AndroidView.MeasureSpec.MakeMeasureSpec(right - left, MeasureSpecMode.Exactly),
                AndroidView.MeasureSpec.MakeMeasureSpec(bottom - top, MeasureSpecMode.Exactly));

            nativeView.Layout(left, top, right, bottom);

            // The transform origin follows the size
            this._layers?.Update();
        }

        public virtual void InvalidateMeasure()
        {
            this._nativeView?.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                // NOTE: ClipChildren clips each child to the child's own bounds, not
                // to this view's. Clipping to the view's outline (its bounds) clips
                // its drawing and all its descendants.
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.OutlineProvider = global::Android.Views.ViewOutlineProvider.Bounds;
                    nativeView.ClipToOutline = value;
                }
            }
        }

        private int ToMeasureSpec(
            float availableLength)
        {
            if (float.IsInfinity(availableLength) || float.IsNaN(availableLength))
            {
                return AndroidView.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            }

            int pixels = (int)Math.Ceiling(this.DevicePixelsToPixels(availableLength));
            return AndroidView.MeasureSpec.MakeMeasureSpec(Math.Max(0, pixels), MeasureSpecMode.AtMost);
        }

        public virtual Color BackgroundColor
        {
            get
            {
                Color color = Colors.Transparent;
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var background = nativeView.Background as ColorDrawable;
                    if (null != background)
                    {
                        color = NativeConversions.FromAndroidColor(background.Color);
                    }
                }
                return color;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                nativeView.SetBackgroundColor(NativeConversions.ToAndroidColor(value));
            }
        }

        public virtual Origin Anchor
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return Origin.TopLeft;
                }

                var pivotX = nativeView.PivotX;
                var pivotY = nativeView.PivotY;

                return new Origin(
                    nativeView.Width == 0 ? 0 : pivotX / nativeView.Width,
                    nativeView.Height == 0 ? 0 : pivotY / nativeView.Height);
            }

            set
            {
                var nativeView = this._nativeView;
                if (nativeView != null)
                {
                    nativeView.PivotX = value.X * nativeView.Width;
                    nativeView.PivotY = value.Y * nativeView.Height;
                }
            }
        }

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            var nativeView = this._nativeView;
            if ((null != this._layers) || (null == nativeView))
            {
                throw new InvalidOperationException("The view's animation layers can't be created.");
            }

            this._layers = new AnimationLayerStack(nativeView, this.DevicePixelsToPixels(1), count);
            return this._layers.Layers;
        }

        // Views hosted for lightweight descendants, after the view's own children
        private int _hostedCount;

        public void AddHostedView(
            IViewRenderer child,
            int index)
        {
            var group = (global::Android.Views.ViewGroup)this._nativeView;
            group.AddView((AndroidView)child.NativeElement, group.ChildCount - this._hostedCount + index);
            this._hostedCount += 1;
        }

        public void RemoveHostedView(
            IViewRenderer child)
        {
            ((global::Android.Views.ViewGroup)this._nativeView).RemoveView((AndroidView)child.NativeElement);
            this._hostedCount -= 1;
        }

        public void InvalidateLayerContent()
        {
            this._nativeView?.Invalidate();
        }

        public bool Focus()
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return false;
            }

            nativeView.Focusable = true;
            nativeView.FocusableInTouchMode = true;
            return nativeView.RequestFocus();
        }

        protected override void OnNativeElementSet(
            TNativeElement oldElement,
            TNativeElement newElement)
        {
            if (null == newElement)
            {
                throw new ArgumentNullException(nameof(newElement));
            }

            base.OnNativeElementSet(oldElement, newElement);

            this._nativeView = newElement;

            if (null != oldElement)
            {
                oldElement.Touch -= Element_Touch;
                oldElement.GenericMotion -= Element_GenericMotion;
                oldElement.FocusChange -= Element_FocusChange;
                oldElement.KeyPress -= Element_KeyPress;
            }

            if (null != newElement)
            {
                newElement.Touch += Element_Touch;
                newElement.GenericMotion += Element_GenericMotion;
                newElement.FocusChange += Element_FocusChange;

                // Text boxes handle their own keys
                if (!(this.Element is XForms.Controls.TextBox))
                {
                    newElement.KeyPress += Element_KeyPress;

                    // Keyboard focus can move to focusable views (with Tab)
                    if ((this.Element is View view) && view.Focusable)
                    {
                        newElement.Focusable = true;
                    }
                }
            }
        }

        // The pointers pressed on this view (Android sends each finger to the
        // view it went down on)
        private readonly System.Collections.Generic.HashSet<int> _pointers = new System.Collections.Generic.HashSet<int>();

        private void Element_Touch(
            object sender,
            AndroidView.TouchEventArgs e)
        {
            bool result = false;
            var control = this.ViewDelegate;

            if (this.IsHitTestVisible && (null != control))
            {
                var motionEvent = e.Event;
                var modifiers = ToKeyModifiers(motionEvent.MetaState);

                switch (motionEvent.ActionMasked)
                {
                    case MotionEventActions.Down:
                    case MotionEventActions.PointerDown:
                        {
                            int index = motionEvent.ActionIndex;
                            int pointerId = motionEvent.GetPointerId(index);
                            this._pointers.Add(pointerId);
                            result = control.DispatchPointerEvent(this.ToPointerEvent(motionEvent, index, PointerInputState.Began, true, modifiers));
                            if (!result)
                            {
                                this._pointers.Remove(pointerId);
                            }
                        }

                        break;

                    case MotionEventActions.Move:
                        // A move carries every pointer's position
                        for (int index = 0; index < motionEvent.PointerCount; index += 1)
                        {
                            if (this._pointers.Contains(motionEvent.GetPointerId(index)))
                            {
                                result |= control.DispatchPointerEvent(this.ToPointerEvent(motionEvent, index, PointerInputState.Moved, true, modifiers));
                            }
                        }

                        // A handled move means a gesture (such as a swipe) is under
                        // way; stop scrolling parents from taking the pointer over
                        if (result)
                        {
                            this.CapturePointer();
                        }

                        break;

                    case MotionEventActions.Up:
                    case MotionEventActions.PointerUp:
                        {
                            int index = motionEvent.ActionIndex;
                            int pointerId = motionEvent.GetPointerId(index);
                            if (this._pointers.Remove(pointerId))
                            {
                                result = control.DispatchPointerEvent(this.ToPointerEvent(motionEvent, index, PointerInputState.Ended, true, modifiers));
                            }

                            if (0 == this._pointers.Count)
                            {
                                this.ReleasePointer();
                            }
                        }

                        break;

                    case MotionEventActions.Cancel:
                        for (int index = 0; index < motionEvent.PointerCount; index += 1)
                        {
                            if (this._pointers.Remove(motionEvent.GetPointerId(index)))
                            {
                                control.DispatchPointerEvent(this.ToPointerEvent(motionEvent, index, PointerInputState.Canceled, true, modifiers));
                            }
                        }

                        this._pointers.Clear();
                        this.ReleasePointer();
                        break;

                    case MotionEventActions.HoverMove:
                        result = control.DispatchPointerEvent(this.ToPointerEvent(motionEvent, 0, PointerInputState.Moved, false, modifiers));
                        break;
                }
            }

            e.Handled = result;
        }

        private PointerEventArgs ToPointerEvent(
            MotionEvent motionEvent,
            int index,
            PointerInputState state,
            bool isInContact,
            KeyModifiers modifiers)
        {
            var position = new Point(
                this.PixelsToDevicePixels(motionEvent.GetX(index)),
                this.PixelsToDevicePixels(motionEvent.GetY(index)));

            return new PointerEventArgs(
                motionEvent.GetPointerId(index),
                ToDeviceType(motionEvent.GetToolType(index)),
                position,
                state,
                isInContact,
                modifiers);
        }

        private static PointerDeviceType ToDeviceType(
            MotionEventToolType toolType)
        {
            switch (toolType)
            {
                case MotionEventToolType.Mouse:
                    return PointerDeviceType.Mouse;

                case MotionEventToolType.Stylus:
                case MotionEventToolType.Eraser:
                    return PointerDeviceType.Pen;

                default:
                    return PointerDeviceType.Touch;
            }
        }

        internal static KeyModifiers ToKeyModifiers(
            MetaKeyStates metaState)
        {
            var modifiers = KeyModifiers.None;
            if (metaState.HasFlag(MetaKeyStates.ShiftOn)) modifiers |= KeyModifiers.Shift;
            if (metaState.HasFlag(MetaKeyStates.CtrlOn)) modifiers |= KeyModifiers.Control;
            if (metaState.HasFlag(MetaKeyStates.AltOn)) modifiers |= KeyModifiers.Alt;
            if (metaState.HasFlag(MetaKeyStates.MetaOn)) modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        private void Element_GenericMotion(
            object sender,
            AndroidView.GenericMotionEventArgs e)
        {
            var motionEvent = e.Event;
            if ((motionEvent.Action != MotionEventActions.Scroll) || (null == this.ViewDelegate))
            {
                e.Handled = false;
                return;
            }

            // One notch is 1.0; scroll the same distance per notch as on Win32
            float delta = motionEvent.GetAxisValue(Axis.Vscroll) * 48.0f;
            var position = new Point(this.PixelsToDevicePixels(motionEvent.GetX()), this.PixelsToDevicePixels(motionEvent.GetY()));
            var pointerEvent = new PointerEventArgs(0, PointerDeviceType.Mouse, position, PointerInputState.Moved, false, ToKeyModifiers(motionEvent.MetaState), delta);
            e.Handled = this.ViewDelegate.DispatchPointerWheel(pointerEvent);
        }

        private void Element_FocusChange(
            object sender,
            AndroidView.FocusChangeEventArgs e)
        {
            if (this.Element is View view)
            {
                view.IsFocused = e.HasFocus;
            }
        }

        private void Element_KeyPress(
            object sender,
            AndroidView.KeyEventArgs e)
        {
            e.Handled = false;

            if (!(this.Element is View view))
            {
                return;
            }

            var key = ToKey(e.KeyCode);
            if (key == Key.None)
            {
                return;
            }

            var keyEvent = new KeyEventArgs(key, ToKeyModifiers(e.Event.MetaState));
            if (e.Event.Action == KeyEventActions.Down)
            {
                e.Handled = view.RouteKeyDown(keyEvent);
            }
            else if (e.Event.Action == KeyEventActions.Up)
            {
                e.Handled = view.RouteKeyUp(keyEvent);
            }
        }

        internal static Key ToKey(
            Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.Tab: return Key.Tab;
                case Keycode.Enter:
                case Keycode.NumpadEnter:
                case Keycode.DpadCenter: return Key.Enter;
                case Keycode.Space: return Key.Space;
                case Keycode.Escape: return Key.Escape;
                case Keycode.DpadLeft: return Key.Left;
                case Keycode.DpadUp: return Key.Up;
                case Keycode.DpadRight: return Key.Right;
                case Keycode.DpadDown: return Key.Down;
                case Keycode.MoveHome: return Key.Home;
                case Keycode.MoveEnd: return Key.End;
                case Keycode.PageUp: return Key.PageUp;
                case Keycode.PageDown: return Key.PageDown;
                case Keycode.Del: return Key.Back;
                case Keycode.ForwardDel: return Key.Delete;
                case Keycode.Insert: return Key.Insert;
                case Keycode.F1: return Key.F1;
                case Keycode.F2: return Key.F2;
                case Keycode.F3: return Key.F3;
                case Keycode.F4: return Key.F4;
                case Keycode.F5: return Key.F5;
                case Keycode.F6: return Key.F6;
                case Keycode.F7: return Key.F7;
                case Keycode.F8: return Key.F8;
                case Keycode.F9: return Key.F9;
                case Keycode.F10: return Key.F10;
                case Keycode.F11: return Key.F11;
                case Keycode.F12: return Key.F12;
                default:
                    if ((keyCode >= Keycode.A) && (keyCode <= Keycode.Z))
                    {
                        return Key.A + (keyCode - Keycode.A);
                    }

                    if ((keyCode >= Keycode.Num0) && (keyCode <= Keycode.Num9))
                    {
                        return Key.D0 + (keyCode - Keycode.Num0);
                    }

                    return Key.None;
            }
        }

        private void CapturePointer()
        {
            this.NativeElement?.Parent?.RequestDisallowInterceptTouchEvent(true);
        }

        private void ReleasePointer()
        {
            this.NativeElement?.Parent?.RequestDisallowInterceptTouchEvent(false);
        }

        public virtual bool IsHitTestVisible
        {
            get
            {
                return this._nativeView?.Enabled ?? false;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Enabled = true;
                }
            }
        }
    }
}
