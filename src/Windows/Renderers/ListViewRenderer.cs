using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Input;
using XForms.Controls;
using XamlScrollViewer = global::Windows.UI.Xaml.Controls.ScrollViewer;

namespace XForms.Windows.Renderers
{
    public class ListViewRenderer : ControlRenderer, IListViewRenderer
    {
        private CustomListView _nativeListView;

        public ListViewRenderer(
            XForms.Controls.ListView listView)
            : base(listView)
        {
            this._nativeListView = new CustomListView(listView);
            this._nativeListView.IsSynchronizedWithCurrentItem = false;
            this._nativeListView.SelectionMode = global::Windows.UI.Xaml.Controls.ListViewSelectionMode.None;

            var itemTemplate = (DataTemplate)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewItemTemplate"];
            this._nativeListView.ItemTemplate = itemTemplate;

            var itemStyle = (Style)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewItemContainerStyle"];
            this._nativeListView.ItemContainerStyle = itemStyle;

            //var listStyle = (Style)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewStyle"];
            //this._nativeListView.Style = listStyle;

            //this._nativeListView.IsItemClickEnabled = true;
            //this._nativeListView.ItemClick += NativeListView_ItemClick;

            this.SetNativeElement(this._nativeListView);
        }

        public float FooterHeight
        {
            get
            {
                return this._nativeListView.FooterHeight;
            }

            set
            {
                this._nativeListView.FooterHeight = value;
            }
        }

        //private void NativeListView_ItemClick(
        //    object sender,
        //    ItemClickEventArgs e)
        //{
        //    if (null == e)
        //    {
        //        return;
        //    }

        //    var item = e.ClickedItem as CustomItem;
        //    if (null == item)
        //    {
        //        return;
        //    }

        //    ((IListViewDelegate)this.Element).NotifyItemClicked(null, null, item.Item, item.Container?.View);
        //}

        public void Bind(
            ListViewSource source)
        {
            this._nativeListView.Bind(source);
        }

        public void ScrollToItem(
            object item)
        {
            if (null == item)
            {
                return;
            }

            this._nativeListView.UpdateLayout();

            this._nativeListView.ScrollIntoView(item, global::Windows.UI.Xaml.Controls.ScrollIntoViewAlignment.Default);
        }
    }

