using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using XForma.Controls;
using XForma.Platform;
using XForma.Primitives;
using XForma.Themes.Basic;
using Xunit;

namespace XForma.Tests
{
    // Where a theme's colors come from: the framework has the roles and a plain
    // fallback, a theme package has the palette, and the system's high contrast
    // colors win
    public class ThemePaletteTests
    {
        private static readonly Color Accent = Color.FromRgb(0x2196f3);

        // A palette and Basic extension where every color is its own
        private static readonly ColorScheme TestColors = new ColorScheme()
        {
            Accent = Color.FromRgb(0x01, 0x02, 0x03),
            OnAccent = Color.FromRgb(0x04, 0x05, 0x06),
            Background = Color.FromRgb(0x07, 0x08, 0x09),
            Foreground = Color.FromRgb(0x0a, 0x0b, 0x0c),
            SubtleForeground = Color.FromRgb(0x0d, 0x0e, 0x0f),
            Surface = Color.FromRgb(0x10, 0x11, 0x12),
            OnSurface = Color.FromRgb(0x13, 0x14, 0x15),
            Outline = Color.FromRgb(0x16, 0x17, 0x18),
            Error = Color.FromRgb(0x19, 0x1a, 0x1b),
            OnError = Color.FromRgb(0x1c, 0x1d, 0x1e),
            Disabled = Color.FromRgb(0x1f, 0x20, 0x21),
            Scrim = Color.FromRgb(0x22, 0x23, 0x24),
        };

        private static readonly BasicColors TestBasicColors = new BasicColors()
        {
            Success = Color.FromRgb(0x25, 0x26, 0x27),
            Warning = Color.FromRgb(0x28, 0x29, 0x2a),
            OnSuccess = Color.FromRgb(0x2b, 0x2c, 0x2d),
            OnWarning = Color.FromRgb(0x2e, 0x2f, 0x30),
            OnPressed = Color.FromRgb(0x31, 0x32, 0x33),
            OnCommand = Color.FromRgb(0x34, 0x35, 0x36),
            SubtleFloatingBackground = Color.FromRgb(0x37, 0x38, 0x39),
            FloatingPressedBackground = Color.FromRgb(0x3a, 0x3b, 0x3c),
            FloatingPressedForeground = Color.FromRgb(0x3d, 0x3e, 0x3f),
            Shadow = Color.FromRgb(0x40, 0x41, 0x42),
            Glow = Color.FromRgb(0x43, 0x44, 0x45),
        };

        private static readonly Color CommandColor = Color.FromRgb(0x46, 0x47, 0x48);

        // A theme package that replaces Basic's palette and keeps its look
        private sealed class TestTheme : AppTheme
        {
            public override ColorScheme CreateColors(
                ThemeMode mode,
                Color accentColor)
            {
                return TestColors;
            }

            public override Theme Apply(
                Theme theme)
            {
                return new BasicTheme().Apply(theme).WithExtension(TestBasicColors);
            }
        }

        // What the platform answers for the theme; the rest does nothing
        public class FakeRenderer : DispatchProxy
        {
            public ThemeMode SystemMode;
            public ColorScheme SystemHighContrast;

            protected override object Invoke(
                MethodInfo targetMethod,
                object[] args)
            {
                switch (targetMethod.Name)
                {
                    case "get_SystemThemeMode": return this.SystemMode;
                    case "get_SystemHighContrastColors": return this.SystemHighContrast;
                    case "get_SystemTextScale": return 1.0f;
                }

                return DefaultFor(targetMethod);
            }
        }

        public class FakePlatform : DispatchProxy
        {
            public object Renderer;

            protected override object Invoke(
                MethodInfo targetMethod,
                object[] args)
            {
                return (targetMethod.Name == "CreateApplicationRenderer") ? this.Renderer : DefaultFor(targetMethod);
            }
        }

        private static object DefaultFor(
            MethodInfo method)
        {
            return method.ReturnType.IsValueType && (method.ReturnType != typeof(void))
                ? Activator.CreateInstance(method.ReturnType)
                : null;
        }

        private class TestApp : Application
        {
        }

        // An accent for each mode, from the app's own CreateTheme
        private sealed class PerModeAccentApp : Application
        {
            public static readonly Color Light = Color.FromRgb(0x11, 0x11, 0x11);
            public static readonly Color Dark = Color.FromRgb(0xee, 0xee, 0xee);

