using System;

namespace XForms
{
    /// <summary>
    /// Edges of the safe area: the parts of the screen next to the system's
    /// bars, display cutout and home indicator.
    /// </summary>
    [Flags]
    public enum SafeAreaEdges
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        All = Left | Top | Right | Bottom,
    }
}
