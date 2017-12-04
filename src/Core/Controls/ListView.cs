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

    public class SwipeCommandCollection
    {
        private List<UICommand> _primaryCommands;
        private List<UICommand> _secondaryCommands;

        public SwipeCommandCollection()
        {
            this._primaryCommands = new List<UICommand>();
            this._secondaryCommands = new List<UICommand>();
        }

        public List<UICommand> PrimaryCommands
        {
            get
            {
                return this._primaryCommands;
            }
        }

        public List<UICommand> SecondaryCommands
        {
            get
            {
                return this._secondaryCommands;
            }
        }

        public bool IsLongSwipeRequiredForPrimaryCommands
        {
            get;
            set;
        }

        public bool IsLongSwipeRequiredForSecondaryCommands
        {
            get;
            set;
        }
    }

    public class ListView : Control
    {
        private object _selectedItem;

        public event EventHandler<ItemClickedEventArgs> ItemClicked;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public ListView()
        {
            this.CommandBarCommands = new CommandBarCommandCollection();
            this.SwipeCommands = new SwipeCommandCollection();

            this.HorizontalAlignment = LayoutAlignment.Fill;
            this.VerticalAlignment = LayoutAlignment.Fill;
        }

        public new IListViewRenderer Renderer
        {
            get
            {
                return (IListViewRenderer)base.Renderer;
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
                return this.Renderer.FooterHeight;
            }

            set
            {
                this.Renderer.FooterHeight = value;
            }
        }

        public CommandBarCommandCollection CommandBarCommands
        {
            get;
        }

        public SwipeCommandCollection SwipeCommands
        {
            get;
        }

        public void Bind(
            ListViewSource source)
        {
            this.Renderer.Bind(source);
        }

        public void ScrollToItem(
            object item)
        {
            this.Renderer.ScrollToItem(item);
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

        internal void NotifyItemClicked(
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

        internal ListViewItemContainer SelectedItemContainer
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
