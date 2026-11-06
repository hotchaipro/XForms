using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    /// <summary>
    /// What a drop does with the data.
    /// </summary>
    public enum DragOperation
    {
        None,
        Copy,
        Move,
    }

    /// <summary>
    /// A file dropped on the app from another one.
    /// </summary>
    public abstract class DroppedFile
    {
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Opens the file's content. Read it during or soon after the drop: the
        /// system can take the app's access back.
        /// </summary>
        public abstract Task<Stream> OpenReadAsync();
    }

    /// <summary>
    /// What's being dragged. Text and a URI go to other apps too;
    /// <see cref="LocalData"/> stays in the app, and files only come in from other
    /// apps.
    /// </summary>
    public sealed class DataPackage
    {
        public string Text
        {
            get;
            set;
        }

        public Uri Uri
        {
            get;
            set;
        }

        /// <summary>
        /// Any object, for a drop in the same app.
        /// </summary>
        public object LocalData
        {
            get;
            set;
        }

        /// <summary>
        /// The files dropped from another app. Empty while the drag is still
        /// moving on platforms that show them only at the drop.
        /// </summary>
        public IReadOnlyList<DroppedFile> Files
        {
            get;
            set;
        } = Array.Empty<DroppedFile>();
    }

    /// <summary>
    /// A drag is starting from a view: fill in <see cref="Data"/>, or set
    /// <see cref="Cancel"/>.
    /// </summary>
    public sealed class DragStartingEventArgs : EventArgs
    {
        public DataPackage Data
        {
            get;
        } = new DataPackage();

        public bool Cancel
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A drag from a view has ended.
    /// </summary>
    public sealed class DropCompletedEventArgs : EventArgs
    {
        internal DropCompletedEventArgs(
            DragOperation operation)
        {
            this.Operation = operation;
        }

        /// <summary>
        /// What the drop did, or <see cref="DragOperation.None"/> if nothing
        /// took it: the source removes the data after a
        /// <see cref="DragOperation.Move"/>.
        /// </summary>
        public DragOperation Operation
        {
            get;
        }
    }

    /// <summary>
    /// A drag over a view that allows drops. A handler of
    /// <see cref="View.DragEnter"/> or <see cref="View.DragOver"/> sets
    /// <see cref="AcceptedOperation"/> to take the drop.
    /// </summary>
    public sealed class DragEventArgs : EventArgs
    {
        internal DragEventArgs(
            DataPackage data,
            Point position,
            DragOperation acceptedOperation,
            bool isFromThisApp)
        {
            this.Data = data;
            this.Position = position;
            this.AcceptedOperation = acceptedOperation;
            this.IsFromThisApp = isFromThisApp;
        }

        public DataPackage Data
        {
            get;
        }

        /// <summary>
        /// Where the drag is, in the view's coordinates.
        /// </summary>
        public Point Position
        {
            get;
        }

        public DragOperation AcceptedOperation
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the drag started in this app.
        /// </summary>
        public bool IsFromThisApp
        {
            get;
        }
    }

    public partial class View
    {
        private bool _canDrag;
        private DragSourceRecognizer _dragRecognizer;

        /// <summary>
        /// Whether the user can drag the view: by moving a mouse over it while
        /// pressed, by holding a finger on it, or from the keyboard, with
        /// Ctrl+Space while it has focus. Handle <see cref="DragStarting"/> to
        /// say what's dragged.
        /// </summary>
        public bool CanDrag
        {
            get
            {
                return this._canDrag;
            }

            set
            {
                if (value == this._canDrag)
                {
                    return;
                }

                this._canDrag = value;
                if (value)
                {
                    this._dragRecognizer ??= new DragSourceRecognizer();
                    this.GestureRecognizers.Insert(0, this._dragRecognizer);
                }
                else
                {
                    this.GestureRecognizers.Remove(this._dragRecognizer);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether drags can drop on the view (and the views inside it that don't
        /// allow drops themselves).
        /// </summary>
        public bool AllowDrop
        {
            get;
            set;
        }

        public event EventHandler<DragStartingEventArgs> DragStarting;

        public event EventHandler<DropCompletedEventArgs> DropCompleted;

        public event EventHandler<DragEventArgs> DragEnter;

        public event EventHandler<DragEventArgs> DragOver;

        public event EventHandler<DragEventArgs> DragLeave;

        public event EventHandler<DragEventArgs> Drop;

        internal bool RaiseDragStarting(
            DragStartingEventArgs e)
        {
            this.DragStarting?.Invoke(this, e);
            return !e.Cancel;
        }

        internal void RaiseDropCompleted(
            DragOperation operation)
        {
            this.DropCompleted?.Invoke(this, new DropCompletedEventArgs(operation));
        }

        internal void RaiseDragEnter(
            DragEventArgs e)
        {
            this.DragEnter?.Invoke(this, e);
        }

        internal void RaiseDragOver(
            DragEventArgs e)
        {
            this.DragOver?.Invoke(this, e);
        }

        internal void RaiseDragLeave(
            DragEventArgs e)
        {
            this.DragLeave?.Invoke(this, e);
        }

        internal void RaiseDrop(
            DragEventArgs e)
        {
            this.Drop?.Invoke(this, e);
        }

        // The view's place in its parent
        internal Point FrameOrigin
        {
            get
            {
                return new Point(this._frame.X, this._frame.Y);
            }
        }

        internal Size FrameSize
        {
            get
            {
                return new Size(this._frame.Width, this._frame.Height);
            }
        }
    }

    /// <summary>
    /// Starts a drag where the platform lets the app choose when (Windows,
    /// Android): a mouse moving past the slop, or a touch held still. iOS and the
    /// browser recognize the gesture themselves.
    /// </summary>
    internal sealed class DragSourceRecognizer : GestureRecognizer
    {
        private DispatcherTimer _timer;
        private Point _start;

        protected override bool IsDragGesture
        {
            get
            {
                return true;
            }
        }

        protected override void OnPointerInput(
            PointerEventArgs e)
        {
            var application = Application.Current;
            if ((null == application) || application.Renderer.UsesSystemDragGesture)
            {
                this.Fail();
                return;
            }

            switch (e.State)
            {
                case PointerInputState.Began:
                    this._start = e.Position;
                    if (e.DeviceType != PointerDeviceType.Mouse)
                    {
                        this._timer = new DispatcherTimer() { Interval = View.LongPressDuration };
                        this._timer.Tick += (sender, args) =>
                        {
                            this.StopTimer();
                            this.Start();
                        };
                        this._timer.Start();
                    }

                    break;

                case PointerInputState.Moved:
                    float dx = e.Position.X - this._start.X;
                    float dy = e.Position.Y - this._start.Y;
                    float slop = (e.DeviceType == PointerDeviceType.Mouse) ? Application.MouseDragSlop : Application.TapSlop;
                    if (((dx * dx) + (dy * dy)) >= (slop * slop))
                    {
                        if (e.DeviceType == PointerDeviceType.Mouse)
                        {
                            this.Start();
                        }
                        else
                        {
                            // A finger moving first is a scroll or a swipe
                            this.StopTimer();
                            this.Fail();
                        }
                    }

                    break;

                default:
                    this.StopTimer();
                    break;
            }
        }

        private void StopTimer()
        {
            this._timer?.Stop();
            this._timer = null;
        }

        private void Start()
        {
            var view = this.View;
            var data = (null != view) ? DragDropSession.Begin(view) : null;
            if (null == data)
            {
                this.Fail();
                return;
            }

            this.Recognize();

            // After this input's handled: the platform's drag can run its own loop
            var start = this._start;
            Application.Current.Dispatcher.Post(() => Application.Current.Renderer.StartDrag(view, data, start));
        }
    }

    /// <summary>
    /// The drag going on, wherever it came from: tells views as it moves over
    /// them, by pointer or from the keyboard.
    /// </summary>
    internal static class DragDropSession
    {
        private static View _source;
        private static DataPackage _localData;
        private static View _target;
        private static DataPackage _targetData;
        private static DragOperation _accepted;

        // A drag from the keyboard: the views it can drop on, and where it is
        private static List<View> _keyboardTargets;
        private static int _keyboardIndex;

        internal static bool IsKeyboardDrag
        {
            get
            {
                return null != _keyboardTargets;
            }
        }

        /// <summary>
        /// The view inside which a drag starts asks what's dragged, or null if
        /// nothing there can be.
        /// </summary>
        public static DataPackage Begin(
            View view)
        {
            for (; null != view; view = view.KeyboardParent)
            {
                if (view.CanDrag)
                {
                    var args = new DragStartingEventArgs();
                    if (!view.RaiseDragStarting(args))
                    {
                        return null;
                    }

                    _source = view;
                    _localData = args.Data;
                    return args.Data;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether a drag could start in the view.
        /// </summary>
        public static bool CanBegin(
            View view)
        {
            for (; null != view; view = view.KeyboardParent)
            {
                if (view.CanDrag)
                {
                    return true;
                }
            }

            return false;
        }

        // The view allowing the drop, and the point in its coordinates
        private static View FindTarget(
            View deepest,
            ref Point point)
        {
            float x = point.X, y = point.Y;
            for (var view = deepest; null != view; view = view.KeyboardParent)
            {
                if (view.AllowDrop && view.IsVisible)
                {
                    point = new Point(x, y);
                    return view;
                }

                x += view.FrameOrigin.X;
                y += view.FrameOrigin.Y;
            }

            return null;
        }

        private static void Leave()
        {
            var target = _target;
            if (null != target)
            {
                var data = _targetData;
                _target = null;
                _targetData = null;
                _accepted = DragOperation.None;
                target.RaiseDragLeave(new DragEventArgs(data, default, DragOperation.None, null != _localData));
            }
        }

        /// <summary>
        /// The drag moved over a view, at a point in its coordinates (null: it's
        /// over nothing). Returns what a drop there would do.
        /// </summary>
        public static DragOperation Update(
            View deepest,
            Point point,
            DataPackage external)
        {
            var data = _localData ?? external ?? new DataPackage();
            var target = FindTarget(deepest, ref point);
            if (target != _target)
            {
                Leave();
                _target = target;
                _targetData = data;
                _accepted = DragOperation.None;
                if (null != target)
                {
                    var enter = new DragEventArgs(data, point, DragOperation.None, null != _localData);
                    target.RaiseDragEnter(enter);
                    _accepted = enter.AcceptedOperation;
                }
            }

            if (null != target)
            {
                var over = new DragEventArgs(data, point, _accepted, null != _localData);
                target.RaiseDragOver(over);
                _accepted = over.AcceptedOperation;
            }

            return (null != target) ? _accepted : DragOperation.None;
        }

        /// <summary>
        /// The drag left the app's window.
        /// </summary>
        public static void Exit()
        {
            Leave();
        }

        /// <summary>
        /// The drag dropped on a view. Returns what the drop did.
        /// </summary>
        public static DragOperation Drop(
            View deepest,
            Point point,
            DataPackage external)
        {
            var operation = Update(deepest, point, external);
            var target = _target;
            var data = _localData ?? external ?? new DataPackage();
            _target = null;
            _targetData = null;
            _accepted = DragOperation.None;
            if ((null == target) || (operation == DragOperation.None))
            {
                target?.RaiseDragLeave(new DragEventArgs(data, default, DragOperation.None, null != _localData));
                return DragOperation.None;
            }

            FindTarget(deepest, ref point);
            var args = new DragEventArgs(data, point, operation, null != _localData);
            target.RaiseDrop(args);
            return args.AcceptedOperation;
        }

        /// <summary>
        /// A drag from this app ended (it can be heard more than once).
        /// </summary>
        public static void End(
            DragOperation operation)
        {
            Leave();
            var source = _source;
            _source = null;
            _localData = null;
            _keyboardTargets = null;
            source?.RaiseDropCompleted(operation);
        }

        // ── From the keyboard ──────────────────────────────────────────────

        private static void CollectTargets(
            View view,
            View source,
            List<View> targets)
        {
            if (!view.IsVisible)
            {
                return;
            }

            if (view.AllowDrop && (view != source))
            {
                targets.Add(view);
            }

            foreach (var child in view.TabOrderChildren)
            {
                CollectTargets(child, source, targets);
            }
        }

        private static void MoveToKeyboardTarget(
            int direction)
        {
            int count = _keyboardTargets.Count;
            for (int i = 1; i <= count; i += 1)
            {
                int index = (_keyboardIndex + (direction * i) + (count * (i + 1))) % count;
                var candidate = _keyboardTargets[index];
                var size = candidate.FrameSize;
                if (DragOperation.None != Update(candidate, new Point(size.Width / 2, size.Height / 2), null))
                {
                    _keyboardIndex = index;
                    return;
                }
            }

            _keyboardIndex = -1;
            Leave();
        }

        /// <summary>
        /// The keys of a drag: Ctrl+Space on a view that can be dragged picks it
        /// up; Tab and the arrow keys go through the views that accept it; Enter
        /// or Space drops it, and Escape puts it back. Returns whether the key was
        /// used.
        /// </summary>
        public static bool HandleKey(
            View focused,
            KeyEventArgs e)
        {
            if (!IsKeyboardDrag)
            {
                if ((e.Key != Key.Space) || !e.KeyModifiers.HasFlag(KeyModifiers.Control) || !CanBegin(focused))
                {
                    return false;
                }

                if (null == Begin(focused))
                {
                    return true;
                }

                var root = focused;
                while (root.KeyboardParent is View parent)
                {
                    root = parent;
                }

                _keyboardTargets = new List<View>();
                CollectTargets(root, _source, _keyboardTargets);
                _keyboardIndex = -1;
                if (0 == _keyboardTargets.Count)
                {
                    End(DragOperation.None);
                }
                else
                {
                    _keyboardIndex = 0;
                    _keyboardIndex = (_keyboardTargets.Count - 1);
                    MoveToKeyboardTarget(1);
                }

                return true;
            }

            switch (e.Key)
            {
                case Key.Tab:
                    MoveToKeyboardTarget(e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -1 : 1);
                    return true;

                case Key.Down:
                case Key.Right:
                    MoveToKeyboardTarget(1);
                    return true;

                case Key.Up:
                case Key.Left:
                    MoveToKeyboardTarget(-1);
                    return true;

                case Key.Enter:
                case Key.Space:
                    {
                        var operation = DragOperation.None;
                        if (_keyboardIndex >= 0)
                        {
                            var target = _keyboardTargets[_keyboardIndex];
                            var size = target.FrameSize;
                            operation = Drop(target, new Point(size.Width / 2, size.Height / 2), null);
                        }

                        End(operation);
                        return true;
                    }

                case Key.Escape:
                    End(DragOperation.None);
                    return true;

                default:
                    // Nothing else happens while a drag's going on
                    return true;
            }
        }
    }
}
