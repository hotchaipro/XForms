using System;
using System.Linq;
using XForms.Controls;
using Xunit;

namespace XForms.Tests
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
    }
}
