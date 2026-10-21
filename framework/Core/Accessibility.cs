using System;

namespace XForms
{
    /// <summary>
    /// What a view is to assistive technology such as a screen reader. Each
    /// platform maps these onto its own accessibility roles.
    /// </summary>
    public enum AccessibilityRole
    {
        /// <summary>
        /// Not exposed itself; its accessible descendants are.
        /// </summary>
        None = 0,

        Text,
        Button,
        ToggleButton,
        TabItem,
        List,
        ListItem,
        Image,
        TextBox,
        Group,
        RadioButton,

        /// <summary>
        /// A field that shows a value and opens a picker for it.
        /// </summary>
        ComboBox,

        /// <summary>
        /// Text that heads a section, which screen readers can jump between.
        /// </summary>
        Heading,
    }

    /// <summary>
    /// What assistive technology can ask a view to do.
    /// </summary>
    public enum AccessibilityAction
    {
        /// <summary>
        /// Does what a tap would.
        /// </summary>
        Invoke,

        /// <summary>
        /// Checks or unchecks a view with a checked state.
        /// </summary>
        Toggle,

        /// <summary>
        /// Selects a list item, tab or radio button.
        /// </summary>
        Select,

        /// <summary>
        /// Opens a view with an expanded state, such as a picker's popup.
        /// </summary>
        Expand,

        /// <summary>
        /// Closes a view with an expanded state.
        /// </summary>
        Collapse,
    }
}
