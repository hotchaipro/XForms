using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// The Basic theme: XForms' controls in a plain, flat look. An app
    /// chooses it as it starts:
    /// <code>
    /// XFormsApp.Run("My app", new App(), new BasicTheme());
    /// </code>
    /// </summary>
    public sealed class BasicTheme : AppTheme
    {
        /// <summary>
        /// Basic's colors for a mode, around an accent color, which light and
        /// dark use as it is.
        /// </summary>
        public override ColorScheme CreateColors(
            ThemeMode mode,
            Color accentColor)
        {
            switch (mode)
            {
                case ThemeMode.Dark:
                    return new ColorScheme()
                    {
                        Accent = accentColor,
                        OnAccent = Theme.ContrastingColor(accentColor),
                        Background = Colors.Black,
                        Foreground = Color.FromRgb(0xcc, 0xcc, 0xcc),
                        SubtleForeground = Color.FromRgb(0x99, 0x99, 0x99),
                        Surface = Color.FromRgb(0x1c, 0x1c, 0x1c),
                        OnSurface = Color.FromRgb(0xdd, 0xdd, 0xdd),
                        Outline = Color.FromRgb(0x44, 0x44, 0x44),
                        Error = Color.FromRgb(0xff, 0x8a, 0x80),
                        OnError = Colors.Black,
                        Disabled = Color.FromRgb(0x5c, 0x5c, 0x5c),

                        // More than light's, where a light dim wouldn't show
                        Scrim = Color.FromArgb(0x80, 0, 0, 0),
                    };

                case ThemeMode.HighContrast:
                    // White on black, as Windows' "Night sky" contrast theme;
                    // platforms replace these with the system's own colors
                    return new ColorScheme()
                    {
                        Accent = Color.FromRgb(0xff, 0xff, 0x00),
                        OnAccent = Colors.Black,
                        Background = Colors.Black,
                        Foreground = Colors.White,
                        SubtleForeground = Colors.White,
                        Surface = Colors.Black,
                        OnSurface = Colors.White,
                        Outline = Colors.White,
                        Error = Color.FromRgb(0xff, 0x80, 0x80),
                        OnError = Colors.Black,
                        Disabled = Color.FromRgb(0x3f, 0xf2, 0x3f),
                        Scrim = Color.FromArgb(0x80, 0, 0, 0),
                    };

                default:
                    return new ColorScheme()
                    {
                        Accent = accentColor,
                        OnAccent = Theme.ContrastingColor(accentColor),
                        Background = Colors.White,
                        Foreground = Colors.Black,
                        SubtleForeground = Color.FromRgb(0x66, 0x66, 0x66),
                        Surface = Color.FromRgb(0xf4, 0xf4, 0xf4),
                        OnSurface = Colors.Black,
                        Outline = Color.FromRgb(0xcc, 0xcc, 0xcc),
                        Error = Color.FromRgb(0xc6, 0x28, 0x28),
                        OnError = Colors.White,
                        Disabled = Color.FromRgb(0xa0, 0xa0, 0xa0),
                        Scrim = Color.FromArgb(0x40, 0, 0, 0),
                    };
            }
        }

        /// <summary>
        /// The theme (with its mode and colors, the system's high-contrast
        /// colors included) with the Basic type
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

        // The Basic look, for each control
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
