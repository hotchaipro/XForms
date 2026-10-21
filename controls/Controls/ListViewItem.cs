using System;
using XForms.Input;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A list's container for one item: it draws the row's background and
    /// selection, and handles taps, which click and select the item. Control
    /// libraries derive from it (and override
    /// <see cref="ListView.CreateItemContainer"/>) to add behavior such as
    /// swipe gestures.
    /// </summary>
    /// <remarks>
    /// The container, as WinUI's and Avalonia's <c>ListViewItem</c>.
    /// </remarks>
    public class ListViewItem : ItemContainer, ITapGestureDelegate
    {
        private bool _isSelected;

        public ListViewItem()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));

            // The container draws the row's background, beneath its children
            this.DrawsContent = true;
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ListItem;
            }
        }

        public override bool AccessibilityIsSelected
        {
            get
            {
                return this.IsSelected;
            }
        }

        /// <summary>
        /// The container of a view that shows a list item (the view, or one of
        /// its ancestors, is a container's content), or null.
        /// </summary>
        public static new ListViewItem ContainerFromContent(
            View view)
        {
            return ItemContainer.ContainerFromContent(view) as ListViewItem;
        }

        /// <summary>
        /// The list the container is showing an item for.
        /// </summary>
        public ListView ListView
        {
            get
            {
                return this.ItemsView as ListView;
            }
        }

        /// <summary>
        /// Whether the container's item is selected. The list sets it.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }

            internal set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    this.InvalidateVisual();
                    this.OnIsSelectedChanged();
                    this.NotifyAccessibilityStateChanged();
                }
            }
        }

        /// <summary>
        /// Called when the item is selected or unselected.
        /// </summary>
        protected virtual void OnIsSelectedChanged()
        {
        }

        /// <summary>
        /// Called when the container is tapped. By default it clicks the item,
        /// and selects it as the list's <see cref="ListView.SelectionMode"/>
        /// says.
        /// </summary>
        protected virtual void OnTapped()
        {
            this.ListView?.OnItemTapped(this);
        }

        // The row's background, tinted with the accent while selected
        protected override void Render(
            DrawingContext drawingContext)
        {
            var colors = this.ActualTheme.Colors;
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            drawingContext.FillRectangle(bounds, colors.Background);
            if (this._isSelected)
            {
                drawingContext.FillRectangle(bounds, colors.Accent.MultiplyAlpha(0.18f));
            }
        }

        void ITapGestureDelegate.OnTapBegan()
        {
        }

        void ITapGestureDelegate.OnTapEnded()
        {
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.OnTapped();
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }
    }
}
