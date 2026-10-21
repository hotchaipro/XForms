using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using XForms.Web.Renderers;

namespace XForms.Web
{
    /// <summary>
    /// The page that hosts the app: it lays the root out to the viewport,
    /// draws the layers that changed once per animation frame, runs work for
    /// the UI thread (the browser's event loop), and routes input to views.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class WebHost
    {
        private readonly Dictionary<int, ViewRenderer> _renderers = new Dictionary<int, ViewRenderer>();
        private readonly Queue<Action> _posted = new Queue<Action>();
        private readonly HashSet<ViewRenderer> _dirtyRenderers = new HashSet<ViewRenderer>();
        private List<Action<TimeSpan>> _frameCallbacks = new List<Action<TimeSpan>>();
        private ViewRenderer _root;
        private bool _isPostPending;
        private bool _isFramePending;
        private bool _isLayoutPending;
        private bool _isUpdating;

        public WebHost()
        {
            Current = this;
            this.ReadViewport();
            Js.ListenVisibility();
            Js.ListenKeyboard();
            Js.ListenHistory();
            View.AccessibilityStateChanged += this.OnAccessibilityStateChanged;
        }

        public static WebHost Current
        {
            get;
            private set;
        }

        /// <summary>
        /// The viewport's size, in CSS pixels (XForms' device-independent
        /// pixels).
        /// </summary>
        public Size ViewportSize
        {
            get;
            private set;
        }

        public float DevicePixelRatio
        {
            get;
            private set;
        } = 1;

        public event Action ThemeChanged;

        public event Action ViewportChanged;

        /// <summary>
        /// Raised when the page is hidden (another tab, or closing), when the
        /// app should save its state.
        /// </summary>
        public event Action Suspending;

        public event Action Resuming;

        private bool _isHidden;

        private void ReadViewport()
        {
            var viewport = Js.Viewport();
            this.ViewportSize = new Size((float)viewport[0], (float)viewport[1]);
            this.DevicePixelRatio = (float)viewport[2];
        }

        #region Renderers

        public void Register(
            ViewRenderer renderer)
        {
            this._renderers[renderer.Id] = renderer;
        }

        /// <summary>
        /// Routes events for another element (such as a text box's input) to
        /// the renderer.
        /// </summary>
        public void RegisterAlias(
            int id,
            ViewRenderer renderer)
        {
            this._renderers[id] = renderer;
        }

        public void UnregisterAlias(
            int id)
        {
            this._renderers.Remove(id);
        }

        public void Unregister(
            ViewRenderer renderer)
        {
            this._renderers.Remove(renderer.Id);
            this._dirtyRenderers.Remove(renderer);
        }

        public void SetRoot(
            ViewRenderer root)
        {
            this._root = root;
            Js.SetRoot(root.Id);
            this.RequestLayout();
        }

        /// <summary>
        /// Runs an event for a renderer's element, if it's still there.
        /// </summary>
        public void OnElementEvent(
            int id,
            Action<ViewRenderer> action)
        {
            if (this._renderers.TryGetValue(id, out var renderer))
            {
                Run(() => action(renderer));
            }
        }

        // Popups' backdrops, by element id: a press on one dismisses its popup
        private readonly Dictionary<int, Action> _backdrops = new Dictionary<int, Action>();

        public void RegisterBackdrop(
            int id,
            Action onPressed)
        {
            this._backdrops[id] = onPressed;
        }

        public void UnregisterBackdrop(
            int id)
        {
            this._backdrops.Remove(id);
        }

        public void OnBackdropPressed(
            int id)
        {
            if (this._backdrops.TryGetValue(id, out var onPressed))
            {
                Run(onPressed);
            }
        }

        #endregion Renderers

        #region Frames

        public void RequestLayout()
        {
            this._isLayoutPending = true;
            this.ScheduleFrame();
        }

        public void RequestRender(
            ViewRenderer renderer)
        {
            this._dirtyRenderers.Add(renderer);
            this.ScheduleFrame();
        }

        /// <summary>
        /// Calls back once, before the browser shows its next frame.
        /// </summary>
        public void RequestFrame(
            Action<TimeSpan> onFrame)
        {
            this._frameCallbacks.Add(onFrame);
            this.ScheduleFrame();
        }

        private void ScheduleFrame()
        {
            if (!this._isFramePending)
            {
                this._isFramePending = true;
                Js.RequestFrame();
            }
        }

        public void OnFrame(
            double time)
        {
            this._isFramePending = false;

            var callbacks = this._frameCallbacks;
            this._frameCallbacks = new List<Action<TimeSpan>>();
            foreach (var callback in callbacks)
            {
                Run(() => callback(TimeSpan.FromMilliseconds(time)));
            }

            this.Update();
        }

        /// <summary>
        /// Lays out and draws now (for work that needs sizes right away).
        /// </summary>
        public void Flush()
        {
            if (!this._isUpdating)
            {
                this.Update();
            }
        }

        private void Update()
        {
            this._isUpdating = true;
            try
            {
                if (this._isLayoutPending && (null != this._root))
                {
                    this._isLayoutPending = false;

                    var size = this.ViewportSize;
                    var root = (View)this._root.Element;
                    Run(() =>
                    {
                        root.Measure(size);
                        root.Arrange(new Rect(0, 0, size.Width, size.Height));
                    });
                }

                // Drawing can invalidate more (text measured for the first time)
                for (int pass = 0; (pass < 3) && (this._dirtyRenderers.Count > 0); pass += 1)
                {
                    var dirty = new List<ViewRenderer>(this._dirtyRenderers);
                    this._dirtyRenderers.Clear();
                    foreach (var renderer in dirty)
                    {
                        Run(renderer.Render);
                    }
                }
            }
            finally
            {
                this._isUpdating = false;
            }
        }

        #endregion Frames

        #region UI thread

        /// <summary>
        /// Runs the action on the UI thread, after what's running now.
        /// </summary>
        public void Post(
            Action action)
        {
            this._posted.Enqueue(action);
            if (!this._isPostPending)
            {
                this._isPostPending = true;
                Js.Post();
            }
        }

        public void RunPosted()
        {

            this._isPostPending = false;
            int count = this._posted.Count;
            for (int i = 0; (i < count) && (this._posted.Count > 0); i += 1)
            {
                Run(this._posted.Dequeue());
            }

            if ((0 < this._posted.Count) && !this._isPostPending)
            {
                this._isPostPending = true;
                Js.Post();
            }
        }

        // An exception must not unwind into the browser's event loop
        private static void Run(
            Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"XForms: {ex}");
            }
        }

        public sealed class WebSynchronizationContext : SynchronizationContext
        {
            private readonly WebHost _host;

            public WebSynchronizationContext(
                WebHost host)
            {
                this._host = host;
            }

            public override void Post(
                SendOrPostCallback d,
                object state)
            {
                this._host.Post(() => d(state));
            }

            public override void Send(
                SendOrPostCallback d,
                object state)
            {
                d(state);
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }

        #endregion UI thread

        #region Page events

        public void OnResize()
        {
            this.ReadViewport();
            this.ViewportChanged?.Invoke();
            this.RequestLayout();
        }

        public void OnVisibilityChanged(
            bool isHidden)
        {
            if (isHidden != this._isHidden)
            {
                this._isHidden = isHidden;
                Run(() => (isHidden ? this.Suspending : this.Resuming)?.Invoke());
            }
        }

        public void OnThemeChanged()
        {
            Run(() => this.ThemeChanged?.Invoke());
        }

        #endregion Page events

        #region Input

        private static PointerInputState ToState(
            int phase)
        {
            switch (phase)
            {
                case 1: return PointerInputState.Began;
                case 3: return PointerInputState.Ended;
                case 4: return PointerInputState.Canceled;
                default: return PointerInputState.Moved;
            }
        }

        public bool OnPointer(
            int id,
            int phase,
            int pointerId,
            int deviceType,
            float x,
            float y,
            bool isInContact,
            int modifiers)
        {
            if (!this._renderers.TryGetValue(id, out var renderer) || !renderer.IsHitTestVisible || !(renderer.Element is IViewDelegate view))
            {
                return false;
            }

            bool result = false;
            var e = new PointerEventArgs(pointerId, (PointerDeviceType)deviceType, new Point(x, y), ToState(phase), isInContact, (KeyModifiers)modifiers);
            Run(() => result = view.DispatchPointerEvent(e));
            return result;
        }

        public void OnHover(
            int id,
            int pointerId,
            int deviceType,
            float x,
            float y,
            int modifiers)
        {
            if (!this._renderers.TryGetValue(id, out var renderer) || !(renderer.Element is View view) || (null == Application.Current))
            {
                return;
            }

            Run(() =>
            {
                // The deepest view under it, lightweight ones included
                var point = new Point(x, y);
                var deepest = view.HitTestPointerOver(point);
                var offset = (deepest == view) ? default : deepest.OffsetInLayer;
                Application.Current.UpdatePointerOver(deepest, new PointerEventArgs(pointerId, (PointerDeviceType)deviceType, new Point(x - offset.X, y - offset.Y), PointerInputState.Moved, false, (KeyModifiers)modifiers));

                // Lightweight views see hover moves too
                ((IViewDelegate)view).DispatchPointerEvent(new PointerEventArgs(pointerId, (PointerDeviceType)deviceType, point, PointerInputState.Moved, false, (KeyModifiers)modifiers));
            });
        }

        public void OnPointerLeave(
            int pointerId)
        {
            Run(() => Application.Current?.UpdatePointerOver(null, new PointerEventArgs(pointerId, PointerDeviceType.Mouse, default, PointerInputState.Moved, false, KeyModifiers.None)));
        }

        public bool OnWheel(
            int id,
            float x,
            float y,
            float delta,
            int modifiers)
        {
            if (!this._renderers.TryGetValue(id, out var renderer) || !(renderer.Element is IViewDelegate view))
            {
                return false;
            }

            // One notch scrolls as far as on the other platforms
            bool result = false;
            var e = new PointerEventArgs(0, PointerDeviceType.Mouse, new Point(x, y), PointerInputState.Moved, false, (KeyModifiers)modifiers, delta * 48);
            Run(() => result = view.DispatchPointerWheel(e));
            return result;
        }

        /// <summary>
        /// A key (by its KeyboardEvent.key name) for the focused element, or
        /// the root when nothing is focused. Returns whether it was used.
        /// </summary>
        public bool OnKey(
            int id,
            string name,
            int modifiers,
            bool isDown)
        {
            var key = ToKey(name);
            if (key == Key.None)
            {
                return false;
            }

            ViewRenderer renderer = null;
            if ((0 != id) && !this._renderers.TryGetValue(id, out renderer))
            {
                return false;
            }

            var view = (renderer ?? this._root)?.Element as View;
            if (null == view)
            {
                return false;
            }

            bool handled = false;
            var e = new KeyEventArgs(key, (KeyModifiers)modifiers);
            Run(() =>
            {
                if (null == renderer)
                {
                    // Nothing focused: Tab starts at the first focusable view
                    handled = isDown && (key == Key.Tab) && view.MoveFocus(!e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                    return;
                }

                handled = isDown ? view.RouteLayerKeyDown(e) : view.RouteLayerKeyUp(e);

                // Escape goes back, as on the desktop
                var application = Application.Current;
                if (!handled && isDown && (key == Key.Escape) && (null != application) && application.CanGoBack)
                {
                    application.OnBackRequested();
                    handled = true;
                }
            });

            return handled;
        }

        private static Key ToKey(
            string name)
        {
            switch (name)
            {
                case "Tab": return Key.Tab;
                case "Enter": return Key.Enter;
                case " ": return Key.Space;
                case "Escape": return Key.Escape;
                case "ArrowLeft": return Key.Left;
                case "ArrowUp": return Key.Up;
                case "ArrowRight": return Key.Right;
                case "ArrowDown": return Key.Down;
                case "Home": return Key.Home;
                case "End": return Key.End;
                case "PageUp": return Key.PageUp;
                case "PageDown": return Key.PageDown;
                case "Backspace": return Key.Back;
                case "Delete": return Key.Delete;
                case "Insert": return Key.Insert;
            }

            if ((name?.Length == 1) && char.IsLetter(name[0]) && (name[0] < 128))
            {
                return Key.A + (char.ToUpperInvariant(name[0]) - 'A');
            }

            if ((name?.Length == 1) && char.IsDigit(name[0]))
            {
                return Key.D0 + (name[0] - '0');
            }

            // F1 to F12
            return ((name?.Length >= 2) && (name[0] == 'F') && Enum.TryParse(name, out Key function)) ? function : Key.None;
        }

        // History: one entry stands for the app's back stack
        private bool _hasHistoryEntry;

        /// <summary>
        /// Adds a history entry once the app can go back, so the browser's Back
        /// goes back in the app, and takes it away once the app can't.
        /// </summary>
        public void SyncHistory()
        {
            var application = Application.Current;
            bool canGoBack = (null != application) && application.CanGoBack;
            if (canGoBack && !this._hasHistoryEntry)
            {
                this._hasHistoryEntry = true;
                Js.PushHistory();
            }
            else if (!canGoBack && this._hasHistoryEntry)
            {
                this._hasHistoryEntry = false;
                Js.PopHistory();
            }
        }

        public void OnHistoryBack()
        {
            this._hasHistoryEntry = false;
            Run(() =>
            {
                var application = Application.Current;
                if ((null != application) && application.CanGoBack)
                {
                    application.OnBackRequested();
                }
            });
        }

        // Accessible elements standing for lightweight views, by element id
        private readonly Dictionary<int, View> _accessibleElements = new Dictionary<int, View>();

        public void RegisterAccessibleElement(
            int id,
            View view)
        {
            this._accessibleElements[id] = view;
        }

        public void UnregisterAccessibleElement(
            int id)
        {
            this._accessibleElements.Remove(id);
        }

        public void OnAccessibilityClick(
            int id)
        {
            if (this._accessibleElements.TryGetValue(id, out var view) && view.IsAccessibilityActionable)
            {
                Run(() => view.PerformDefaultAccessibilityAction());
            }
        }

        // A view's state changed: its element's attributes, or its layer's
        // accessible elements
        private void OnAccessibilityStateChanged(
            View view)
        {
            Run(() =>
            {
                if (view.IsLightweight)
                {
                    (view.FindLayer() as ViewRenderer)?.InvalidateRender();
                }
                else if (view.Renderer is ViewRenderer renderer)
                {
                    renderer.ApplyAccessibility();
                }
            });
        }

        #endregion Input
    }
}
