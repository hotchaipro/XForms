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
    }
}