            protected override Theme CreateTheme(
                ThemeMode mode,
                Color accentColor)
            {
                return base.CreateTheme(mode, (mode == ThemeMode.Light) ? Light : Dark);
            }
        }

        // A started app on a platform that shows nothing
        private static TApp Start<TApp>(
            TApp app,
            ThemeMode systemMode,
            ColorScheme systemHighContrast = null)
            where TApp : Application
        {
            var renderer = DispatchProxy.Create<IApplicationRenderer, FakeRenderer>();
            ((FakeRenderer)(object)renderer).SystemMode = systemMode;
            ((FakeRenderer)(object)renderer).SystemHighContrast = systemHighContrast;

            var platform = DispatchProxy.Create<IPlatform, FakePlatform>();
            ((FakePlatform)(object)platform).Renderer = renderer;

            // NOTE: The app has no routes, so its first navigation faults,
            // which nothing here waits for
            app.Start(platform, null);
            return app;
        }

        // The values Basic had when they were in the framework: its screens
        // stay as they were
        [Fact]
        public void BasicKeepsItsLightColors()
        {
            Assert.Equal(
                new ColorScheme()
                {
                    Accent = Accent,
                    OnAccent = Colors.White,
                    Background = Colors.White,
                    Foreground = Colors.Black,
                    SubtleForeground = Color.FromRgb(0x666666),
                    Surface = Color.FromRgb(0xf4f4f4),
                    OnSurface = Colors.Black,
                    Outline = Color.FromRgb(0xcccccc),
                    Error = Color.FromRgb(0xc62828),
                    OnError = Colors.White,
                    Disabled = Color.FromRgb(0xa0a0a0),
                    Scrim = Color.FromArgb(0x40, 0, 0, 0),
                },
                new BasicTheme().CreateColors(ThemeMode.Light, Accent));
        }

        [Fact]
        public void BasicKeepsItsDarkColors()
        {
            Assert.Equal(
                new ColorScheme()
                {
                    Accent = Accent,
                    OnAccent = Colors.White,
                    Background = Colors.Black,
                    Foreground = Color.FromRgb(0xcccccc),
                    SubtleForeground = Color.FromRgb(0x999999),
                    Surface = Color.FromRgb(0x1c1c1c),
                    OnSurface = Color.FromRgb(0xdddddd),
                    Outline = Color.FromRgb(0x444444),
                    Error = Color.FromRgb(0xff8a80),
                    OnError = Colors.Black,
                    Disabled = Color.FromRgb(0x5c5c5c),
                    Scrim = Color.FromArgb(0x80, 0, 0, 0),
                },
                new BasicTheme().CreateColors(ThemeMode.Dark, Accent));
        }

        [Fact]
        public void BasicKeepsItsHighContrastColors()
        {
            Assert.Equal(
                new ColorScheme()
                {
                    Accent = Color.FromRgb(0xffff00),
                    OnAccent = Colors.Black,
                    Background = Colors.Black,
                    Foreground = Colors.White,
                    SubtleForeground = Colors.White,
                    Surface = Colors.Black,
                    OnSurface = Colors.White,
                    Outline = Colors.White,
                    Error = Color.FromRgb(0xff8080),
                    OnError = Colors.Black,
                    Disabled = Color.FromRgb(0x3ff23f),
                    Scrim = Color.FromArgb(0x80, 0, 0, 0),
                },
                new BasicTheme().CreateColors(ThemeMode.HighContrast, Accent));
        }

        [Fact]
        public void AnAppWithNoPackageHasPlainColors()
        {
            var app = Start(new TestApp(), ThemeMode.Dark);

            Assert.Equal(ColorScheme.Plain(ThemeMode.Dark, app.ActualAccentColor), app.Theme.Colors);
        }

        [Fact]
        public void APackagesPaletteIsTheThemesColors()
        {
            var package = new TestTheme();
            var app = new TestApp() { ThemePalette = package.CreateColors, ThemeExtender = package.Apply };
            Start(app, ThemeMode.Light);

            Assert.Equal(TestColors, app.Theme.Colors);
        }

