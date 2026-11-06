using System;
using XForma.Layouts;
using XForma.Testing;
using Xunit;

// The tests share Application.Current and other process-wide state
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace XForma.Tests
{
    // How each panel measures and places its children
    public class LayoutTests
    {
        private sealed class TestApp : Application
        {
            protected override Theme CreateTheme(
                ThemeMode mode,
                Color accentColor)
            {
                return new XForma.Themes.Basic.BasicTheme().Create(mode, accentColor);
            }
        }

        private sealed class Box : View
        {
            public Box(
                float width,
                float height)
            {
                this.Width = width;
                this.Height = height;
            }
        }

        public LayoutTests()
        {
            TestHost.Start(new TestApp());
        }

        private static void LayOut(
            View root,
            float width,
            float height)
        {
            TestHost.LayOut(root, width, height);
        }

        private static void AssertAt(
            View view,
            float x,
            float y,
            float width,
            float height)
        {
            Assert.Equal(new Point(x, y), view.FrameOrigin);
            Assert.Equal(new Size(width, height), view.FrameSize);
        }

        [Fact]
        public void AStackPanelStacksDownWithSpacing()
        {
            var a = new Box(50, 20) { HorizontalAlignment = HorizontalAlignment.Left };
            var b = new Box(60, 30) { HorizontalAlignment = HorizontalAlignment.Left };
            var panel = new StackPanel() { Spacing = 5 };
            panel.Children.Add(a);
            panel.Children.Add(b);

            LayOut(panel, 200, 200);

            AssertAt(a, 0, 0, 50, 20);
            AssertAt(b, 0, 25, 60, 30);
        }

        [Fact]
        public void AStackPanelsDesiredSizeFitsItsChildren()
        {
            var panel = new StackPanel() { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(new Box(50, 20));
            panel.Children.Add(new Box(60, 30));

            panel.Measure(new Size(200, 200));

            Assert.Equal(new Size(60, 55), panel.DesiredSize);
        }

        [Fact]
        public void AHorizontalStackPanelStacksAcross()
        {
            var a = new Box(50, 20) { VerticalAlignment = VerticalAlignment.Top };
            var b = new Box(60, 30) { VerticalAlignment = VerticalAlignment.Top };
            var panel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 10 };
            panel.Children.Add(a);
            panel.Children.Add(b);

            LayOut(panel, 200, 200);

            AssertAt(a, 0, 0, 50, 20);
            AssertAt(b, 60, 0, 60, 30);
        }

        [Fact]
        public void AMarginMovesAndShrinksAView()
        {
            var child = new Box(50, 20) { Margin = new Thickness(10, 5, 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            var panel = new StackPanel();
            panel.Children.Add(child);

            LayOut(panel, 200, 200);

            AssertAt(child, 10, 5, 50, 20);
        }

        [Fact]
        public void AnAlignmentPlacesAViewInItsSlot()
        {
            var child = new Box(50, 20) { HorizontalAlignment = HorizontalAlignment.Right };
            var panel = new StackPanel();
            panel.Children.Add(child);

            LayOut(panel, 200, 200);

            AssertAt(child, 150, 0, 50, 20);
        }

        [Fact]
        public void AHiddenViewTakesNoRoom()
        {
            var hidden = new Box(50, 20) { IsVisible = false };
            var shown = new Box(50, 20) { HorizontalAlignment = HorizontalAlignment.Left };
            var panel = new StackPanel();
            panel.Children.Add(hidden);
            panel.Children.Add(shown);

            LayOut(panel, 200, 200);

            AssertAt(shown, 0, 0, 50, 20);
        }

        [Fact]
        public void ADockPanelDocksThenFills()
        {
            var top = new Box(Dimension.Auto, 30);
            var left = new Box(40, Dimension.Auto);
            var fill = new Box(Dimension.Auto, Dimension.Auto);
            var panel = new DockPanel();
            panel.Children.Add(top, Dock.Top);
            panel.Children.Add(left, Dock.Left);
            panel.Children.Add(fill, Dock.CenterOverlay);

            LayOut(panel, 200, 100);

            AssertAt(top, 0, 0, 200, 30);
            AssertAt(left, 0, 30, 40, 70);
            AssertAt(fill, 40, 30, 160, 70);
        }

        [Fact]
        public void AGridSharesItsStarsAfterFixedAndAutoSizes()
        {
            var fixedCell = new Box(Dimension.Auto, Dimension.Auto);
            var autoCell = new Box(30, Dimension.Auto);
            var starCell = new Box(Dimension.Auto, Dimension.Auto);
            var doubleCell = new Box(Dimension.Auto, Dimension.Auto);
            var grid = new Grid();
            grid.ColumnDefinitions.AddRange("100,Auto,*,2*");
            grid.Children.Add(fixedCell, 0, 0);
            grid.Children.Add(autoCell, 0, 1);
            grid.Children.Add(starCell, 0, 2);
            grid.Children.Add(doubleCell, 0, 3);

            LayOut(grid, 430, 50);

            Assert.Equal(100, fixedCell.FrameSize.Width);
            Assert.Equal(30, autoCell.FrameSize.Width);
            Assert.Equal(100, starCell.FrameSize.Width);
            Assert.Equal(200, doubleCell.FrameSize.Width);
            Assert.Equal(130, starCell.FrameOrigin.X);
        }

        [Fact]
        public void AGridsSpacingSeparatesCellsAndASpanCoversThem()
        {
            var spanning = new Box(Dimension.Auto, Dimension.Auto);
            var below = new Box(Dimension.Auto, Dimension.Auto);
            var grid = new Grid() { ColumnSpacing = 10, RowSpacing = 4 };
            grid.ColumnDefinitions.AddRange("50,50");
            grid.RowDefinitions.AddRange("20,20");
            grid.Children.Add(spanning, 0, 0, columnSpan: 2);
            grid.Children.Add(below, 1, 1);

            LayOut(grid, 200, 100);

            AssertAt(spanning, 0, 0, 110, 20);
            AssertAt(below, 60, 24, 50, 20);
        }

        [Fact]
        public void AUniformGridSharesTheRoomEvenly()
        {
            var cells = new[] { new Box(20, 20), new Box(20, 20), new Box(20, 20), new Box(20, 20) };
            var grid = new UniformGrid();
            foreach (var cell in cells)
            {
                grid.Children.Add(cell);
            }

            LayOut(grid, 200, 100);

            Assert.Equal(cells[0].FrameSize, cells[3].FrameSize);
            Assert.NotEqual(cells[0].FrameOrigin, cells[3].FrameOrigin);
        }

        [Fact]
        public void AWrapPanelWrapsWhenALineIsFull()
        {
            var a = new Box(80, 20);
            var b = new Box(80, 20);
            var c = new Box(80, 20);
            var panel = new WrapPanel() { ItemSpacing = 10, LineSpacing = 5 };
            panel.Children.Add(a);
            panel.Children.Add(b);
            panel.Children.Add(c);

            LayOut(panel, 200, 100);

            Assert.Equal(new Point(0, 0), a.FrameOrigin);
            Assert.Equal(new Point(90, 0), b.FrameOrigin);
            Assert.Equal(new Point(0, 25), c.FrameOrigin);
        }

        [Fact]
        public void ACanvasPlacesChildrenWhereItsTold()
        {
            var child = new Box(30, 40);
            var canvas = new Canvas();
            canvas.Children.Add(child, 15, 25);

            LayOut(canvas, 200, 100);

            AssertAt(child, 15, 25, 30, 40);
        }

        [Fact]
        public void RightToLeftMirrorsAStackPanelsChildren()
        {
            var child = new Box(50, 20) { HorizontalAlignment = HorizontalAlignment.Left };
            var panel = new StackPanel() { FlowDirection = FlowDirection.RightToLeft };
            panel.Children.Add(child);

            LayOut(panel, 200, 200);

            Assert.Equal(150, child.FrameOrigin.X);
        }

        [Fact]
        public void TestHostFindsAViewByName()
        {
            var named = new Box(10, 10) { Name = "save" };
            var panel = new StackPanel();
            panel.Children.Add(new Box(10, 10));
            panel.Children.Add(named);

            Assert.Same(named, TestHost.Find<Box>(panel, "save"));
            Assert.Throws<InvalidOperationException>(() => TestHost.Find<Box>(panel, "missing"));
        }
    }
}
