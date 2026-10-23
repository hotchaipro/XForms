using System;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    /// <summary>
    /// The Simple theme: XForms' controls in a plain, flat look. An app
    /// chooses it as it starts:
    /// <code>
    /// XFormsApp.Run("My app", new App(), new SimpleTheme());
    /// </code>
    /// </summary>
    public sealed class SimpleTheme : AppTheme
    {
        /// <summary>
        /// The theme (with its mode and colors, which the framework builds,
        /// the system's high-contrast colors included) with the Simple type
        /// scale, shapes, motion and control templates. Tokens the theme
        /// already has (an app's own) are kept.
        /// </summary>
        public override Theme Apply(
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
            .With(new ProgressRingTemplate())
            .With(new MenuBarTemplate())
            .With(new TabStripTemplate())
            .With(new CommandListViewItemTemplate())
            .With(new HyperlinkButtonTemplate())
            .With(new MessageBoxTemplate())
            .With(new CheckBoxTemplate())
            .With(new ToggleSwitchTemplate())
            .With(new SliderTemplate())
            .With(new ProgressBarTemplate())
            .With(new TimePickerTemplate())
            .With(new TimePickerViewTemplate())
            .With(new NumberBoxTemplate())
            .With(new PasswordBoxTemplate())
            .With(new AutoSuggestBoxTemplate())
            .With(new AutoSuggestBoxListViewTemplate())
            .With(new ToolTipTemplate())
            .With(new MenuFlyoutViewTemplate())
            .With(new InfoBarTemplate())
            .With(new InfoBarCloseButtonTemplate())
            .With(new ExpanderTemplate())
            .With(new ExpanderHeaderTemplate())
            .With(new NavigationViewTemplate())
            .With(new NavigationViewPaneTemplate())
            .With(new NavigationViewItemTemplate())
            .With(new NavigationViewMenuButtonTemplate())
            .With(new NavigationViewScrimTemplate())
            .With(new FlipViewTemplate())
            .With(new FlipViewButtonTemplate())
            .With(new PipsPagerTemplate())
            .With(new RefreshContainerTemplate())
            .With(new TreeViewTemplate())
            .With(new TreeViewItemTemplate());

        private static Theme WithDefault<TExtension>(
            Theme theme,
            TExtension extension)
            where TExtension : class
        {
            return (null == theme.GetExtension<TExtension>()) ? theme.WithExtension(extension) : theme;
        }
    }
}
