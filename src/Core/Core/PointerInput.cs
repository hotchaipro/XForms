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

    public sealed class PointerInputEvent
    {
        public PointerInputEvent(
            Point point,
            PointerInputState state,
            bool isInContact)
        {
            this.Point = point;
            this.State = state;
            this.IsHovering = ((state == PointerInputState.Moved) && (!isInContact));
        }

        public Point Point
        {
            get;
            private set;
        }

        public PointerInputState State
        {
            get;
            private set;
        }

        public bool IsHovering
        {
            get;
            private set;
        }
    }
}