    internal sealed class CustomListViewSource : IList, INotifyCollectionChanged
    {
        private ListViewSource _source;
        private Dictionary<object, CustomItem> _customItemMap;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public CustomListViewSource(
            ListViewSource source)
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this._source = source;
            this._customItemMap = new Dictionary<object, CustomItem>();
            this._source.CollectionChanged += Source_CollectionChanged;
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs changedArgs;
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newItems = MapCustomItems(e.NewItems);
                if (e.NewStartingIndex < 0)
                {
                    changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems);
                }
                else
                {
                    changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var oldItems = MapCustomItems(e.OldItems);
                if (e.OldStartingIndex < 0)
                {
                    changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems);
                }
                else
                {
                    changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var oldItems = MapCustomItems(e.OldItems);
                changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItems, e.NewStartingIndex, e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var oldItems = MapCustomItems(e.OldItems);
                var newItems = MapCustomItems(e.NewItems);
                changedArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, e.NewStartingIndex);
            }
            else
            {
                throw new NotSupportedException("Unsupported collection changed action.");
            }

            // Verify the expected results
            if ((e.Action != changedArgs.Action)
                || ((e.NewItems == null) && (changedArgs.NewItems != null))
                || ((e.NewItems != null) && ((changedArgs.NewItems == null) || (e.NewItems.Count != changedArgs.NewItems.Count)))
                || (e.NewStartingIndex != changedArgs.NewStartingIndex)
                || ((e.OldItems == null) && (changedArgs.OldItems != null))
                || ((e.OldItems != null) && ((changedArgs.OldItems == null) || (e.OldItems.Count != changedArgs.OldItems.Count)))
                || (e.OldStartingIndex != changedArgs.OldStartingIndex))
            {
                throw new InvalidOperationException("Invalid collection changed notification.");
            }

            this.CollectionChanged?.Invoke(sender, changedArgs);
        }

        private IList<CustomItem> MapCustomItems(
            IList changeList)
        {
            if (changeList == null)
            {
                return null;
            }

            List<CustomItem> list = new List<CustomItem>(changeList.Count);

            foreach (object item in changeList)
            {
                CustomItem customItem = GetItem(item);
                list.Add(customItem);
            }

            return list;
        }

        public object this[int index]
        {
            get
            {
                object item = this._source[index];
                CustomItem customItem = GetItem(item);
                return customItem;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        internal CustomItem GetItem(
            object item)
        {
            CustomItem customItem;
            if (!this._customItemMap.TryGetValue(item, out customItem))
            {
                customItem = new CustomItem(item);
                this._customItemMap.Add(item, customItem);
            }
            return customItem;
        }

        public int Count
        {
            get
            {
                return this._source.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Add(
            object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(
            object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            //return new ListViewSourceEnumerator(this);
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            // NOTE: Used for ScrollItemIntoView on Windows UWP ListView
            return this._source.IndexOf(((CustomItem)value).Item);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        //private sealed class ListViewSourceEnumerator : IEnumerator
        //{
        //    private CustomListViewSource _source;
        //    private int _index;

        //    internal ListViewSourceEnumerator(
        //        CustomListViewSource source)
        //    {
        //        if (null == source)
        //        {
        //            throw new ArgumentNullException(nameof(source));
        //        }

        //        this._source = source;
        //        this._index = -1;
        //    }

        //    public object Current
        //    {
        //        get
        //        {
        //            if (this._index < 0)
        //            {
        //                throw new InvalidOperationException("MoveNext must be called before Current.");
        //            }

        //            if (this._index >= this._source.Count)
        //            {
        //                throw new InvalidOperationException("Past the end of the enumeration.");
        //            }

        //            return this._source[this._index];
        //        }
        //    }

        //    public bool MoveNext()
        //    {
        //        this._index += 1;

        //        if (this._index >= this._source.Count)
        //        {
        //            this._index = this._source.Count;

        //            return false;
        //        }

        //        return true;
        //    }

        //    public void Reset()
        //    {
        //        this._index = -1;
        //    }
        //}
    }

    internal sealed class CustomItem
    {
        public CustomItem(
            object item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.Item = item;
        }

        public object Item
        {
            get;
            private set;
        }

        public CustomItemContainer Container
        {
            get;
            set;
        }

        /// <summary>
        /// Referenced by the XAML DataTemplate resource binding.
        /// </summary>
        public object NativeView
        {
            get
            {
                return this.Container?.ItemContainer?.Renderer.NativeElement;
            }
        }
    }

    internal sealed class CustomItemContainer : global::Windows.UI.Xaml.Controls.ListViewItem
    {
        private CustomListView _parent;
        private bool _pointerCaptured;

        public CustomItemContainer(
            CustomListView parent)
        {
            if (null == parent)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this._parent = parent;

            // NOTE: This enables swiping with touch. For some reason, swiping only works with 
            // the mouse pointer until this parameter is set.
            this.ManipulationMode = ManipulationModes.System | ManipulationModes.TranslateX;

            // NOTE: This allows the list item row height to match the height of the actual content in the row
            this.MinHeight = 10;
        }

        public object Item
        {
            get;

            set;
        }

        public ListViewItemContainer ItemContainer
        {
            get;

            set;
        }

        public void Recycle()
        {
            this.ReleasePointerCaptures();

            this._pointerCaptured = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                itemContainer.Recycle();
            }
        }

        #region Input

        protected override void OnPointerPressed(
            PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            bool result = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                var position = e.GetCurrentPoint(this).Position;
                Point point = new Point((float)position.X, (float)position.Y);

                var pointerEvent = new PointerInputEvent(point, PointerInputState.Began, e.Pointer.IsInContact);

                result = itemContainer.HandlePointerEvent(pointerEvent);

                if (result)
                {
                    this._pointerCaptured = this.CapturePointer(e.Pointer);
                }
            }

            e.Handled = result;
        }

        protected override void OnPointerMoved(
            PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);

            bool result = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                var position = e.GetCurrentPoint(this).Position;
                Point point = new Point((float)position.X, (float)position.Y);

                var pointerEvent = new PointerInputEvent(point, PointerInputState.Moved, e.Pointer.IsInContact);

                result = itemContainer.HandlePointerEvent(pointerEvent);
            }

            e.Handled = result;
        }

        protected override void OnPointerReleased(
            PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            bool result = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                var position = e.GetCurrentPoint(this).Position;
                Point point = new Point((float)position.X, (float)position.Y);

                var pointerEvent = new PointerInputEvent(point, PointerInputState.Ended, e.Pointer.IsInContact);

                result = itemContainer.HandlePointerEvent(pointerEvent);
            }

            if (this._pointerCaptured)
            {
                this.ReleasePointerCapture(e.Pointer);
            }

            e.Handled = result;
        }

        protected override void OnPointerCanceled(
            PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);

            bool result = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                var position = e.GetCurrentPoint(this).Position;
                Point point = new Point((float)position.X, (float)position.Y);

                var pointerEvent = new PointerInputEvent(point, PointerInputState.Canceled, e.Pointer.IsInContact);

                result = itemContainer.HandlePointerEvent(pointerEvent);
            }

            if (this._pointerCaptured)
            {
                this.ReleasePointerCapture(e.Pointer);
            }

            e.Handled = result;
        }

        protected override void OnPointerCaptureLost(
            PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            bool result = false;

            var itemContainer = this.ItemContainer;
            if (null != itemContainer)
            {
                var position = e.GetCurrentPoint(this).Position;
                Point point = new Point((float)position.X, (float)position.Y);

                var pointerEvent = new PointerInputEvent(point, PointerInputState.Canceled, e.Pointer.IsInContact);

                result = itemContainer.HandlePointerEvent(pointerEvent);
            }

            if (this._pointerCaptured)
            {
                this.ReleasePointerCapture(e.Pointer);
            }

            e.Handled = result;
        }

        #endregion Input
    }

    internal sealed class CustomListView : global::Windows.UI.Xaml.Controls.ListView
    {
        private ListView _listView;
        private ListViewSource _source;
        private CustomListViewSource _customSource;
        private float _footerHeight;

        public CustomListView(
            ListView listView)
        {
            if (null == listView)
            {
                throw new ArgumentNullException(nameof(listView));
            }

            this._listView = listView;
        }

        //protected override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    var scrollViewer = GetScrollViewer(this);
        //    //scrollViewer.ManipulationMode = ManipulationModes.None;
        //    //scrollViewer.IsHorizontalScrollChainingEnabled = false;
        //    //scrollViewer.IsHorizontalRailEnabled = false;
        //}

        public float FooterHeight
        {
            get
            {
                return this._footerHeight;
            }

            set
            {
                this._footerHeight = value;

                this.Footer = new global::Windows.UI.Xaml.Controls.Border()
                {
                    BorderThickness = new global::Windows.UI.Xaml.Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = value,
                };
            }
        }

        public void Bind(
            ListViewSource source)
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this._source = source;
            this._customSource = new CustomListViewSource(source);
            this.ItemsSource = this._customSource;
        }

        protected override bool IsItemItsOwnContainerOverride(
            object item)
        {
            return item is CustomItemContainer;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CustomItemContainer(this);
        }

        protected override void ClearContainerForItemOverride(
            DependencyObject element,
            object item)
        {
            base.ClearContainerForItemOverride(element, item);

            var customContainer = element as CustomItemContainer;
            if (null != customContainer)
            {
                customContainer.Recycle();

                customContainer.Item = null;
            }

            var customItem = item as CustomItem;
            customItem.Container = null;
        }

        protected override void PrepareContainerForItemOverride(
            DependencyObject element,
            object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var customContainer = element as CustomItemContainer;
            if (null != customContainer)
            {
                var customItem = item as CustomItem;
                customItem.Container = customContainer;

                customContainer.Item = customItem.Item;

                var view = this._source.GetItemContainer(this._listView, customContainer.ItemContainer, customItem.Item);
                customContainer.ItemContainer = view;
            }
        }

        public new void ScrollIntoView(
            object item)
        {
            this.ScrollIntoView(item, global::Windows.UI.Xaml.Controls.ScrollIntoViewAlignment.Default);
        }

        public new void ScrollIntoView(
            object item,
            global::Windows.UI.Xaml.Controls.ScrollIntoViewAlignment alignment)
        {
            var nowait = Window.Current.Dispatcher.RunIdleAsync((ignore) =>
            {
                this.EnsureItemIsVisible(item);
            });
        }

        private void EnsureItemIsVisible(
            object item)
        {
            // Get the native list item container
            CustomItem customItem = this._customSource.GetItem(item);
            var selectorItem = this.ContainerFromItem(customItem) as FrameworkElement;

            if (null == selectorItem)
            {
                // Item is not realized in the list view (it is virtualized)
                return;
            }

            // Find the offset of the item from the top of the view
            var selectorItemTop = selectorItem.TransformToVisual(this).TransformPoint(new global::Windows.Foundation.Point(0, 0));

            // Get the height of the item
            var itemHeight = selectorItem.ActualHeight;

            // Determine whether or not scrolling is necessary
            double scrollBy = 0;
            if (selectorItemTop.Y < 0)
            {
                scrollBy = selectorItemTop.Y;
            }
            else if (selectorItemTop.Y + itemHeight > this.ActualHeight - this.FooterHeight)
            {
                scrollBy = selectorItemTop.Y + itemHeight - this.ActualHeight + this.FooterHeight;
            }

            if (scrollBy != 0)
            {
                var scrollViewer = GetScrollViewer(this);
                scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + scrollBy, zoomFactor: null, disableAnimation: false);
            }
        }

        private static XamlScrollViewer GetScrollViewer(
            DependencyObject element)
        {
            if (element is XamlScrollViewer)
            {
                return (XamlScrollViewer)element;
            }

            for (int i = 0; i < global::Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = global::Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }
    }
}
