using System;
using System.Collections.Specialized;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using XForms.Primitives;
using XForms.Platform;
using AndroidListView = global::Android.Widget.ListView;
using AndroidView = global::Android.Views.View;

namespace XForms.Android.Renderers
{
    /// <summary>
    /// A list in a pull-to-refresh layout: Android's <c>ListView</c> for a
    /// vertical list, and a <c>RecyclerView</c> for a horizontal list or a
    /// grid.
    /// </summary>
    public class ItemsViewRenderer : ViewRenderer<SwipeRefreshLayout>, IItemsViewRenderer
    {
        private readonly SwipeRefreshLayout _refreshLayout;

        // NOTE: The refresh layout keeps its first child as its target, so the
        // lists come and go inside this frame instead
        private readonly global::Android.Widget.FrameLayout _listFrame;
        private AndroidListView _nativeListView;
        private RecyclerView _recyclerView;
        private ListLayout _layout = ListLayout.Vertical;
        private ItemsViewSource _source;
        private float _footerHeight;

        public ItemsViewRenderer(
            global::Android.Content.Context context,
            ItemsView listView)
            : base(context, listView)
        {
            this._refreshLayout = new SwipeRefreshLayout(context)
            {
                Enabled = false,
            };
            this._refreshLayout.Refresh += (sender, e) => ((ItemsView)this.Element).NotifyRefreshRequested();
            this._refreshLayout.SetOnChildScrollUpCallback(new ScrollUpCallback(this));

            this._listFrame = new global::Android.Widget.FrameLayout(context);
            this._refreshLayout.AddView(this._listFrame);

            this.CreateList();
            this.SetNativeElement(this._refreshLayout);
        }

        public float FooterHeight
        {
            get
            {
                return this._footerHeight;
            }

            set
            {
                this._footerHeight = value;
                var list = this.List;
                list.SetPadding(list.PaddingLeft, list.PaddingTop, list.PaddingRight, (int)this.DevicePixelsToPixels(value));
            }
        }

        public ListLayout Layout
        {
            set
            {
                if (value == this._layout)
                {
                    return;
                }

                bool wasVertical = this.IsVertical;
                this._layout = value;
                if (wasVertical != this.IsVertical)
                {
                    this.CreateList();
                }
                else if (null != this._recyclerView)
                {
                    this._recyclerView.SetLayoutManager(this.CreateLayoutManager());
                }

                this.Bind(this._source);
            }
        }

        public bool IsRefreshEnabled
        {
            set
            {
                this._refreshLayout.Enabled = value;
            }
        }

        public bool IsRefreshing
        {
            set
            {
                this._refreshLayout.Refreshing = value;
            }
        }

        private bool IsVertical
        {
            get
            {
                return (this._layout.Orientation == Orientation.Vertical) && !this._layout.IsGrid;
            }
        }

        private global::Android.Views.ViewGroup List
        {
            get
            {
                return (global::Android.Views.ViewGroup)this._nativeListView ?? this._recyclerView;
            }
        }

        // The native list for the layout, in the refresh layout
        private void CreateList()
        {
            this._listFrame.RemoveAllViews();
            if (null != this._nativeListView)
            {
                var oldAdapter = this._nativeListView.Adapter;
                this._nativeListView.Adapter = null;
                oldAdapter?.Dispose();
            }

            this._nativeListView = null;
            this._recyclerView = null;

            global::Android.Views.ViewGroup list;
            if (this.IsVertical)
            {
                this._nativeListView = new AndroidListView(this.NativeContext)
                {
                    ChoiceMode = global::Android.Widget.ChoiceMode.Single,
                    Divider = null,
                    DividerHeight = 0,
                };
                this._nativeListView.SetSelector(global::Android.Resource.Color.Transparent);
                list = this._nativeListView;
            }
            else
            {
                this._recyclerView = new RecyclerView(this.NativeContext);
                this._recyclerView.SetLayoutManager(this.CreateLayoutManager());
                list = this._recyclerView;
            }

            list.SetClipToPadding(false);
            this._listFrame.AddView(list);
            this.FooterHeight = this._footerHeight;
        }

