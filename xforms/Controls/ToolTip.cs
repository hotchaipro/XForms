using System;
using System.Runtime.CompilerServices;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A short description shown beside a view: the content of the popup a
    /// view's tooltip (see <see cref="ViewAttachments"/>) opens. Its theme's
    /// template draws it, in the layout it measures: the text, wrapped, inside
    /// its padding.
    /// </summary>
    public sealed class ToolTip : TemplatedControl
    {
        /// <summary>
        /// The widest a tooltip grows before its text wraps.
        /// </summary>
        public const float MaximumWidth = 320.0f;

        private string _text;

        public ToolTip()
        {
            this.Padding = new Thickness(8, 5, 8, 6);
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ToolTip;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._text;
            }
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Caption.ToTextFormat();
            format.TextWrapping = TextWrapping.Wrap;
            format.HorizontalAlignment = (this.FlowDirection == FlowDirection.RightToLeft) ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            return format;
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var padding = this.Padding;
            float maxText = Math.Min(MaximumWidth, availableSize.Width) - padding.Left - padding.Right;
            var size = TextLayout.Measure(this._text ?? string.Empty, this.CreateTextFormat(), Math.Max(0, maxText));
            return new Size(
                (float)Math.Ceiling(size.Width) + padding.Left + padding.Right,
                (float)Math.Ceiling(size.Height) + padding.Top + padding.Bottom);
        }
    }

    /// <summary>
    /// What any view can be given beyond its own properties, writable in an
    /// initializer: a tooltip, and a context menu.
    /// </summary>
    public static class ViewAttachments
    {
        private static readonly ConditionalWeakTable<View, Attachment> Attachments = new ConditionalWeakTable<View, Attachment>();

        extension(View view)
        {
            /// <summary>
            /// Text shown beside the view while the pointer rests on it, while
            /// it has keyboard focus, and after a long press (where there's no
            /// context menu); null for none.
            /// </summary>
            public string ToolTip
            {
                get
                {
                    return Attachments.TryGetValue(view, out var attachment) ? attachment.ToolTipText : null;
                }

                set
                {
                    Attachments.GetValue(view, (v) => new Attachment(v)).ToolTipText = value;
                }
            }

            /// <summary>
            /// The popup (such as a <see cref="MenuFlyout"/>) shown when the
            /// user asks for the view's context menu: a right-click, a long
            /// press, the Menu key or Shift+F10.
            /// </summary>
            public Popup ContextFlyout
            {
                get
                {
                    return Attachments.TryGetValue(view, out var attachment) ? attachment.ContextFlyout : null;
                }

                set
                {
                    Attachments.GetValue(view, (v) => new Attachment(v)).ContextFlyout = value;
                }
            }
        }

        /// <summary>
        /// A view's tooltip and context menu, and the handlers that show them.
        /// </summary>
        private sealed class Attachment
        {
            private static readonly TimeSpan HoverDelay = TimeSpan.FromMilliseconds(500);
            private static readonly TimeSpan TouchDuration = TimeSpan.FromSeconds(3);

            private readonly WeakReference<View> _view;
            private readonly DispatcherTimer _timer = new DispatcherTimer();
            private Popup _toolTipPopup;
            private ToolTip _toolTip;
            private bool _isPointerOver;

            public Attachment(
                View view)
            {
                this._view = new WeakReference<View>(view);
                this._timer.Tick += this.Timer_Tick;
                view.PointerEntered += this.View_PointerEntered;
                view.PointerExited += this.View_PointerExited;
                view.PointerPressed += this.View_PointerPressed;
                view.GotFocus += this.View_GotFocus;
                view.LostFocus += this.View_LostFocus;
                view.KeyDown += this.View_KeyDown;
                view.ContextRequested += this.View_ContextRequested;
            }

            public string ToolTipText
            {
                get;
                set;
            }

            public Popup ContextFlyout
            {
                get;
                set;
            }

            private bool IsShowing
            {
                get
                {
                    return this._toolTipPopup?.IsOpen ?? false;
                }
            }

            // Shown after resting a moment, or at once
            private void ShowToolTip(
                TimeSpan delay,
                TimeSpan duration)
            {
                if (string.IsNullOrEmpty(this.ToolTipText))
                {
                    return;
                }

                this._timer.Stop();
                if (delay > TimeSpan.Zero)
                {
                    this._pendingDuration = duration;
                    this._timer.Interval = delay;
                    this._timer.Start();
                    return;
                }

                this.Open(duration);
            }

            private TimeSpan _pendingDuration;

            private void Timer_Tick(
                object sender,
                EventArgs e)
            {
                this._timer.Stop();
                if (this.IsShowing)
                {
                    // Shown for a while (after a long press): now gone
                    this.HideToolTip();
                    return;
                }

                this.Open(this._pendingDuration);
            }

            private void Open(
                TimeSpan duration)
            {
                if (!this._view.TryGetTarget(out var view) || string.IsNullOrEmpty(this.ToolTipText))
                {
                    return;
                }

                if (null == this._toolTipPopup)
                {
                    this._toolTip = new ToolTip();
                    this._toolTipPopup = new Popup()
                    {
                        IsLightDismissEnabled = false,
                        Content = this._toolTip,
                    };
                }

                this._toolTip.Text = this.ToolTipText;
                this._toolTipPopup.PlacementTarget = view;
                if (!this._toolTipPopup.IsOpen)
                {
                    this._toolTipPopup.Show();
                }

                if (duration > TimeSpan.Zero)
                {
                    this._timer.Interval = duration;
                    this._timer.Start();
                }
            }

            private void HideToolTip()
            {
                this._timer.Stop();
                this._toolTipPopup?.Close();
            }

            private void View_PointerEntered(
                object sender,
                PointerEventArgs e)
            {
                this._isPointerOver = true;
                if (e.DeviceType != PointerDeviceType.Touch)
                {
                    this.ShowToolTip(HoverDelay, TimeSpan.Zero);
                }
            }

            private void View_PointerExited(
                object sender,
                PointerEventArgs e)
            {
                this._isPointerOver = false;
                this.HideToolTip();
            }

            private void View_PointerPressed(
                object sender,
                PointerEventArgs e)
            {
                this.HideToolTip();
            }

            // Keyboard focus shows it (a press that focuses the view doesn't)
            private void View_GotFocus(
                object sender,
                EventArgs e)
            {
                if (!this._isPointerOver)
                {
                    this.ShowToolTip(TimeSpan.Zero, TimeSpan.Zero);
                }
            }

            private void View_LostFocus(
                object sender,
                EventArgs e)
            {
                this.HideToolTip();
            }

            private void View_KeyDown(
                object sender,
                KeyEventArgs e)
            {
                if ((e.Key == Key.Escape) && this.IsShowing)
                {
                    this.HideToolTip();
                    e.Handled = true;
                }
            }

            // The context menu, or, after a long press without one, the
            // tooltip for a few seconds
            private void View_ContextRequested(
                object sender,
                ContextRequestedEventArgs e)
            {
                if (!this._view.TryGetTarget(out var view))
                {
                    return;
                }

                var flyout = this.ContextFlyout;
                if (null != flyout)
                {
                    this.HideToolTip();
                    flyout.PlacementTarget = view;
                    flyout.Show();
                    (flyout as MenuFlyout)?.FocusMenu();
                    e.Handled = true;
                    return;
                }

                if (e.DeviceType.HasValue && (e.DeviceType != PointerDeviceType.Mouse) && !string.IsNullOrEmpty(this.ToolTipText))
                {
                    this.ShowToolTip(TimeSpan.Zero, TouchDuration);
                    e.Handled = true;
                }
            }
        }
    }
}
