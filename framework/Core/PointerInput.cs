using System;

namespace XForms
{
    public enum PointerInputState
    {
        Began = 0,
        Moved,
        Ended,
        Canceled,
    }

    public enum PointerDeviceType
    {
        Touch = 0,
        Mouse,
        Pen,
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4,
        Meta = 8,
    }

    /// <summary>
    /// A pointer (touch, mouse or pen) pressing, moving, releasing or turning
    /// a wheel over a view.
    /// </summary>
    public sealed class PointerEventArgs : EventArgs
    {
        public PointerEventArgs(
            Point position,
            PointerInputState state,
            bool isInContact)
            : this(0, PointerDeviceType.Touch, position, state, isInContact, KeyModifiers.None)
        {
        }

        public PointerEventArgs(
            int pointerId,
            PointerDeviceType deviceType,
            Point position,
            PointerInputState state,
            bool isInContact,
            KeyModifiers keyModifiers,
            float wheelDelta = 0)
        {
            this.PointerId = pointerId;
            this.DeviceType = deviceType;
            this.Position = position;
            this.State = state;
            this.IsInContact = isInContact;
            this.KeyModifiers = keyModifiers;
            this.WheelDelta = wheelDelta;
        }

        /// <summary>
        /// Identifies the pointer while it's down: each finger of a multi-touch
        /// gesture has its own.
        /// </summary>
        public int PointerId
        {
            get;
        }

        public PointerDeviceType DeviceType
        {
            get;
        }

        /// <summary>
        /// Where the pointer is, relative to the view receiving the event.
        /// </summary>
        public Point Position
        {
            get;
        }

        public PointerInputState State
        {
            get;
        }

        public bool IsInContact
        {
            get;
        }

        public bool IsHovering
        {
            get
            {
                return (this.State == PointerInputState.Moved) && !this.IsInContact;
            }
        }

        /// <summary>
        /// The modifier keys held down.
        /// </summary>
        public KeyModifiers KeyModifiers
        {
            get;
        }

        /// <summary>
        /// For a wheel event, how far the wheel turned, in device-independent
        /// pixels (positive is up, or away from the user).
        /// </summary>
        public float WheelDelta
        {
            get;
        }

        /// <summary>
        /// The view the event was first delivered to.
        /// </summary>
        public View OriginalSource
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set to true to stop the event: a handled press keeps the pointer for
        /// this view, a handled move keeps scrolling parents from taking it.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }

    public sealed class TappedEventArgs : EventArgs
    {
        public TappedEventArgs(
            Point position,
            PointerDeviceType deviceType)
        {
            this.Position = position;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// Where the tap was, relative to the view.
        /// </summary>
        public Point Position
        {
            get;
        }

        public PointerDeviceType DeviceType
        {
            get;
        }
    }

    public enum HoldingState
    {
        /// <summary>
        /// The pointer has been held down long enough.
        /// </summary>
        Started = 0,

        /// <summary>
        /// The pointer was released after holding.
        /// </summary>
        Completed,

        /// <summary>
        /// The pointer moved away or was canceled after holding.
        /// </summary>
        Canceled,
    }

    public sealed class HoldingEventArgs : EventArgs
    {
        public HoldingEventArgs(
            Point position,
            HoldingState state)
        {
            this.Position = position;
            this.HoldingState = state;
        }

        public Point Position
        {
            get;
        }

        public HoldingState HoldingState
        {
            get;
        }
    }
}
