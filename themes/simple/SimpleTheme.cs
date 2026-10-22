using System;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    /// <summary>
    /// The Simple theme: XForms.Controls' plain, flat look. Choose it in
    /// <c>Application.CreateTheme</c>:
    /// <code>
    /// protected override Theme CreateTheme(ThemeMode mode, Color accentColor)
    /// {
    ///     return SimpleTheme.Apply(base.CreateTheme(mode, accentColor));
    /// }
    /// </code>
    /// </summary>
    public static class SimpleTheme
    {
        /// <summary>
        /// The theme (with its mode and colors, which the framework builds,
        /// the system's high-contrast colors included) with the Simple type
        /// scale, shapes, motion and control templates. Tokens the theme
        /// already has (an app's own) are kept.
        /// </summary>
        public static Theme Apply(
            Theme theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            theme = WithDefault(theme, new Typography());
            theme = WithDefault(theme, new Shapes());
            theme = WithDefault(theme, new Motion());
            theme = WithDefault(theme, Templates);
            return theme;
        }

        // The Simple look, for each control
        private static readonly ControlTemplates Templates = ControlTemplates.Empty
            .With(new ButtonTemplate())
            .With(new ToggleButtonTemplate())
            .With(new RadioButtonTemplate())
            .With(new ImageToggleButtonTemplate())
            .With(new MenuButtonTemplate())
            .With(new TabStripItemTemplate())
            .With(new FloatingButtonTemplate())
            .With(new CommandBarButtonTemplate())
            .With(new DayOfWeekToggleButtonTemplate())
            .With(new ListViewItemTemplate())
            .With(new ListViewSwipeItemTemplate())
            .With(new ComboBoxTemplate())
            .With(new CalendarDatePickerTemplate())
            .With(new PopupFrameTemplate())
            .With(new ComboBoxItemsViewTemplate())
            .With(new CalendarViewTemplate())
            .With(new ProgressRingTemplate());

        private static Theme WithDefault<TExtension>(
            Theme theme,
            TExtension extension)
            where TExtension : class
        {
            return (null == theme.GetExtension<TExtension>()) ? theme.WithExtension(extension) : theme;
        }
    }
}
