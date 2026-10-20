using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    public interface IListViewRenderer : IElementRenderer
    {
        float FooterHeight { get; set; }

        void Bind(ListViewSource source);

        void ScrollToItem(object item);
    }

    public class ItemClickedEventArgs : EventArgs
    {
        public ItemClickedEventArgs(
            object item,
            View view)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (null == view)
            {
                throw new ArgumentNullException(nameof(view));
            }

            this.Item = item;
            this.View = view;
        }

        public object Item
        {
            get;
            private set;
        }

        public View View
        {
            get;
            private set;
        }
    }


    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(
            object oldItem,
            object newItem)
        {
            this.OldItem = oldItem;
            this.NewItem = newItem;
        }

        public object OldItem
        {
            get;
        }

        public object NewItem
        {
            get;
        }
    }

    public class ListView : Control
    {
        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.List;
            }
        }

        private object _selectedItem;

        public event EventHandler<ItemClickedEventArgs> ItemClicked;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public ListView()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private float? _footerHeight;
        private ListViewSource _source;
        private object _pendingScrollItem;

        internal new IListViewRenderer Renderer
        {
            get
            {
                return (IListViewRenderer)base.Renderer;
            }
        }

        private IListViewRenderer Native
        {
            get
            {
                return (IListViewRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._footerHeight.HasValue)
            {
                native.FooterHeight = this._footerHeight.Value;
            }

            if (null != this._source)
            {
                native.Bind(this._source);
            }

            if (null != this._pendingScrollItem)
            {
                native.ScrollToItem(this._pendingScrollItem);
                this._pendingScrollItem = null;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Theme.SubtleBackgroundColor;
            }
        }

        public float FooterHeight
        {
            get
            {
                return this._footerHeight ?? this.Native?.FooterHeight ?? 0;
            }

            set
            {
                this._footerHeight = value;
                this.Native?.FooterHeight = value;
            }
        }

        /// <summary>
        /// Creates the container for an item's view. Control libraries override
        /// this to add behavior to every item, such as swipe gestures.
        /// </summary>
        public virtual ListViewItemContainer CreateItemContainer()
        {
            return new ListViewItemContainer();
        }

        public void Bind(
            ListViewSource source)
        {
            this._source = source;
            this.Native?.Bind(source);
        }

        public void ScrollToItem(
            object item)
        {
            var native = this.Native;
            if (null != native)
            {
                native.ScrollToItem(item);
            }
            else
            {
                this._pendingScrollItem = item;
            }
        }

        public void ClearSelection()
        {
            var selectedItem = this._selectedItem;
            if (null != selectedItem)
            {
                SelectionChangedEventArgs selectionChangedEventArgs = new SelectionChangedEventArgs(selectedItem, null);
                this._selectedItem = null;

                this.SelectionChanged?.Invoke(this, selectionChangedEventArgs);
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateListViewRenderer(this);
        }

        /// <summary>
        /// Raises <see cref="ItemClicked"/> and <see cref="SelectionChanged"/>
        /// for a tapped item; called by item containers.
        /// </summary>
        public void NotifyItemClicked(
            object item,
            View itemView,
            bool isSelected)
        {
            var itemClickedEventArgs = new ItemClickedEventArgs(item, itemView);
            this.ItemClicked?.Invoke(this, itemClickedEventArgs);

            SelectionChangedEventArgs selectionChangedEventArgs;
            if (isSelected)
            {
                selectionChangedEventArgs = new SelectionChangedEventArgs(this._selectedItem, item);
                this._selectedItem = item;
            }
            else
            {
                selectionChangedEventArgs = new SelectionChangedEventArgs(this._selectedItem, null);
                this._selectedItem = null;
            }

            this.SelectionChanged?.Invoke(this, selectionChangedEventArgs);
        }

        /// <summary>
        /// The container of the selected item, if it is in view.
        /// </summary>
        public ListViewItemContainer SelectedItemContainer
        {
            get;
            set;
        }

        //internal override IEnumerable<Element> GetChildElements()
        //{
        //    //TODO: Return list items and containers
        //}
    }
}
