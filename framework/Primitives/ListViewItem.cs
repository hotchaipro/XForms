using System;
using XForms.Input;

namespace XForms.Primitives
{
    /// <summary>
    /// A list's container for one item: it holds the item's view (any view),
    /// draws the row's background and selection, and handles taps. The
    /// platform list recycles containers and animates a container's height
    /// when it changes, so a container can reveal more content (such as
    /// commands) by showing it. Control libraries derive from it (and override
    /// <see cref="ListView.CreateItemContainer"/>) to add behavior such as
    /// swipe gestures.
    /// </summary>
    /// <remarks>
    /// The container, as WinUI's and Avalonia's <c>ListViewItem</c>.
    /// </remarks>
    public class ListViewItem : View, ITapGestureDelegate
    {
        private View _content;
        private bool _isSelected;

        public ListViewItem()
        {
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.GestureRecognizer = new TapGestureRecognizer(this);

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
        public static ListViewItem ContainerFromContent(
            View view)
        {
            for (var element = view?.Parent; null != element; element = (element as View)?.Parent)
            {
                if (element is ListViewItem container)
                {
                    return container;
                }
            }

            return null;
        }

        /// <summary>
        /// The list the container is showing an item for.
        /// </summary>
        public ListView ListView
        {
            get;
            private set;
        }

        /// <summary>
        /// The item the container is showing.
        /// </summary>
        public object Item
        {
            get;
            private set;
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
        /// The item's view. The container keeps its view as it's recycled,
        /// unless the list needs another kind of view (a group header, say).
        /// </summary>
        public View Content
        {
            get
            {
                return this._content;
            }

            internal set
            {
                if (value == this._content)
                {
                    return;
                }

                if (null != this._content)
                {
                    this.RemoveContent(this._content);
                }

                this._content = value;
                if (null != value)
                {
                    this.AddContent(value);
                }
            }
        }

        /// <summary>
        /// Places the item's view in the container. By default it fills the
        /// container (children overlap, each in the whole area).
        /// </summary>
        protected virtual void AddContent(
            View content)
        {
            this.AddVisualChild(content);
        }

        /// <summary>
        /// Takes the item's view out of the container.
        /// </summary>
        protected virtual void RemoveContent(
            View content)
        {
            this.RemoveVisualChild(content);
        }

        internal void Bind(
            ListView listView,
            object item)
        {
            this.ListView = listView ?? throw new ArgumentNullException(nameof(listView));
            listView.AddRealizedContainer(this);
            this.Item = item;
            this.IsSelected = listView.IsSelected(item);
            this.OnBind();
        }

        // A row isn't attached to its list, but looks like it
        internal override Element ParentElement
        {
            get
            {
                return this.ListView;
            }
        }

        /// <summary>
        /// Called when the container starts showing an item (it may be recycled).
        /// </summary>
        protected virtual void OnBind()
        {
        }

        /// <summary>
        /// Called by the platform list when the container scrolls out of view and
        /// is set aside to show another item.
        /// </summary>
        public void Recycle()
        {
            this.OnRecycle();
        }

        /// <summary>
        /// Called when the container is set aside, to reset any per-item state.
        /// </summary>
        protected virtual void OnRecycle()
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
    }
}
