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
        /// scale, shapes, motion, control styles and templates. Tokens the theme
        /// already has (an app's own) are kept.
        /// </summary>
        public override Theme Apply(
            Theme theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            theme = WithDefault(theme, Type);
            theme = WithDefault(theme, Corners);
            theme = WithDefault(theme, Animation);
            foreach (var apply in Styles)
            {
                theme = apply(theme);
            }

            theme = WithDefault(theme, Templates);
            return theme;
        }

        private static readonly Typography Type = new Typography()
        {
            Display = new TextStyle() { FontSize = 34, FontWeight = FontWeight.Light },
            Title = new TextStyle() { FontSize = 20, FontWeight = FontWeight.SemiBold },
            Body = new TextStyle() { FontSize = 15 },
            Label = new TextStyle() { FontSize = 14 },
            Caption = new TextStyle() { FontSize = 12 },
        };

        private static readonly Shapes Corners = new Shapes()
        {
            SmallCornerRadius = 4,
            MediumCornerRadius = 8,
            LargeCornerRadius = 16,
        };

        private static readonly Motion Animation = new Motion()
        {
            Short = TimeSpan.FromMilliseconds(100),
            Medium = TimeSpan.FromMilliseconds(250),
            Long = TimeSpan.FromMilliseconds(400),
            Standard = new CubicEase(EasingMode.EaseInOut),
            Decelerate = new CubicEase(EasingMode.EaseOut),
            Accelerate = new CubicEase(EasingMode.EaseIn),
            Spring = new SpringEase(EasingMode.EaseOut),
        };

        // What each control lays out by
        private static readonly Func<Theme, Theme>[] Styles =
        {
            Style(new MenuBarStyle() { Height = 50, ItemWidth = 72, ItemSpacing = 0, ItemsAlignment = HorizontalAlignment.Left }),
            Style(new CommandBarStyle() { ButtonSpacing = 0, ButtonWidth = 48, ButtonHeight = 48, SwipeIconSize = new Size(30, 30) }),
            Style(new PopupStyle() { ContentInset = 6, ListVerticalPadding = 4, RowHeight = 36, TouchRowHeight = 48 }),
            Style(new CheckBoxStyle() { BoxSize = 20, TextGap = 8, MinimumHeight = 32 }),
            Style(new ToggleSwitchStyle() { TrackWidth = 40, TrackHeight = 20, TouchHeight = 32, TextGap = 12, HeaderGap = 4 }),
            Style(new SliderStyle() { ThumbSize = 20, TouchHeight = 32, HeaderGap = 4, DefaultWidth = 200 }),
            Style(new ProgressBarStyle() { Height = 4, DefaultWidth = 200 }),
            Style(new CalendarViewStyle() { CellSize = 40, HeaderHeight = 40, WeekdayHeight = 28, Inset = 8, HeaderInset = 8 }),
            Style(new TimePickerViewStyle() { RowHeight = 40, ColumnWidth = 72, ButtonHeight = 44 }),
            Style(new PickerFieldStyle() { TextInset = 12, GlyphWidth = 34 }),
            Style(new NumberBoxStyle() { SpinButtonWidth = 32 }),
            Style(new PasswordBoxStyle() { RevealButtonWidth = 36 }),
            Style(new MenuFlyoutStyle() { TextInset = 40, TextEndInset = 24, MinimumWidth = 160 }),
            Style(new ToolTipStyle() { MaximumWidth = 320, Padding = new Thickness(8, 5, 8, 6) }),
            Style(new InfoBarStyle() { Spacing = 12, IconSize = 20, IconOffset = 1, CloseButtonSize = 32, CloseButtonOutset = 6, DefaultWidth = 480, TitleFontWeight = FontWeight.SemiBold }),
            Style(new ExpanderStyle() { HeaderHeight = 48, HeaderChromeWidth = 72, ContentPadding = new Thickness(16, 8, 16, 16), RevealOffset = 12 }),
            Style(new TreeViewStyle() { ItemHeight = 36, Indent = 20, ChevronWidth = 28, ItemInset = 8 }),
            Style(new NavigationViewStyle() { PaneWidth = 260, BarHeight = 48, TitleHeight = 56, ItemHeight = 40, ItemMargin = new Thickness(4, 0, 4, 0), ItemSpacing = 2, DefaultSize = new Size(800, 600) }),
            Style(new TabStripStyle() { MaximumWidth = 600 }),
            Style(new RadioButtonStyle() { Width = 72, Height = 60 }),
            Style(new DaysOfWeekPickerStyle() { ButtonSize = 40, MaximumWidth = 320 }),
            Style(new FlipViewStyle() { ButtonWidth = 32, ButtonMaximumHeight = 64, DefaultSize = new Size(320, 240) }),
            Style(new PipsPagerStyle() { DotSpacing = 20, Height = 28 }),
            Style(new ListViewStyle() { GroupHeaderMargin = new Thickness(12, 12, 12, 4), GroupHeaderFontWeight = FontWeight.SemiBold }),
            Style(new FlyoutButtonStyle()
            {
                OpenRotation = Angle.FromDegrees(90),
                OpenScale = 0.75f,
                CollapsedButtonScale = 0.5f,
                ArcCenter = Angle.FromDegrees(45),
                ArcSweep = Angle.FromDegrees(90),
                EvenArcSweep = Angle.FromDegrees(60),
                ButtonClearance = 15,
                ButtonSpacing = 12,
            }),
        };

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

        private static Func<Theme, Theme> Style<TStyle>(
            TStyle style)
            where TStyle : class
        {
            return theme => WithDefault(theme, style);
        }

        private static Theme WithDefault<TExtension>(
            Theme theme,
            TExtension extension)
            where TExtension : class
        {
            return (null == theme.GetExtension<TExtension>()) ? theme.WithExtension(extension) : theme;
        }
    }
}
