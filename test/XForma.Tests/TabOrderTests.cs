using System;
using System.Linq;
using XForma.Controls;
using Xunit;

namespace XForma.Tests
{
    // The order Tab reaches a view's children in (View.TabOrderChildren)
    public class TabOrderTests
    {
        // A view with children, in drawing order
        private sealed class Box : View
        {
            public Box(
                params View[] children)
            {
                foreach (var child in children)
                {
                    this.AddVisualChild(child);
                }
            }
        }

        [Fact]
        public void WithoutTabIndexesTabFollowsDrawingOrder()
        {
            var a = new Box();
            var b = new Box();
            var c = new Box();

            var parent = new Box(a, b, c);

            Assert.Equal(new View[] { a, b, c }, parent.TabOrderChildren);
        }

        [Fact]
        public void LowerTabIndexesComeFirst()
        {
            var a = new Box() { TabIndex = 2 };
            var b = new Box() { TabIndex = 0 };
            var c = new Box() { TabIndex = 1 };

            var parent = new Box(a, b, c);

            Assert.Equal(new View[] { b, c, a }, parent.TabOrderChildren);
        }

        [Fact]
        public void ViewsGivenATabIndexComeBeforeTheRestAndTiesKeepDrawingOrder()
        {
            var a = new Box();
            var b = new Box() { TabIndex = 5 };
            var c = new Box();
            var d = new Box() { TabIndex = 5 };

            var parent = new Box(a, b, c, d);

            Assert.Equal(new View[] { b, d, a, c }, parent.TabOrderChildren);
        }

        [Fact]
        public void TabIndexDoesNotChangeDrawingOrder()
        {
            var a = new Box() { TabIndex = 1 };
            var b = new Box() { TabIndex = 0 };

            var parent = new Box(a, b);

            Assert.Equal(new View[] { a, b }, parent.DrawingOrderChildren);
        }

        [Fact]
        public void ATabStripDrawsOverItsFrameButTabReachesItFirst()
        {
            var tabStrip = new TabStrip();
            var frame = new Frame();

            var parent = new Box(tabStrip, frame);

            Assert.Equal(new View[] { frame, tabStrip }, parent.DrawingOrderChildren);
            Assert.Equal(new View[] { tabStrip, frame }, parent.TabOrderChildren);
        }

        [Fact]
        public void AViewRaisedOverItsSiblingsIsALayerOfItsOwn()
        {
            var plain = new Box();
            var raised = new Box() { ZIndex = 1 };
            var tabStrip = new TabStrip();

            var parent = new Box(plain, raised, tabStrip);
            plain.Parent = parent;
            raised.Parent = parent;
            tabStrip.Parent = parent;

            Assert.True(plain.CanBeLightweight);
            Assert.False(raised.CanBeLightweight);
            Assert.False(tabStrip.CanBeLightweight);
        }

        [Fact]
        public void ANavigationViewsPaneComesBeforeItsContent()
        {
            var navigationView = new NavigationView();

            var children = navigationView.TabOrderChildren;
            var drawn = navigationView.DrawingOrderChildren;

            // Drawn: the content, the scrim, the pane, the menu button. Tab:
            // the menu button, the pane, the content, then the rest
            Assert.IsType<NavigationViewMenuButton>(children[0]);
            Assert.IsType<NavigationViewPane>(children[1]);
            Assert.Same(drawn[0], children[2]);
            Assert.IsType<NavigationViewScrim>(children[3]);
            Assert.IsType<NavigationViewPane>(drawn[2]);
        }

        private static readonly Theme Basic = new XForma.Themes.Basic.BasicTheme().Create(ThemeMode.Light, Colors.Blue);

        private static NavigationView Narrow(
            bool isOpen)
        {
            var navigationView = new NavigationView() { Theme = Basic };
            navigationView.MenuItems.Add(new NavigationViewItem() { Text = "one" });
            navigationView.MenuItems.Add(new NavigationViewItem() { Text = "two" });
            navigationView.SetIsNarrow(true);
            navigationView.IsPaneOpen = isOpen;
            return navigationView;
        }

        private static View ContentHost(
            NavigationView navigationView)
        {
            return navigationView.DrawingOrderChildren[0];
        }

        [Fact]
        public void ClosedATabReachesTheContentAndTheRestOfTheTree()
        {
            var navigationView = Narrow(isOpen: false);
            var parent = new Box(navigationView);

            Assert.True(navigationView.IsInTabOrder(ContentHost(navigationView)));
            Assert.Same(parent, parent.ActiveTabScope);
        }

        [Fact]
        public void AnOpenDrawerKeepsTabInsideIt()
        {
            var navigationView = Narrow(isOpen: true);
            var parent = new Box(new Box(), new Box(navigationView));

            Assert.False(navigationView.IsInTabOrder(ContentHost(navigationView)));
            Assert.True(navigationView.IsInTabOrder(navigationView.TabOrderChildren[1]));
            Assert.Same(navigationView, parent.ActiveTabScope);
        }

        [Fact]
        public void AWidePaneKeepsNothing()
        {
            var navigationView = new NavigationView() { Theme = Basic };
            navigationView.SetIsNarrow(false);
            navigationView.IsPaneOpen = true;

            Assert.True(navigationView.IsInTabOrder(ContentHost(navigationView)));
            Assert.False(navigationView.IsTabScope);
        }

        [Fact]
        public void APaneIsOneTabStopTheFirstItemWithNothingSelected()
        {
            var navigationView = Narrow(isOpen: false);

            Assert.True(navigationView.MenuItems[0].IsTabStop);
            Assert.False(navigationView.MenuItems[1].IsTabStop);
        }

        [Fact]
        public void APanesTabStopIsTheSelectedItem()
        {
            var navigationView = Narrow(isOpen: false);

            navigationView.SelectedItem = navigationView.MenuItems[1];

            Assert.False(navigationView.MenuItems[0].IsTabStop);
            Assert.True(navigationView.MenuItems[1].IsTabStop);
        }
    }
}