        [Fact]
        public void ThePaletteGetsTheModeAndAccent()
        {
            var app = new TestApp()
            {
                AccentColor = Color.FromRgb(0x123456),
                RequestedThemeMode = ThemeMode.Dark,
                ThemePalette = new BasicTheme().CreateColors,
            };
            Start(app, ThemeMode.Light);

            Assert.Equal(ThemeMode.Dark, app.Theme.Mode);
            Assert.Equal(Color.FromRgb(0x123456), app.Theme.Colors.Accent);
            Assert.Equal(Colors.Black, app.Theme.Colors.Background);
        }

        [Fact]
        public void TheSystemsHighContrastColorsWinOverAPackagesPalette()
        {
            var system = new ColorScheme() { Background = Color.FromRgb(0x0000aa), Foreground = Color.FromRgb(0xffff55) };
            var package = new TestTheme();
            var app = new TestApp() { ThemePalette = package.CreateColors, ThemeExtender = package.Apply };
            Start(app, ThemeMode.HighContrast, system);

            Assert.Equal(system with { Scrim = TestColors.Scrim }, app.Theme.Colors);
        }

        [Fact]
        public void APackagesHighContrastColorsAreUsedWhereTheSystemHasNone()
        {
            var package = new BasicTheme();
            var app = new TestApp() { ThemePalette = package.CreateColors };
            Start(app, ThemeMode.HighContrast);

            Assert.Equal(package.CreateColors(ThemeMode.HighContrast, app.ActualAccentColor), app.Theme.Colors);
        }

        [Fact]
        public void APackageMakesAWholeThemeForASection()
        {
            var theme = new TestTheme().Create(ThemeMode.Dark, Accent);

            Assert.Equal(ThemeMode.Dark, theme.Mode);
            Assert.Equal(TestColors, theme.Colors);
            Assert.NotNull(theme.GetTemplate(typeof(Button)));
        }

        [Fact]
        public void AnAppGivesEachModeItsOwnAccentInCreateTheme()
        {
            var app = new PerModeAccentApp() { ThemePalette = new BasicTheme().CreateColors };
            Start(app, ThemeMode.Light);
            Assert.Equal(PerModeAccentApp.Light, app.Theme.Colors.Accent);

            app.RequestedThemeMode = ThemeMode.Dark;
            Assert.Equal(PerModeAccentApp.Dark, app.Theme.Colors.Accent);
        }

