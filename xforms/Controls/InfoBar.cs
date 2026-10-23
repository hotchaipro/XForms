using System;
using XForms.Input;

namespace XForms.Controls
{
    /// <summary>
    /// How serious an info bar's message is, which its icon and color show.
    /// </summary>
    public enum InfoBarSeverity
    {
        Informational,
        Success,
        Warning,
        Error,
    }

    /// <summary>
    /// A message in the page about the app's state: a title and a message
    /// beside an icon for its severity, an optional action button beneath,
    /// and a close button. Assistive technology reads it as it opens. Its
    /// theme's template draws it, in the layout it measures: the icon at the
    /// start, the text after it, wrapped, and the close button at the end.
    /// </summary>
    public class InfoBar : TemplatedControl
    {
        public static readonly ViewProperty<InfoBar, bool> IsOpenProperty = new(nameof(IsOpen), v => v.IsOpen, (v, x) => v.IsOpen = x);

        /// <summary>
        /// The space inside the bar's edges, and between its parts.
        /// </summary>
        public const float Spacing = 12.0f;

        /// <summary>
        /// The severity icon's size.
        /// </summary>
        public const float IconSize = 20.0f;

        private const float CloseButtonSize = 32.0f;

        private readonly InfoBarCloseButton _closeButton;
        private string _title;
        private string _message;
        private InfoBarSeverity _severity;
        private bool _isOpen;
        private bool _isClosable = true;
        private View _actionButton;

        public InfoBar()
        {
            this.IsVisible = false;
            this._closeButton = new InfoBarCloseButton(this);
            this.AddVisualChild(this._closeButton);
        }

        /// <summary>
        /// Raised when the user closes the bar with its close button.
        /// </summary>
        public event EventHandler Closed;

        public string Title
        {
            get
            {
                return this._title;
            }

            set
            {
                this._title = value;
                this.OnContentChanged();
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }

            set
            {
                this._message = value;
                this.OnContentChanged();
            }
        }

        public InfoBarSeverity Severity
        {
            get
            {
                return this._severity;
            }

            set
            {
                this._severity = value;
                this.OnStateChanged();
            }
        }

        /// <summary>
        /// Whether the bar shows (by default, it doesn't).
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }

