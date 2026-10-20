using System;

namespace XForms
{
    /// <summary>
    /// Keys that controls handle themselves. Text typed into text entries goes to
    /// the native text entry and isn't reported as keys.
    /// </summary>
    public enum Key
    {
        None = 0,
        Tab,
        Enter,
        Space,
        Escape,
        Left,
        Up,
        Right,
        Down,
        Home,
        End,
        PageUp,
        PageDown,
        F4,
    }

    /// <summary>
    /// A key pressed while a view has keyboard focus.
    /// </summary>
    public sealed class KeyInputEvent
    {
        public KeyInputEvent(
            Key key,
            bool isShiftDown,
            bool isAltDown)
        {
            this.Key = key;
            this.IsShiftDown = isShiftDown;
            this.IsAltDown = isAltDown;
        }

        public Key Key
        {
            get;
        }

        public bool IsShiftDown
        {
            get;
        }

        public bool IsAltDown
        {
            get;
        }
    }
}