        [Fact]
        public void AFlyoutDimsWithItsThemesScrim()
        {
            var scrim = typeof(Flyout).GetMethod("GetDefaultScrimColor", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.Equal(TestColors.Scrim, scrim.Invoke(new Flyout(), new object[] { new Theme() { Colors = TestColors } }));
        }

        // Basic's defaults for what has no role in a palette are what its
        // templates drew before they became settings
        [Fact]
        public void BasicKeepsItsOtherColors()
        {
            var colors = new BasicColors();

            Assert.Equal(Color.FromRgb(0x2e9e44), colors.Success);
            Assert.Equal(Color.FromRgb(0xe89e00), colors.Warning);
            Assert.Equal(Colors.White, colors.OnSuccess);
            Assert.Equal(Colors.Black, colors.OnWarning);
            Assert.Equal(Colors.White, colors.OnPressed);
            Assert.Equal(Colors.White, colors.OnCommand);
            Assert.Null(colors.SubtleFloatingBackground);
            Assert.Equal(Colors.White, colors.FloatingPressedBackground);
            Assert.Equal(Colors.Gray, colors.FloatingPressedForeground);
            Assert.Equal(Color.FromArgb(0x4d, 0, 0, 0), colors.Shadow);
            Assert.Equal(Shadow.GlowColor, colors.Glow);
        }

        // The controls whose templates drew fixed colors, in each state that
        // did, and a selection of others
        private static IEnumerable<View> ControlsToDraw()
        {
            foreach (InfoBarSeverity severity in Enum.GetValues<InfoBarSeverity>())
            {
                yield return new InfoBar() { Severity = severity };
            }

            yield return new FloatingButton();
            yield return Pressed(new FloatingButton());
            yield return new FloatingButton() { IsSubtleStyle = true };
            yield return Pressed(new FloatingButton() { IsSubtleStyle = true });
            yield return new FloatingButton() { IsEnabled = false };
            yield return new ToggleButton() { Text = "Toggle" };
            yield return Pressed(new ToggleButton() { Text = "Toggle" });
            yield return new ToggleButton() { Text = "Toggle", IsChecked = true };
            yield return new DayOfWeekToggleButton(new DaysOfWeekPicker()) { Text = "M" };
            yield return Pressed(new DayOfWeekToggleButton(new DaysOfWeekPicker()) { Text = "M" });
            yield return new DayOfWeekToggleButton(new DaysOfWeekPicker()) { Text = "M", IsChecked = true };
            yield return new ListViewSwipeItem() { Command = new UICommand() { Text = "Done", AccentColor = CommandColor } };
            yield return new NavigationViewScrim(null);
            yield return new CheckBox() { IsChecked = true };
            yield return new ToggleSwitch() { IsOn = true };
        }

        private static TControl Pressed<TControl>(
            TControl control)
            where TControl : TemplatedControl
        {
            typeof(TemplatedControl).GetProperty(nameof(TemplatedControl.IsPressed)).SetValue(control, true);
            return control;
        }

        [Fact]
        public void BasicControlsDrawInAReplacedPalette()
        {
            var theme = new TestTheme().Create(ThemeMode.Dark, Accent);

            // A color's own value, ignoring the opacity a template gives it
            var expected = typeof(ColorScheme).GetProperties().Where(p => p.PropertyType == typeof(Color)).Select(p => (Color)p.GetValue(TestColors))
                .Concat(typeof(BasicColors).GetProperties().Where(p => (p.PropertyType == typeof(Color)) || (p.PropertyType == typeof(Color?))).Select(p => (Color)p.GetValue(TestBasicColors)))
                .Append(CommandColor)
                .Select(WithoutOpacity)
                .ToHashSet();

            foreach (var control in ControlsToDraw())
            {
                var recorder = new RecordingContext();
                control.Theme = theme;
                SetBounds(control);

                theme.GetTemplate(control.GetType()).RenderCore(control, recorder);

                Assert.Contains(recorder.Colors, c => !c.IsTransparent);
                foreach (var color in recorder.Colors.Where(c => !c.IsTransparent))
                {
                    Assert.True(expected.Contains(WithoutOpacity(color)), $"{control.GetType().Name} drew {color}, which isn't in the theme");
                }
            }
        }

        [Fact]
        public void ShadowsFollowTheThemeUnlessTheAppSetsItsOwn()
        {
            var theme = new BasicTheme().Create(ThemeMode.Light, Accent);
            var first = theme.WithExtension(new BasicColors() { Shadow = Color.FromRgb(0x010101) });
            var second = theme.WithExtension(new BasicColors() { Shadow = Color.FromRgb(0x020202) });

            var button = new FloatingButton();
            var host = ((ITemplatedView)button).TemplateHost;
            void Show(Theme shown)
            {
                button.Theme = shown;
                host.Apply(shown);
            }

            Show(first);
            Assert.Equal(Color.FromRgb(0x010101), button.Shadow.Color);

            Show(second);
            Assert.Equal(Color.FromRgb(0x020202), button.Shadow.Color);

            var own = new Shadow() { Color = Colors.Red };
            button.Shadow = own;
            Show(first);
            Assert.Equal(own, button.Shadow);
        }

        // On each severity's color, in whatever the palette puts on it, so it
        // reads in high contrast's yellow accent and dark mode's pale error
        [Theory]
        [InlineData(ThemeMode.Dark, InfoBarSeverity.Error)]
        [InlineData(ThemeMode.HighContrast, InfoBarSeverity.Error)]
        [InlineData(ThemeMode.HighContrast, InfoBarSeverity.Informational)]
        public void AnInfoBarsMarkReadsOnItsSeverity(
            ThemeMode mode,
            InfoBarSeverity severity)
        {
            var theme = new BasicTheme().Create(mode, Accent);
            var control = new InfoBar() { Theme = theme, Severity = severity };
            SetBounds(control);

            var recorder = new RecordingContext();
            theme.GetTemplate(control.GetType()).RenderCore(control, recorder);

            var on = (severity == InfoBarSeverity.Error) ? theme.Colors.OnError : theme.Colors.OnAccent;
            Assert.Contains(on, recorder.Colors);
            Assert.DoesNotContain(Colors.White, recorder.Colors.Where(c => c != theme.Colors.Foreground));
        }

        [Fact]
        public void APackageMustGiveItsColors()
        {
            Assert.True(typeof(AppTheme).GetMethod(nameof(AppTheme.CreateColors)).IsAbstract);
        }

        [Fact]
        public void ASwipedRowUsesItsCommandsForeground()
        {
            var foreground = Color.FromRgb(0x123456);
            var theme = new TestTheme().Create(ThemeMode.Light, Accent);
            var control = new ListViewSwipeItem() { Theme = theme, Command = new UICommand() { Text = "Done", ForegroundColor = foreground } };
            SetBounds(control);

            var recorder = new RecordingContext();
            theme.GetTemplate(control.GetType()).RenderCore(control, recorder);

            Assert.Equal(new[] { foreground }, recorder.Colors.Distinct());
        }

        [Fact]
        public void BasicsTemplatesHaveNoColorsOfTheirOwn()
        {
            var templates = Path.Combine(RepositoryRoot(), "themes", "basic", "Templates");
            var literal = new Regex(@"Color\.From\w+\(|new Color\(|(?<!\.)\bColors\.(?!Transparent\b)[A-Z]\w*");

            foreach (var file in Directory.GetFiles(templates, "*.cs"))
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    // A role's color, given an opacity, is still the role's
                    bool isScrim = line.Contains("Color.FromArgb(0x59, scrim.R, scrim.G, scrim.B)");
                    Assert.False(literal.IsMatch(line) && !isScrim, $"{Path.GetFileName(file)}: {line.Trim()}");
                }
            }
        }

