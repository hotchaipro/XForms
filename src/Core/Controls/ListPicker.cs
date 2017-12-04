using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public interface IListPickerRenderer : IControlRenderer
    {
        int Count { get; }

        void AddItem(ListPickerItem item);

        void Clear();

        int SelectedIndex { get; set; }

        ListPickerItem SelectedValue { get; set; }
    }

    public interface IListPickerDelegate
    {
        void NotifyItemSelected(ListPickerItem item);
    }

    public class ListPicker : Control, IListPickerDelegate
    {
        public event EventHandler<ListPickerItem> SelectionChanged;

        public ListPicker()
        {
            this.Items = new ListPickerItemCollection(this);
        }

        public new IListPickerRenderer Renderer
        {
            get
            {
                return (IListPickerRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateListPickerRenderer(this);
        }

        public int SelectedIndex
        {
            get
            {
                return this.Renderer.SelectedIndex;
            }

            set
            {
                this.Renderer.SelectedIndex = value;
            }
        }

        public ListPickerItem SelectedValue
        {
            get
            {
                return this.Renderer.SelectedValue;
            }

            set
            {
                this.Renderer.SelectedValue = value;
            }
        }

        public ListPickerItemCollection Items
        {
            get;
        }

        internal int Count
        {
            get
            {
                return this.Renderer.Count;
            }
        }

        internal void Add(
            ListPickerItem item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this.Renderer.AddItem(item);
        }

        internal void Clear()
        {
            this.Renderer.Clear();
        }

        void IListPickerDelegate.NotifyItemSelected(
            ListPickerItem item)
        {
            this.SelectionChanged?.Invoke(this, item);
        }
    }
}
