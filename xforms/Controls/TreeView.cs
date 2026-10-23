using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    /// <summary>
    /// An item in a tree: its text, the items under it, and whether they
    /// show. The app's data, which the tree view shows.
    /// </summary>
    public sealed class TreeViewNode
    {
        private bool _isExpanded;

        public TreeViewNode()
        {
        }

        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Anything the app keeps with the node.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        public List<TreeViewNode> Children
        {
            get;
        } = new List<TreeViewNode>();

        public bool HasChildren
        {
            get
            {
                return this.Children.Count > 0;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this._isExpanded;
            }

            set
            {
                if (value != this._isExpanded)
                {
                    this._isExpanded = value;
                    this.ExpandedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        internal event EventHandler ExpandedChanged;
    }

    /// <summary>
    /// The node chosen in a tree view.
    /// </summary>
    public sealed class TreeViewItemInvokedEventArgs : EventArgs
    {
        public TreeViewItemInvokedEventArgs(
            TreeViewNode node)
        {
            this.Node = node;
        }

        public TreeViewNode Node
        {
            get;
        }
    }

    /// <summary>
    /// Nested items that expand and collapse, one selected: a tap selects an
    /// item (and on its chevron, expands or collapses it); with keyboard
    /// focus, Up and Down move between the items shown, Right expands an item
    /// (or goes to its first child), Left collapses it (or goes to its
    /// parent), Home and End go to the ends, and Enter or Space selects. Tab
    /// stops once, at the item focus was last on (or the selected one). Its
    /// theme's template draws its items.
    /// </summary>
    public class TreeView : TemplatedControl
    {
        private readonly StackPanel _panel;
        private readonly List<TreeViewItem> _items = new List<TreeViewItem>();
        private TreeViewNode _selectedNode;

        public TreeView()
        {
            this._panel = new StackPanel();
            base.Content = this._panel;
        }

        /// <summary>
        /// Raised when the user chooses an item (a tap, Enter or Space).
        /// </summary>
        public event EventHandler<TreeViewItemInvokedEventArgs> ItemInvoked;

        /// <summary>
        /// The top-level nodes, in order.
        /// </summary>
        public List<TreeViewNode> RootNodes
        {
            get;
        } = new List<TreeViewNode>();

        public TreeViewNode SelectedNode
        {
            get
            {
                return this._selectedNode;
            }

            set
            {
                if (value == this._selectedNode)
                {
                    return;
                }

                this._selectedNode = value;
                this._currentNode = value ?? this._currentNode;
                foreach (var item in this._items)
                {
                    item.OnSelectionChanged();
                }

                this.UpdateTabStop();
                this.OnPropertyChanged();
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Tree;
            }
        }

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this.Rebuild();
        }

        /// <summary>
        /// Shows the nodes again, after the app changed them.
        /// </summary>
        public void Refresh()
        {
            this.Rebuild();
        }

        // An item for each node shown: the roots, and the children of
        // expanded nodes, depth first
        private void Rebuild()
        {
            var focused = this._items.Find((item) => item.IsFocused)?.Node;
            foreach (var item in this._items)
            {
                item.Node.ExpandedChanged -= this.Node_ExpandedChanged;
            }

            this._items.Clear();
            this._panel.Children.Clear();
            foreach (var node in this.RootNodes)
            {
                this.AddItems(node, null, 0);
            }

            this.UpdateTabStop();

            // Focus stays on its node, once the new items are in place
            var refocus = this._items.Find((item) => item.Node == focused);
            if (null != refocus)
            {
                Application.Current?.RequestAnimationFrame((time) => refocus.Focus());
            }
        }

        // The node keyboard focus is on (or was last on, or the selected one)
        private TreeViewNode _currentNode;

        // The tree is one Tab stop, as WinUI's: its current item (or, hidden
        // in a collapsed node, the selected or first item); the arrow keys
        // reach the rest
        private void UpdateTabStop()
        {
            var stop = this._items.Find((item) => item.Node == this._currentNode)
                ?? this._items.Find((item) => item.Node == this._selectedNode)
                ?? ((this._items.Count > 0) ? this._items[0] : null);
            foreach (var item in this._items)
            {
                item.Focusable = item == stop;
            }
        }

        internal void OnItemFocused(
            TreeViewItem item)
        {
            this._currentNode = item.Node;
            this.UpdateTabStop();
        }

        private void AddItems(
            TreeViewNode node,
            TreeViewNode parent,
            int level)
        {
            var item = new TreeViewItem(this, node, parent, level);
            node.ExpandedChanged += this.Node_ExpandedChanged;
            this._items.Add(item);
            this._panel.Children.Add(item);
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    this.AddItems(child, node, level + 1);
                }
            }
        }

        private void Node_ExpandedChanged(
            object sender,
            EventArgs e)
        {
            this.Rebuild();
        }

        internal void Choose(
            TreeViewNode node)
        {
            this.SelectedNode = node;
            this.ItemInvoked?.Invoke(this, new TreeViewItemInvokedEventArgs(node));
        }

        // The keyboard, from an item
        internal bool OnItemKeyDown(
            TreeViewItem item,
            Key key)
        {
            int index = this._items.IndexOf(item);
            bool isRightToLeft = this.FlowDirection == FlowDirection.RightToLeft;
            var node = item.Node;

            if ((key == Key.Left) || (key == Key.Right))
            {
                bool isOpen = (key == Key.Right) != isRightToLeft;
                if (isOpen)
                {
                    if (node.HasChildren && !node.IsExpanded)
                    {
                        node.IsExpanded = true;
                    }
                    else if (node.IsExpanded && (index + 1 < this._items.Count))
                    {
                        this._items[index + 1].Focus();
                    }
                }
                else if (node.IsExpanded)
                {
                    node.IsExpanded = false;
                }
                else if (null != item.ParentNode)
                {
                    this._items.Find((i) => i.Node == item.ParentNode)?.Focus();
                }

                return true;
            }

            switch (key)
            {
                case Key.Up:
                    this._items[Math.Max(0, index - 1)].Focus();
                    return true;

                case Key.Down:
                    this._items[Math.Min(this._items.Count - 1, index + 1)].Focus();
                    return true;

                case Key.Home:
                    this._items[0].Focus();
                    return true;

                case Key.End:
                    this._items[this._items.Count - 1].Focus();
                    return true;

                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// An item in a tree view: a node's text, indented by its level, with a
    /// chevron when it has children. Its theme's template draws it.
    /// </summary>
    public sealed class TreeViewItem : TemplatedControl, ITapGestureDelegate
    {
        public const float ItemHeight = 36.0f;

        /// <summary>
        /// How far each level is indented.
        /// </summary>
        public const float Indent = 20.0f;

        /// <summary>
        /// The width of the chevron's area, at the start of the item's
        /// indented place.
        /// </summary>
        public const float ChevronWidth = 28.0f;

        private readonly TreeView _tree;

        internal TreeViewItem(
            TreeView tree,
            TreeViewNode node,
            TreeViewNode parentNode,
            int level)
        {
            this._tree = tree;
            this.Node = node;
            this.ParentNode = parentNode;
            this.Level = level;
            this.Focusable = true;
            this.Height = ItemHeight;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public TreeView TreeView
        {
            get
            {
                return this._tree;
            }
        }

        public TreeViewNode Node
        {
            get;
        }

        internal TreeViewNode ParentNode
        {
            get;
        }

        /// <summary>
        /// How deep the node is (0 for a root).
        /// </summary>
        public int Level
        {
            get;
        }

        public bool IsSelected
        {
            get
            {
                return this._tree.SelectedNode == this.Node;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.TreeItem;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Node.Content;
            }
        }

        public override bool AccessibilityIsSelected
        {
            get
            {
                return this.IsSelected;
            }
        }

        public override bool? AccessibilityIsExpanded
        {
            get
            {
                return this.Node.HasChildren ? this.Node.IsExpanded : null;
            }
        }

        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            return (action == AccessibilityAction.Select) || base.IsAccessibilityActionSupported(action);
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            switch (action)
            {
                case AccessibilityAction.Select:
                case AccessibilityAction.Invoke:
                    this._tree.Choose(this.Node);
                    return true;

                case AccessibilityAction.Expand:
                case AccessibilityAction.Collapse:
                    this.Node.IsExpanded = action == AccessibilityAction.Expand;
                    return true;

                default:
                    return base.PerformAccessibilityAction(action);
            }
        }

        internal void OnSelectionChanged()
        {
            this.OnStateChanged();
            this.NotifyAccessibilityStateChanged();
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// Where the chevron is.
        /// </summary>
        public Rect ChevronBounds
        {
            get
            {
                float start = 8 + (this.Level * Indent);
                float x = this.IsRightToLeft ? this.Bounds.Width - start - ChevronWidth : start;
                return new Rect(x, 0, ChevronWidth, this.Bounds.Height);
            }
        }

        /// <summary>
        /// Where the text is.
        /// </summary>
        public Rect TextBounds
        {
            get
            {
                float start = 8 + (this.Level * Indent) + ChevronWidth;
                float width = Math.Max(0, this.Bounds.Width - start - 8);
                return new Rect(this.IsRightToLeft ? 8 : start, 0, width, this.Bounds.Height);
            }
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = this.IsRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        protected override void OnActivate()
        {
            this._tree.Choose(this.Node);
        }

        protected override void OnGotFocus()
        {
            base.OnGotFocus();
            this._tree.OnItemFocused(this);
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if (this._tree.OnItemKeyDown(this, keyInputEvent.Key))
            {
                keyInputEvent.Handled = true;
            }
        }

        private Point _pressPosition;

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this._pressPosition = e.Position;
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            // On the chevron: expands or collapses; elsewhere: selects
            var chevron = this.ChevronBounds;
            if (this.Node.HasChildren && (this._pressPosition.X >= chevron.X) && (this._pressPosition.X < chevron.Right))
            {
                this.Node.IsExpanded = !this.Node.IsExpanded;
                return;
            }

            this._tree.Choose(this.Node);
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
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