        private static void SetBounds(
            View view)
        {
            typeof(View).GetField("_frame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, new Rect(0, 0, 100, 100));
        }

        private static Color WithoutOpacity(
            Color color)
        {
            return Color.FromRgb(color.R, color.G, color.B);
        }

        private static string RepositoryRoot(
            [CallerFilePath] string path = null)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "..", ".."));
        }

        // Keeps every color drawn with
        private sealed class RecordingContext : DrawingContext
        {
            public List<Color> Colors { get; } = new List<Color>();

            private void Add(
                Brush brush)
            {
                if (brush is SolidColorBrush solid)
                {
                    this.Colors.Add(solid.Color);
                }
            }

            protected override void ClearCore(Color color) => this.Colors.Add(color);

            public override void DrawRectangle(Rect rectangle, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void FillRectangle(Rect rectangle, Color color) => this.Colors.Add(color);

            public override void DrawRoundedRectangle(Rect rectangle, float cornerRadius, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void FillRoundedRectangle(Rect rectangle, float cornerRadius, Color color) => this.Colors.Add(color);

            public override void DrawEllipse(Point center, float radiusX, float radiusY, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void FillEllipse(Point center, float radiusX, float radiusY, Color color) => this.Colors.Add(color);

            public override void FillEllipse(Rect rectangle, Color color) => this.Colors.Add(color);

            public override void FillShadow(Rect rectangle, Shadow shadow) => this.Colors.Add(shadow.Color);

            public override void DrawImage(Bitmap bitmap, Rect rectangle, Color tint) => this.Colors.Add(tint);

            public override void DrawLine(Point start, Point end, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void DrawArc(Point center, float radius, float startAngle, float sweepAngle, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void DrawText(TextLayout layout, Point origin, Color color) => this.Colors.Add(color);

            public override void DrawText(string text, Rect bounds, Color color, TextFormat textFormat) => this.Colors.Add(color);

            public override void FillRectangle(Rect rectangle, Brush brush) => this.Add(brush);

            public override void FillRoundedRectangle(Rect rectangle, float cornerRadius, Brush brush) => this.Add(brush);

            public override void FillEllipse(Rect rectangle, Brush brush) => this.Add(brush);

            public override void FillGeometry(PathGeometry geometry, Brush brush) => this.Add(brush);

            public override void DrawGeometry(PathGeometry geometry, Color color, float strokeWidth = 1) => this.Colors.Add(color);

            public override void Pop()
            {
            }

            protected override void PushClipCore(Rect clip)
            {
            }

            protected override void PushTransformCore(Matrix3x2 transform)
            {
            }

            protected override void PushOpacityCore(float opacity)
            {
            }
        }
    }
}
