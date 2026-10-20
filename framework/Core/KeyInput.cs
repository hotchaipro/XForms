using System;

namespace XForms
{
    /// <summary>
    /// Keys that views can handle. Text typed into text boxes goes to the native
    /// editor and isn't reported as keys.
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
        Back,
        Delete,
        Insert,
        F1,
        F2,
        F3,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        A, B, C, D, E, F, G, H, I, J, K, L, M,
        N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    }

    /// <summary>
    /// A key pressed or released while a view (or one of its descendants) has
    /// keyboard focus.
    /// </summary>
    public sealed class KeyEventArgs : EventArgs
    {
        public KeyEventArgs(
            Key key,
            bool isShiftDown,
            bool isAltDown)
            : this(key, (isShiftDown ? KeyModifiers.Shift : KeyModifiers.None) | (isAltDown ? KeyModifiers.Alt : KeyModifiers.None))
        {
        }

        public KeyEventArgs(
            Key key,
            KeyModifiers keyModifiers)
        {
            this.Key = key;
            this.KeyModifiers = keyModifiers;
        }

        public Key Key
        {
            get;
        }

        /// <summary>
        /// The modifier keys held down.
        /// </summary>
        public KeyModifiers KeyModifiers
        {
            get;
        }

        public bool IsShiftDown
        {
            get
            {
                return this.KeyModifiers.HasFlag(KeyModifiers.Shift);
            }
        }

        public bool IsAltDown
        {
            get
            {
                return this.KeyModifiers.HasFlag(KeyModifiers.Alt);
            }
        }

        /// <summary>
        /// The view with keyboard focus when the key was pressed.
        /// </summary>
        public View OriginalSource
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set to true to stop the key going to the view's ancestors.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}
