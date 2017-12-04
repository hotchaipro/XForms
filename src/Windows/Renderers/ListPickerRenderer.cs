using System;
using global::Windows.UI.Text;
using global::Windows.UI.Xaml.Controls;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class ListPickerRenderer : ControlRenderer, IListPickerRenderer
    {
        private ComboBox _comboBox;

        public ListPickerRenderer(
            ListPicker listPicker)
            : base(listPicker)
        {
            this._comboBox = new ComboBox()
            {
                FontWeight = FontWeights.SemiLight,
            };
            this._comboBox.SelectionChanged += ComboBox_SelectionChanged;

            this.SetNativeElement(this._comboBox);
        }

        public int Count
        {
            get
            {
                return this._comboBox.Items.Count;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this._comboBox.SelectedIndex;
            }

            set
            {
                this._comboBox.SelectedIndex = value;
            }
        }

        public ListPickerItem SelectedValue
        {
            get
            {
                return this._comboBox.SelectedValue as ListPickerItem;
            }

            set
            {
                this._comboBox.SelectedValue = value;
            }
        }

        public void AddItem(
            ListPickerItem item)
        {
            this._comboBox.Items.Add(item);
        }

        public void Clear()
        {
            this._comboBox.Items.Clear();
        }

        private void ComboBox_SelectionChanged(
            object sender,
            global::Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as ListPickerItem;
                if (null != item)
                {
                    ((IListPickerDelegate)this.Element).NotifyItemSelected(item);
                }
            }
        }
    }
}
