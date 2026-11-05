using System;

namespace XForma.Input
{
    /// <summary>
    /// Where a swipe is: begun (the finger has moved far enough sideways),
    /// moving, released, or canceled.
    /// </summary>
    public enum SwipePhase
    {
        Began,
        Moved,
        Ended,
        Canceled,
    }

    /// <summary>
    /// A sideways swipe as it moves, for something that follows the finger
    /// (such as <see cref="Controls.TabStrip.FollowSwipe"/>).
    /// </summary>
    public sealed class SwipingEventArgs : EventArgs
    {
        public SwipingEventArgs(
            SwipePhase phase,
            int direction,
            float distance,
            float velocity)
        {
            this.Phase = phase;
            this.Direction = direction;
            this.Distance = distance;
            this.Velocity = velocity;
        }

        public SwipePhase Phase { get; }

        /// <summary>
        /// Which way the swipe started: 1 to the right, -1 to the left.
        /// </summary>
        public int Direction { get; }

        /// <summary>
        /// How far the finger has moved that way from where it pressed (0 if
        /// it's gone back past it).
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// How fast the finger was moving that way as it was released, in
        /// points a second (negative going back); 0 until then.
        /// </summary>
        public float Velocity { get; }
    }
}
