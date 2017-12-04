using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlSize = global::Windows.Foundation.Size;
using System.Collections.Generic;

namespace XForms.Windows
{
    internal enum NativeDockRegion
    {
        CenterOverlay = 0,
        Left,
        Top,
        Right,
        Bottom,
        LeftOverlay,
        TopOverlay,
        RightOverlay,
        BottomOverlay,
    }

    internal class NativeDockPanel : Panel
    {
        public static readonly DependencyProperty DockProperty = DependencyProperty.RegisterAttached(
            "Dock",
            typeof(NativeDockRegion),
            typeof(NativeDockPanel),
            new PropertyMetadata(NativeDockRegion.Left, OnDockPropertyChanged));

        public NativeDockPanel()
        {
            // NOTE: For whatever reason, the Background property must be set for input handling,
            // such as the PointerPressed event in the parent Page, to work properly.
            //this.Background = new SolidColorBrush(global::Windows.UI.Colors.Transparent);
        }

        public static NativeDockRegion GetDock(
            UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (NativeDockRegion)element.GetValue(DockProperty);
        }

        public static void SetDock(
            UIElement element,
            NativeDockRegion dock)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(DockProperty, dock);
        }

        private static void OnDockPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            UIElement element = (UIElement)d;
            NativeDockRegion value = (NativeDockRegion)e.NewValue;

            NativeDockPanel panel = VisualTreeHelper.GetParent(element) as NativeDockPanel;
            if (panel != null)
            {
                panel.InvalidateMeasure();
            }
        }

        protected override XamlSize MeasureOverride(
            XamlSize availableSize)
        {
            // NOTE: Children must be measured in order to set the DesiredSize 
            // required for the ArrangeOverride implementation.

            // Keep track of the space used by children
            double usedLeft = 0;
            double usedTop = 0;
            double usedRight = 0;
            double usedBottom = 0;

            // Keep track of the starting point for measuring each dimension
            // in support of overlay regions
            double startLeft = 0;
            double startTop = 0;
            double startRight = 0;
            double startBottom = 0;

            double minWidth = 0;
            double minHeight = 0;

            var remainingSize = availableSize;

            var centerElements = new List<UIElement>();

            foreach (UIElement child in this.Children)
            {
                var dockRegion = GetDock(child);

                if (dockRegion == NativeDockRegion.CenterOverlay)
                {
                    // Arrange the center elements last
                    centerElements.Add(child);
                }
                else
                {
                    child.Measure(remainingSize);

                    XamlSize childSize = child.DesiredSize;

                    if (dockRegion == NativeDockRegion.Left)
                    {
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        startLeft += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.LeftOverlay)
                    {
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.Top)
                    {
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        startTop += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                    }
                    else if (dockRegion == NativeDockRegion.TopOverlay)
                    {
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                    }
                    else if (dockRegion == NativeDockRegion.Right)
                    {
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        startRight += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.RightOverlay)
                    {
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.Bottom)
                    {
                        usedBottom = Math.Max(usedBottom, startBottom + childSize.Height);
                        startBottom += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                    }
                    else if (dockRegion == NativeDockRegion.BottomOverlay)
                    {
                        usedBottom = Math.Max(usedBottom, startBottom + childSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported dock region.");
                    }

                    // Update the remaining size
                    remainingSize = new XamlSize(
                        Math.Max(0, availableSize.Width - startLeft - startRight),
                        Math.Max(0, availableSize.Height - startTop - startBottom));
                }
            }

            // Measure the center elements now that the center size has been completely calculated
            XamlSize centerSize = new XamlSize(0, 0);
            foreach (var child in centerElements)
            {
                child.Measure(remainingSize);

                XamlSize childSize = child.DesiredSize;

                centerSize = new XamlSize(
                    Math.Max(centerSize.Width, childSize.Width),
                    Math.Max(centerSize.Height, childSize.Height));
            }

            // Calculate the total size used by children
            XamlSize usedSize = new XamlSize(
                Math.Max(minWidth, usedLeft + usedRight + centerSize.Width),
                Math.Max(minHeight, usedTop + usedBottom + centerSize.Height));

            // Default the final size to the size used by children
            var finalSize = usedSize;

            // Adjust final size for horizontal stretch taking infinity into account
            if ((this.HorizontalAlignment == HorizontalAlignment.Stretch)
                && (!double.IsInfinity(availableSize.Width))
                && (!double.IsNaN(availableSize.Width)))
            {
                finalSize.Width = availableSize.Width;
            }

            // Adjust final size for vertical stretch taking infinity into account
            if ((this.VerticalAlignment == VerticalAlignment.Stretch)
                && (!double.IsInfinity(availableSize.Height))
                && (!double.IsNaN(availableSize.Height)))
            {
                finalSize.Height = availableSize.Height;
            }

            return finalSize;
        }

        protected override XamlSize ArrangeOverride(
            XamlSize arrangeSize)
        {
            Rect remainingRect = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);

            var centerElements = new List<UIElement>();

            foreach (UIElement child in this.Children)
            {
                var dockRegion = GetDock(child);

                if (dockRegion == NativeDockRegion.CenterOverlay)
                {
                    // Arrange the center elements last
                    centerElements.Add(child);
                }
                else
                {
                    Rect arrangeRect = remainingRect;

                    XamlSize childSize = child.DesiredSize;

                    if (dockRegion == NativeDockRegion.Left)
                    {
                        arrangeRect.Width = childSize.Width;
                        remainingRect.X += childSize.Width;
                        remainingRect.Width = Math.Max(0, remainingRect.Width - childSize.Width);
                    }
                    else if (dockRegion == NativeDockRegion.LeftOverlay)
                    {
                        arrangeRect.Width = childSize.Width;
                    }
                    else if (dockRegion == NativeDockRegion.Top)
                    {
                        arrangeRect.Height = childSize.Height;
                        remainingRect.Y += childSize.Height;
                        remainingRect.Height = Math.Max(0, remainingRect.Height - childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.TopOverlay)
                    {
                        arrangeRect.Height = childSize.Height;
                    }
                    else if (dockRegion == NativeDockRegion.Right)
                    {
                        arrangeRect.X = remainingRect.Right - childSize.Width;
                        arrangeRect.Width = childSize.Width;
                        remainingRect.Width = Math.Max(0, remainingRect.Width - childSize.Width);
                    }
                    else if (dockRegion == NativeDockRegion.RightOverlay)
                    {
                        arrangeRect.X = remainingRect.Right - childSize.Width;
                        arrangeRect.Width = childSize.Width;
                    }
                    else if (dockRegion == NativeDockRegion.Bottom)
                    {
                        arrangeRect.Y = remainingRect.Bottom - childSize.Height;
                        arrangeRect.Height = childSize.Height;
                        remainingRect.Height = Math.Max(0, remainingRect.Height - childSize.Height);
                    }
                    else if (dockRegion == NativeDockRegion.BottomOverlay)
                    {
                        arrangeRect.Y = remainingRect.Bottom - childSize.Height;
                        arrangeRect.Height = childSize.Height;
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported dock region.");
                    }

                    child.Arrange(arrangeRect);
                }
            }

            // Arrange the center elements now that the center rectangle has been calculated
            foreach (var element in centerElements)
            {
                element.Arrange(remainingRect);
            }

            return arrangeSize;
        }
    }
}
