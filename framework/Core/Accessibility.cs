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

        /// <summary>
        /// A box that's checked, unchecked or (when
        /// <see cref="View.AccessibilityIsMixed"/>) neither.
        /// </summary>
        CheckBox,

        /// <summary>
        /// An on and off switch.
        /// </summary>
        Switch,

        /// <summary>
        /// A value the user picks from a range
        /// (<see cref="View.AccessibilityRange"/>).
        /// </summary>
        Slider,

        /// <summary>
        /// How far a task has got: a range, or none while it's indeterminate.
        /// </summary>
        ProgressBar,

        /// <summary>
        /// A number the user types, or steps up and down.
        /// </summary>
        SpinButton,

        /// <summary>
        /// A short description shown beside a view.
        /// </summary>
        ToolTip,

        /// <summary>
        /// A message about the app's state, read as it appears.
        /// </summary>
        Alert,

        /// <summary>
        /// A list of commands.
        /// </summary>
        Menu,

        /// <summary>
        /// A command in a menu.
        /// </summary>
        MenuItem,

        /// <summary>
        /// A hierarchy of items that expand and collapse.
        /// </summary>
        Tree,

        /// <summary>
        /// An item in a tree.
        /// </summary>
        TreeItem,
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

        /// <summary>
        /// Steps a range's value up.
        /// </summary>
        Increment,

        /// <summary>
        /// Steps a range's value down.
        /// </summary>
        Decrement,
    }

    /// <summary>
    /// A view's value within a range, for assistive technology: a slider's,
    /// a progress bar's or a number box's.
    /// </summary>
    public readonly record struct AccessibilityRange(
        double Minimum,
        double Maximum,
        double Value);
}
