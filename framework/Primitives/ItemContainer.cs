using System;

namespace XForms.Primitives
{
    /// <summary>
    /// An items view's container for one item: it holds the item's view (any
    /// view). The platform recycles containers and animates a container's
    /// height when it changes, so a container can reveal more content (such as
    /// commands) by showing it. Control libraries derive from it (and override
    /// <see cref="ItemsView.CreateItemContainer"/>) to add behavior such as
    /// selection or swipe gestures.
    /// </summary>
    /// <remarks>
    /// As WinUI's <c>ItemContainer</c>.
    /// </remarks>
    public class ItemContainer : View
    {
        private View _content;

        public ItemContainer()
        {
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        /// <summary>
        /// The container of a view that shows an item (the view, or one of its
        /// ancestors, is a container's content), or null.
        /// </summary>
        public static ItemContainer ContainerFromContent(
            View view)
        {
            for (var element = view?.Parent; null != element; element = (element as View)?.Parent)
            {
                if (element is ItemContainer container)
                {
                    return container;
                }
            }

            return null;
        }

        /// <summary>
        /// The items view the container is showing an item for.
        /// </summary>
        public ItemsView ItemsView
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
        /// The item's view. The container keeps its view as it's recycled,
        /// unless the items view needs another kind of view (a group header,
        /// say).
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
            ItemsView itemsView,
            object item)
        {
            this.ItemsView = itemsView ?? throw new ArgumentNullException(nameof(itemsView));
            itemsView.AddRealizedContainer(this);
            this.Item = item;
            this.IsRecycled = false;
            itemsView.OnContainerBound(this);
            this.OnBind();
        }

        // A container isn't attached to its items view, but looks like it
        internal override Element ParentElement
        {
            get
            {
                return this.ItemsView;
            }
        }

        // A list is one Tab stop: the container of its current item
        internal override bool IsTabStop
        {
            get
            {
                return base.IsTabStop && (this.ItemsView?.TabStopContainer == this);
            }
        }

        // Focus (from a tap, Tab or the arrow keys) makes the item current
        internal override void OnFocusChangedCore(
            bool isFocused)
        {
            base.OnFocusChangedCore(isFocused);
            if (isFocused)
            {
                this.ItemsView?.OnContainerFocused(this);
            }
        }

        /// <summary>
        /// Called when the container starts showing an item (it may be recycled).
        /// </summary>
        protected virtual void OnBind()
        {
        }

        /// <summary>
        /// Called by the platform when the container scrolls out of view and is
        /// set aside to show another item.
        /// </summary>
        public void Recycle()
        {
            this.IsRecycled = true;
            this.OnRecycle();
        }

        /// <summary>
        /// Whether the container is set aside (out of view) until it's bound
        /// to an item again.
        /// </summary>
        internal bool IsRecycled
        {
            get;
            private set;
        }

        /// <summary>
        /// Called when the container is set aside, to reset any per-item state.
        /// </summary>
        protected virtual void OnRecycle()
        {
        }
    }
}