            set
            {
                if (value != this._isOpen)
                {
                    this._isOpen = value;
                    this.IsVisible = value;
                    this.OnPropertyChanged();

                    // Read as it opens
                    if (value)
                    {
                        this.NotifyAccessibilityStateChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the bar has a close button.
        /// </summary>
        public bool IsClosable
        {
            get
            {
                return this._isClosable;
            }

            set
            {
                this._isClosable = value;
                this._closeButton.IsVisible = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// A button (or another view) beneath the message, for what the user
        /// can do about it.
        /// </summary>
        public View ActionButton
        {
            get
            {
                return this._actionButton;
            }

            set
            {
                if (null != this._actionButton)
                {
                    this.RemoveVisualChild(this._actionButton);
                }

                this._actionButton = value;
                if (null != value)
                {
                    this.AddVisualChild(value);
                }

                this.InvalidateMeasure();
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Alert;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return string.IsNullOrEmpty(this._title) ? this._message
                    : string.IsNullOrEmpty(this._message) ? this._title
                    : this._title + ", " + this._message;
            }
        }

        private void OnContentChanged()
        {
            this.InvalidateMeasure();
            this.InvalidateVisual();
            if (this._isOpen)
            {
                this.NotifyAccessibilityStateChanged();
            }
        }

        internal void Close()
        {
            this.IsOpen = false;
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        public TextFormat CreateTitleFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.FontWeight = FontWeight.SemiBold;
            return this.Wrapped(format);
        }

        public TextFormat CreateMessageFormat()
        {
            return this.Wrapped(this.ActualTheme.Typography.Body.ToTextFormat());
        }

        private TextFormat Wrapped(
            TextFormat format)
        {
            format.TextWrapping = TextWrapping.Wrap;
            format.HorizontalAlignment = this.IsRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            return format;
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        // The text's width, between the icon and the close button
        private float TextWidth(
            float width)
        {
            float closeWidth = this._isClosable ? CloseButtonSize : 0;
            return Math.Max(0, width - (2 * Spacing) - IconSize - Spacing - closeWidth);
        }

        private float TitleHeight(
            float textWidth)
        {
            return string.IsNullOrEmpty(this._title) ? 0 : TextLayout.Measure(this._title, this.CreateTitleFormat(), textWidth).Height;
        }

        private float MessageHeight(
            float textWidth)
        {
            return string.IsNullOrEmpty(this._message) ? 0 : TextLayout.Measure(this._message, this.CreateMessageFormat(), textWidth).Height;
        }

        /// <summary>
        /// Where the severity icon is.
        /// </summary>
        public Rect IconBounds
        {
            get
            {
                float x = this.IsRightToLeft ? this.Bounds.Width - Spacing - IconSize : Spacing;
                return new Rect(x, Spacing + 1, IconSize, IconSize);
            }
        }

        /// <summary>
        /// Where the title is (above the message).
        /// </summary>
        public Rect TitleBounds
        {
            get
            {
                float width = this.TextWidth(this.Bounds.Width);
                return new Rect(this.TextX(width), Spacing, width, this.TitleHeight(width));
            }
        }

        /// <summary>
        /// Where the message is.
        /// </summary>
        public Rect MessageBounds
        {
            get
            {
                float width = this.TextWidth(this.Bounds.Width);
                float top = Spacing + this.TitleHeight(width);
                return new Rect(this.TextX(width), top, width, this.MessageHeight(width));
            }
        }

        private float TextX(
            float textWidth)
        {
            return this.IsRightToLeft ? this.Bounds.Width - Spacing - IconSize - Spacing - textWidth : Spacing + IconSize + Spacing;
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 480 : availableSize.Width;
            float textWidth = this.TextWidth(width);
            float height = this.TitleHeight(textWidth) + this.MessageHeight(textWidth);

            this._closeButton.Measure(new Size(CloseButtonSize, CloseButtonSize));
            if (null != this._actionButton)
            {
                this._actionButton.Measure(new Size(textWidth, float.PositiveInfinity));
                height += Spacing + this._actionButton.DesiredSize.Height;
            }

            height = Math.Max(height, IconSize + 2);
            return new Size(width, Math.Min(height + (2 * Spacing), availableSize.Height));
        }

        // NOTE: Arranged in left-to-right terms; a right-to-left parent
        // mirrors the children
        protected override void ArrangeOverride(
            Size finalSize)
        {
            float textWidth = this.TextWidth(finalSize.Width);
            this._closeButton.Arrange(new Rect(finalSize.Width - Spacing - CloseButtonSize + 6, Spacing - 6, CloseButtonSize, CloseButtonSize));

            if (null != this._actionButton)
            {
                float top = Spacing + this.TitleHeight(textWidth) + this.MessageHeight(textWidth) + Spacing;
                var size = this._actionButton.DesiredSize;
                this._actionButton.Arrange(new Rect(Spacing + IconSize + Spacing, top, Math.Min(size.Width, textWidth), size.Height));
            }
        }
    }

    /// <summary>
    /// An info bar's close button. Its theme's template draws it.
    /// </summary>
    public sealed class InfoBarCloseButton : TemplatedControl, ITapGestureDelegate
    {
        private readonly InfoBar _infoBar;

        internal InfoBarCloseButton(
            InfoBar infoBar)
        {
            this._infoBar = infoBar;
            this.Focusable = true;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public InfoBar InfoBar
        {
            get
            {
                return this._infoBar;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return "Close";
            }
        }

        protected override void OnActivate()
        {
            this._infoBar.Close();
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this._infoBar.Close();
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