        private RecyclerView.LayoutManager CreateLayoutManager()
        {
            if (this._layout.IsGrid)
            {
                return new GridLayoutManager(this.NativeContext, this._layout.Columns);
            }

            return new LinearLayoutManager(this.NativeContext, (this._layout.Orientation == Orientation.Horizontal) ? LinearLayoutManager.Horizontal : LinearLayoutManager.Vertical, false);
        }

        public void Bind(
            ItemsViewSource source)
        {
            this._source = source;
            var listView = (ItemsView)this.Element;

            if (null != this._nativeListView)
            {
                // NOTE: The list calls its adapter as it changes; dispose of the
                // old one only after
                var oldAdapter = this._nativeListView.Adapter;
                this._nativeListView.Adapter = (null == source) ? null : new NativeListViewAdapter(listView, this.NativeContext, source);
                oldAdapter?.Dispose();
            }
            else
            {
                (this._recyclerView.GetAdapter() as RecyclerAdapter)?.Detach();
                this._recyclerView.SetAdapter((null == source) ? null : new RecyclerAdapter(listView, source, this._layout));
            }
        }

        public void ScrollToItem(
            object item)
        {
            int position = ((null == item) || (null == this._source)) ? -1 : this._source.IndexOf(item);
            if (position < 0)
            {
                return;
            }

            if (null != this._nativeListView)
            {
                this._nativeListView.SmoothScrollToPosition(position);
            }
            else
            {
                this._recyclerView.SmoothScrollToPosition(position);
            }
        }

        // A pull refreshes only when the list is at its top
        private sealed class ScrollUpCallback : Java.Lang.Object, SwipeRefreshLayout.IOnChildScrollUpCallback
        {
            private readonly ItemsViewRenderer _renderer;

            public ScrollUpCallback(
                ItemsViewRenderer renderer)
            {
                this._renderer = renderer;
            }

            public bool CanChildScrollUp(
                SwipeRefreshLayout parent,
                AndroidView child)
            {
                return this._renderer.List?.CanScrollVertically(-1) ?? false;
            }
        }

        /// <summary>
        /// Shows the source's items in a RecyclerView, in the same native
        /// containers as the vertical list.
        /// </summary>
        private sealed class RecyclerAdapter : RecyclerView.Adapter
        {
            private readonly ItemsView _listView;
            private readonly ItemsViewSource _source;
            private readonly ListLayout _layout;

            public RecyclerAdapter(
                ItemsView listView,
                ItemsViewSource source,
                ListLayout layout)
            {
                this._listView = listView;
                this._source = source;
                this._layout = layout;
                source.CollectionChanged += this.Source_CollectionChanged;
            }

            public void Detach()
            {
                this._source.CollectionChanged -= this.Source_CollectionChanged;
            }

            private void Source_CollectionChanged(
                object sender,
                NotifyCollectionChangedEventArgs e)
            {
                this.NotifyDataSetChanged();
            }

            public override int ItemCount
            {
                get
                {
                    return this._source.Count;
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(
                global::Android.Views.ViewGroup parent,
                int viewType)
            {
                var container = new NativeListViewItemContainer(parent.Context);

                // A horizontal list's items are as wide as their content, and as
                // tall as the list; a grid's fill their column
                bool isHorizontal = this._layout.Orientation == Orientation.Horizontal;
                container.LayoutParameters = new RecyclerView.LayoutParams(
                    isHorizontal ? global::Android.Views.ViewGroup.LayoutParams.WrapContent : global::Android.Views.ViewGroup.LayoutParams.MatchParent,
                    isHorizontal ? global::Android.Views.ViewGroup.LayoutParams.MatchParent : global::Android.Views.ViewGroup.LayoutParams.WrapContent);
                return new Holder(container);
            }

            public override void OnBindViewHolder(
                RecyclerView.ViewHolder holder,
                int position)
            {
                var container = ((Holder)holder).Container;
                object item = this._source[position];
                container.Item = item;
                container.ItemContainer = this._source.GetItemContainer(this._listView, container.ItemContainer, item);
            }

            public override void OnViewRecycled(
                Java.Lang.Object holder)
            {
                base.OnViewRecycled(holder);
                ((Holder)holder).Container.ItemContainer?.Recycle();
            }

            private sealed class Holder : RecyclerView.ViewHolder
            {
                public Holder(
                    NativeListViewItemContainer container)
                    : base(container)
                {
                    this.Container = container;
                }

                public NativeListViewItemContainer Container { get; }
            }
        }
    }
}
