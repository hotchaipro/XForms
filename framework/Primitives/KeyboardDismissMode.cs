namespace XForms.Primitives
{
    /// <summary>
    /// Whether scrolling a <see cref="ScrollViewer"/> hides the soft keyboard.
    /// </summary>
    public enum KeyboardDismissMode
    {
        /// <summary>
        /// Scrolling leaves the keyboard up.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dragging the content ends text entry, hiding the keyboard.
        /// </summary>
        OnDrag,

        /// <summary>
        /// The keyboard follows a drag down and hides if dragged off (iOS);
        /// elsewhere, as <see cref="OnDrag"/>.
        /// </summary>
        Interactive,
    }
}
