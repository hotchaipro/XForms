using System;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    /// <summary>
    /// Holds one item's view in a <see cref="ListView"/>. The platform list
    /// recycles containers and animates a container's height when it changes,
    /// so a container can reveal more content (such as commands) by showing it.
    /// Tapping a container selects its item, or clears the selection when it
    /// is already selected. Control libraries derive from this class (and
    /// override <see cref="ListView.CreateItemContainer"/>) to add behavior such
    /// as swipe gestures.
    /// </summary>
    public class ListViewItemContainer : DockPanel, ITapGestureDelegate
    {
        private ListViewItem _content;

        public ListViewItemContainer()
        {
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ListItem;
            }
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
        /// The item's view. The container keeps the same view as it is recycled.
        /// </summary>
        public ListViewItem Content
        {
            get
            {
                return this._content;
            }

            internal set
            {
                if (null != this._content)
                {
                    if (this._content != value)
                    {
                        throw new NotImplementedException("ListView content must be recycled.");
                    }
                }
                else
                {
                    this._content = value;
                    this.AddContent(value);
                }

                value.Container = this;
            }
        }

        /// <summary>
        /// Places the item's view in the container. By default it fills the container.
        /// </summary>
        protected virtual void AddContent(
            ListViewItem content)
        {
            this.Children.Add(content, Dock.CenterOverlay);
        }

        internal void Bind(
            ListView listView,
            object item)
        {
            this.ListView = listView ?? throw new ArgumentNullException(nameof(listView));
            listView.AddRealizedContainer(this);
            this.Item = item;
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
            var content = this.Content;
            if (null != content)
            {
                content.Item = null;
                content.Container = null;
            }

            this.OnRecycle();
        }

        /// <summary>
        /// Called when the container is set aside, to reset any per-item state.
        /// </summary>
        protected virtual void OnRecycle()
        {
        }

        /// <summary>
        /// Called when the container is tapped. By default this toggles the item's selection.
        /// </summary>
        protected virtual void OnTapped()
        {
            var listView = this.ListView;
            if (null == listView)
            {
                return;
            }

            bool isSelected = listView.SelectedItemContainer != this;
            listView.SelectedItemContainer = isSelected ? this : null;
            listView.NotifyItemClicked(this.Item, this.Content, isSelected);
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
