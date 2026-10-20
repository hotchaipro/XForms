using System;

namespace XForms
{
    public enum TextAlignment
    {
        Start = 0,
        Center = 1,
        End = 2,
    }

    /// <summary>
    /// Whether text wraps onto more lines when it doesn't fit.
    /// </summary>
    public enum TextWrapping
    {
        NoWrap = 0,
        Wrap = 1,
    }

    /// <summary>
    /// What happens to text that doesn't fit.
    /// </summary>
    public enum TextTrimming
    {
        None = 0,
        CharacterEllipsis = 1,
    }
}
