using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using XForma.Controls;
using XForma.Themes.Basic;
using Xunit;

namespace XForma.Tests
{
    // What a control lays out and hit-tests by is its theme's: Basic's values
    // keep its screens as they were, and another theme's move both the layout
    // and where presses land
    public class ControlStyleTests
    {
        private static readonly Theme Basic = new BasicTheme().Create(ThemeMode.Light, Colors.Blue);

        private static void SetBounds(
            View view,
            Size size)
        {
            typeof(View).GetField("_frame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, new Rect(0, 0, size.Width, size.Height));
        }

        private static PointerEventArgs Mouse(
            Point position,
            PointerInputState state)
        {
            return new PointerEventArgs(1, PointerDeviceType.Mouse, position, state, state == PointerInputState.Began, KeyModifiers.None);
        }

        private static Size Measure(
            View view)
        {
            // The control's own measuring: View.Measure needs a platform
            var measure = view.GetType().GetMethod("MeasureOverride", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                return (Size)measure.Invoke(view, new object[] { new Size(float.PositiveInfinity, float.PositiveInfinity) });
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }

        private sealed class TestSlider : Slider
        {
            public void Press(
                Point position)
            {
                this.OnPointerPressed(Mouse(position, PointerInputState.Began));
                this.OnPointerReleased(Mouse(position, PointerInputState.Ended));
            }
        }

        private sealed class TestCalendar : CalendarView
        {
            public CalendarViewPart Press(
                Point position)
            {
                this.OnPointerPressed(Mouse(position, PointerInputState.Began));
                var part = this.PressedPart;
                this.OnPointerCanceled(Mouse(position, PointerInputState.Ended));
                return part;
            }
        }

        [Fact]
        public void BasicKeepsItsCheckBoxLayout()
        {
            var box = new CheckBox() { Theme = Basic };

            Assert.Equal(new Size(20, 32), Measure(box));

            SetBounds(box, new Size(100, 32));
            Assert.Equal(new Rect(0, 6, 20, 20), box.BoxBounds);
            Assert.Equal(new Rect(28, 0, 72, 32), box.TextBounds);
        }

        [Fact]
        public void BasicKeepsItsSliderLayout()
        {
            var slider = new Slider() { Theme = Basic };

            Assert.Equal(new Size(200, 32), Measure(slider));

            SetBounds(slider, new Size(200, 32));
            Assert.Equal(new Rect(10, 16, 180, 0), slider.TrackBounds);
        }

        [Fact]
        public void BasicKeepsItsCalendarLayout()
        {
            var calendar = new CalendarView() { Theme = Basic };

            Assert.Equal(new Size(296, 324), Measure(calendar));

            SetBounds(calendar, new Size(296, 324));
            Assert.Equal(new Rect(8, 76, 40, 40), calendar.GetDayBounds(0, 0));
            Assert.Equal(new Rect(8, 48, 40, 28), calendar.GetWeekdayBounds(0));
            Assert.Equal(new Rect(16, 8, 192, 40), calendar.HeaderBounds);
            Assert.Equal(new Rect(208, 8, 40, 40), calendar.PreviousButtonBounds);
            Assert.Equal(new Rect(248, 8, 40, 40), calendar.NextButtonBounds);
        }

        [Fact]
        public void AThemeSizesACheckBox()
        {
            var theme = Basic.WithExtension(Basic.CheckBoxStyle with { BoxSize = 40, MinimumHeight = 44 });
            var box = new CheckBox() { Theme = theme };

            Assert.Equal(new Size(40, 44), Measure(box));

            SetBounds(box, new Size(100, 44));
            Assert.Equal(new Rect(0, 2, 40, 40), box.BoxBounds);
            Assert.Equal(new Rect(48, 0, 52, 44), box.TextBounds);
        }

        [Fact]
        public void AThemeSizesASlidersThumbAndWherePressesLand()
        {
            var theme = Basic.WithExtension(Basic.SliderStyle with { ThumbSize = 40, TouchHeight = 60, DefaultWidth = 300 });
            var slider = new TestSlider() { Theme = theme, Minimum = 0, Maximum = 100, StepFrequency = 0 };

            Assert.Equal(new Size(300, 60), Measure(slider));

            SetBounds(slider, new Size(300, 60));
            Assert.Equal(new Rect(20, 30, 260, 0), slider.TrackBounds);

            // The track starts half a thumb in: at Basic's thumb, 150 is the
            // middle too, but 85 is a quarter of this track and not of that one
            slider.Press(new Point(85, 30));
            Assert.Equal(25, slider.Value, 3);
        }

        [Fact]
        public void AThemeSizesACalendarsCellsAndWherePressesLand()
        {
            var theme = Basic.WithExtension(Basic.CalendarViewStyle with { CellSize = 60, HeaderHeight = 50, WeekdayHeight = 30, Inset = 10, HeaderInset = 4 });
            var calendar = new TestCalendar() { Theme = theme };

            Assert.Equal(new Size(440, 460), Measure(calendar));

            SetBounds(calendar, new Size(440, 460));
            Assert.Equal(new Rect(70, 90, 60, 60), calendar.GetDayBounds(0, 1));
            Assert.Equal(new Rect(14, 10, 296, 50), calendar.HeaderBounds);

            // Below Basic's calendar: only this theme's sixth row is here
            Assert.Equal(CalendarViewPart.Day, calendar.Press(new Point(425, 445)));
            Assert.Equal(new Rect(370, 390, 60, 60), calendar.GetDayBounds(5, 6));

            // Basic's arrow buttons are days here, and this theme's are further along
            Assert.Equal(CalendarViewPart.NextMonth, calendar.Press(new Point(400, 30)));
            Assert.Equal(CalendarViewPart.None, calendar.Press(new Point(250, 30)));
        }

        private static void DeliverTheme(
            View view,
            Theme theme)
        {
            view.Theme = theme;
            view.GetType().GetMethod("OnThemeChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { theme });
        }

        [Fact]
        public void AThemeSizesASwitchsTrack()
        {
            var theme = Basic.WithExtension(Basic.ToggleSwitchStyle with { TrackWidth = 60, TrackHeight = 30, TouchHeight = 50 });
            var toggle = new ToggleSwitch() { Theme = theme };
            SetBounds(toggle, new Size(200, 50));

            Assert.Equal(new Rect(0, 10, 60, 30), toggle.TrackBounds);
        }

        [Fact]
        public void AThemeSizesATreeViewsItems()
        {
            var theme = Basic.WithExtension(Basic.TreeViewStyle with { ItemHeight = 50, Indent = 30, ChevronWidth = 40, ItemInset = 2 });
            var item = (TreeViewItem)Activator.CreateInstance(typeof(TreeViewItem), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { null, new TreeViewNode(), null, 2 }, null);
            DeliverTheme(item, theme);
            SetBounds(item, new Size(300, 50));

            Assert.Equal(50, item.Height);
            Assert.Equal(new Rect(62, 0, 40, 50), item.ChevronBounds);
        }

        [Fact]
        public void AThemeInsetsAPopupsContent()
        {
            var theme = Basic.WithExtension(Basic.PopupStyle with { ContentInset = 20 });
            var view = new MenuFlyoutView(new MenuFlyout());
            var frame = (View)Activator.CreateInstance(
                typeof(MenuFlyout).GetNestedType("MenuFrame", BindingFlags.NonPublic),
                new object[] { view });

            DeliverTheme(frame, theme);

            Assert.Equal(new Thickness(20), view.Margin);
        }

        [Fact]
        public void ASizeTheAppSetsStays()
        {
            var item = new NavigationViewItem();
            DeliverTheme(item, Basic);
            Assert.Equal(40, item.Height);
            Assert.Equal(new Thickness(4, 0, 4, 0), item.Margin);

            // The style's value follows the theme...
            DeliverTheme(item, Basic.WithExtension(Basic.NavigationViewStyle with { ItemHeight = 60 }));
            Assert.Equal(60, item.Height);

            // ...until the app gives its own, before or after it's attached
            item.Height = 25;
            DeliverTheme(item, Basic);
            Assert.Equal(25, item.Height);

            var early = new NavigationViewItem() { Height = 25 };
            DeliverTheme(early, Basic);
            Assert.Equal(25, early.Height);
        }

        // Tokens are shape only: a value in the framework would be a default look
        [Fact]
        public void EveryTokenIsRequired()
        {
            var optional = new[]
            {
                (typeof(TextStyle), nameof(TextStyle.FontFamily)),
                (typeof(TextStyle), nameof(TextStyle.FontWeight)),
                (typeof(CommandBarStyle), nameof(CommandBarStyle.LabelFontSize)),
            };

            var tokens = typeof(ThemeTokens).Assembly.GetExportedTypes()
                .Where(t => t.Namespace == typeof(ThemeTokens).Namespace && (t.Name.EndsWith("Style", StringComparison.Ordinal) || (t == typeof(Typography)) || (t == typeof(Shapes)) || (t == typeof(Motion))))
                .ToList();
            Assert.Contains(typeof(CheckBoxStyle), tokens);

            foreach (var token in tokens)
            {
                foreach (var property in token.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(p => p.Name != "EqualityContract"))
                {
                    bool isRequired = property.IsDefined(typeof(RequiredMemberAttribute), false);
                    Assert.True(isRequired || optional.Contains((token, property.Name)), $"{token.Name}.{property.Name} isn't required, so the framework gives it a value");
                }
            }
        }

        [Fact]
        public void BasicSuppliesEveryToken()
        {
            var getters = typeof(ThemeTokens).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("get_", StringComparison.Ordinal))
                .ToList();
            Assert.NotEmpty(getters);

            foreach (var getter in getters)
            {
                Assert.NotNull(getter.Invoke(null, new object[] { Basic }));
            }
        }

        [Fact]
        public void AThemeChangeMeasuresAControlAgain()
        {
            var box = new CheckBox() { Theme = Basic };
            var host = ((ITemplatedView)box).TemplateHost;
            var measured = typeof(View).GetField("_contentMeasureSize", BindingFlags.Instance | BindingFlags.NonPublic);
            host.Apply(Basic);

            measured.SetValue(box, new Size(1, 1));
            host.Apply(Basic);
            Assert.Equal(new Size(1, 1), measured.GetValue(box));

            host.Apply(Basic.WithExtension(Basic.CheckBoxStyle with { BoxSize = 40 }));
            Assert.True(float.IsNaN(((Size)measured.GetValue(box)).Width));
        }

        [Fact]
        public void BasicKeepsATokenTheThemeAlreadyHas()
        {
            var style = Basic.CheckBoxStyle with { BoxSize = 40 };

            var theme = new BasicTheme().Apply(new Theme().WithExtension(style));

            Assert.Equal(style, theme.CheckBoxStyle);
        }

        [Fact]
        public void AThemeWithoutAStyleSaysSo()
        {
            var box = new CheckBox() { Theme = new Theme() };

            var error = Assert.Throws<InvalidOperationException>(() => Measure(box));
            Assert.Contains(nameof(CheckBoxStyle), error.Message);
        }
    }
}
