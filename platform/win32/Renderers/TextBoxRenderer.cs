using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Controls.RichEdit;
using Windows.Win32.UI.WindowsAndMessaging;
using XForms.Controls;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// Native text entry: a RichEdit control (the text engine underneath the
    /// XAML TextBox) in its Direct2D/DirectWrite mode, as a child window.
    /// </summary>
    internal sealed unsafe class TextBoxRenderer : ViewRenderer, ITextBoxRenderer
    {
        private const uint EM_REQUESTRESIZE = PInvoke.WM_USER + 65;
        private const uint EM_SETBKGNDCOLOR = PInvoke.WM_USER + 67;
        private const uint EM_SETCHARFORMAT = PInvoke.WM_USER + 68;
        private const uint EM_SETEVENTMASK = PInvoke.WM_USER + 69;
        private const uint EM_SETLANGOPTIONS = PInvoke.WM_USER + 120;
        private const uint EM_GETLANGOPTIONS = PInvoke.WM_USER + 121;
        private const uint EN_SETFOCUS = 0x0100;
        private const uint EN_KILLFOCUS = 0x0200;
        private const uint EN_CHANGE = 0x0300;
        private const uint EN_REQUESTRESIZE = 0x0701;
        private const uint EN_SELCHANGE = 0x0702;
        private const int ENM_CHANGE = 0x00000001;
        private const int ENM_REQUESTRESIZE = 0x00040000;
        private const int ENM_SELCHANGE = 0x00080000;
        private const uint EM_EXGETSEL = PInvoke.WM_USER + 52;
        private const uint EM_EXLIMITTEXT = PInvoke.WM_USER + 53;
        private const uint EM_EXSETSEL = PInvoke.WM_USER + 55;
        private const uint EM_SETPASSWORDCHAR = 0x00CC;
        private const uint EM_SETREADONLY = 0x00CF;
        private const char PasswordChar = '\u25CF';
        private const int IMF_SPELLCHECKING = 0x0800;
        private const int SCF_ALL = 0x0004;
        private const uint ES_MULTILINE = 0x0004;
        private const uint ES_AUTOVSCROLL = 0x0040;
        private const uint ES_AUTOHSCROLL = 0x0080;
        private const uint ES_WANTRETURN = 0x1000;

        private const float FontSize = 15.0f;
        private const float Padding = 6.0f;

        private HWND _hwnd;
        private bool _multiline;
        private string _placeholderText;
        private KeyboardOptions _keyboardOptions = KeyboardOptions.All;
        private float _contentHeight;
        private Color _textColor = Colors.Black;
        private Color _fieldColor = Color.FromArgb(0xFF, 0xF2, 0xF2, 0xF2);
        private Color _placeholderColor = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
        private RECT _windowBounds;
        private bool _isWindowShown;
        private bool _isFocused;
        private bool _isPassword;
        private int _maxLength;
        private bool _isReadOnly;

        // The range RichEdit reports (cpMin, cpMax)
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct CharRange
        {
            public int Min;
            public int Max;
        }

        public TextBoxRenderer(
            TextBox textEntry)
            : base(textEntry, isContainer: false)
        {
            this.CreateEditWindow(string.Empty);
            this.Host.RegisterTextBox(this);
        }

        public HWND Hwnd
        {
            get
            {
                return this._hwnd;
            }
        }

        private void CreateEditWindow(
            string text)
        {
            uint style = (uint)(WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_TABSTOP);
            style |= this._multiline ? (ES_MULTILINE | ES_AUTOVSCROLL | ES_WANTRETURN) : ES_AUTOHSCROLL;

            // Prefer RichEdit's Direct2D/DirectWrite rendering
            this._hwnd = Create(Environment.GetEnvironmentVariable("XFORMS_EDIT_CLASS") ?? "RichEditD2DPT", (WINDOW_STYLE)style); // TEMP
            if (this._hwnd.IsNull)
            {
                this._hwnd = Create("RICHEDIT50W", (WINDOW_STYLE)style);
            }

            PInvoke.SendMessage(this._hwnd, EM_SETEVENTMASK, 0, ENM_CHANGE | ENM_REQUESTRESIZE | ENM_SELCHANGE);

            this.ApplyFormat();
            this.ApplyKeyboardOptions();
            this.ApplyEditOptions();

            if (null != this._placeholderText)
            {
                this.ApplyPlaceholder();
            }

            if (!string.IsNullOrEmpty(text))
            {
                PInvoke.SetWindowText(this._hwnd, text);
            }
        }

        private HWND Create(
            string className,
            WINDOW_STYLE style)
        {
            fixed (char* classNamePointer = className)
            {
                return PInvoke.CreateWindowEx(
                    0,
                    classNamePointer,
                    null,
                    style,
                    0,
                    0,
                    100,
                    30,
                    this.Host.Hwnd,
                    HMENU.Null,
                    HINSTANCE.Null,
                    null);
            }
        }

        private void ApplyFormat()
        {
            var format = new CHARFORMAT2W();
            format.Base.cbSize = (uint)sizeof(CHARFORMAT2W);
            format.Base.dwMask = CFM_MASK.CFM_FACE | CFM_MASK.CFM_SIZE | CFM_MASK.CFM_COLOR;

            // Points, in twips
            format.Base.yHeight = (int)(FontSize * 72 / 96 * 20);
            format.Base.crTextColor = ToColorRef(this._textColor);
            "Segoe UI".AsSpan().CopyTo(format.Base.szFaceName.AsSpan());

            PInvoke.SendMessage(this._hwnd, EM_SETCHARFORMAT, (nuint)SCF_ALL, (nint)(&format));
            PInvoke.SendMessage(this._hwnd, EM_SETBKGNDCOLOR, 0, (nint)ToColorRef(this._fieldColor).Value);
        }

        private void ApplyEditOptions()
        {
            // NOTE: A password shows as dots; RichEdit keeps it from being copied
            PInvoke.SendMessage(this._hwnd, EM_SETPASSWORDCHAR, this._isPassword ? PasswordChar : 0u, 0);

            // Without a limit, RichEdit stops at 64K characters
            PInvoke.SendMessage(this._hwnd, EM_EXLIMITTEXT, 0, (0 < this._maxLength) ? this._maxLength : int.MaxValue);

            PInvoke.SendMessage(this._hwnd, EM_SETREADONLY, this._isReadOnly ? 1u : 0u, 0);
        }

        public bool IsPassword
        {
            set
            {
                this._isPassword = value;
                this.ApplyEditOptions();
                this.ApplyKeyboardOptions();
            }
        }

        public int MaxLength
        {
            set
            {
                this._maxLength = value;
                this.ApplyEditOptions();
            }
        }

        public bool IsReadOnly
        {
            set
            {
                this._isReadOnly = value;
                this.ApplyEditOptions();
            }
        }

        private CharRange GetSelection()
        {
            var range = new CharRange();
            PInvoke.SendMessage(this._hwnd, EM_EXGETSEL, 0, (nint)(&range));
            return range;
        }

        public int SelectionStart
        {
            get
            {
                return this.GetSelection().Min;
            }
        }

        public int SelectionLength
        {
            get
            {
                var range = this.GetSelection();
                return Math.Max(0, range.Max - range.Min);
            }
        }

        public void Select(
            int start,
            int length)
        {
            var range = new CharRange() { Min = start, Max = start + length };
            PInvoke.SendMessage(this._hwnd, EM_EXSETSEL, 0, (nint)(&range));
        }

        /// <summary>
        /// Handles Return in a single-line text box: submits instead of beeping.
        /// Returns whether it handled the key.
        /// </summary>
        public bool OnReturnKey()
        {
            if (this._multiline)
            {
                return false;
            }

            this.Host.Post(((ITextBoxDelegate)this.Element).OnNativeSubmitted);
            return true;
        }

        private void ApplyKeyboardOptions()
        {
            nint options = PInvoke.SendMessage(this._hwnd, EM_GETLANGOPTIONS, 0, 0).Value;
            if (this._keyboardOptions.HasFlag(KeyboardOptions.SpellCheck) && !this._isPassword && Environment.GetEnvironmentVariable("XFORMS_NOSPELL") != "1") // TEMP
            {
                options |= IMF_SPELLCHECKING;
            }
            else
            {
                options &= ~IMF_SPELLCHECKING;
            }

            PInvoke.SendMessage(this._hwnd, EM_SETLANGOPTIONS, 0, options);
        }

        private void ApplyPlaceholder()
        {
            // Screen readers name the edit window by the view's label (by default its placeholder)
            if (Environment.GetEnvironmentVariable("XFORMS_NOACCNAME") != "1") // TEMP
            Automation.AccessibleNames.Set(this._hwnd, ((View)this.Element).AccessibilityLabel);
            this.RefreshPlaceholder();
        }

        /// <summary>
        /// RichEdit has no placeholder of its own, so while the entry is empty and
        /// unfocused the edit window is hidden and the placeholder is drawn instead.
        /// </summary>
        private bool IsShowingPlaceholder
        {
            get
            {
                return (!this._isFocused)
                    && (!string.IsNullOrEmpty(this._placeholderText))
                    && (PInvoke.GetWindowTextLength(this._hwnd) == 0);
            }
        }

        /// <summary>
        /// Whether the RichEdit window is showing (it then describes itself to UI Automation).
        /// </summary>
        public bool IsEditWindowShown
        {
            get
            {
                return this._isWindowShown;
            }
        }

        private void RefreshPlaceholder()
        {
            this.InvalidateRender();
            this.UpdateWindowPosition();
        }

        private static COLORREF ToColorRef(
            Color color)
        {
            return new COLORREF((uint)(color.R | (color.G << 8) | (color.B << 16)));
        }

        public string Text
        {
            get
            {
                int length = PInvoke.GetWindowTextLength(this._hwnd);
                if (length == 0)
                {
                    return string.Empty;
                }

                Span<char> buffer = new char[length + 1];
                int copied = PInvoke.GetWindowText(this._hwnd, buffer);

                // NOTE: RichEdit uses carriage returns for line breaks
                return new string(buffer.Slice(0, copied)).Replace("\r\n", "\n").Replace('\r', '\n');
            }

            set
            {
                PInvoke.SetWindowText(this._hwnd, value ?? string.Empty);
                this.RequestContentHeight();
                this.RefreshPlaceholder();
            }
        }

        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }

            set
            {
                this._placeholderText = value;
                this.ApplyPlaceholder();
            }
        }

        public bool Multiline
        {
            get
            {
                return this._multiline;
            }

            set
            {
                if (value == this._multiline)
                {
                    return;
                }

                // NOTE: The multiline style can only be chosen when the control is created
                string text = this.Text;
                PInvoke.DestroyWindow(this._hwnd);

                this._multiline = value;
                this.CreateEditWindow(text);
                this._windowBounds = default;
                this._isWindowShown = false;
                this.UpdateWindowPosition();
                this.InvalidateMeasure();
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                return this._keyboardOptions;
            }

            set
            {
                this._keyboardOptions = value;
                this.ApplyKeyboardOptions();
            }
        }

        public void Focus()
        {
            // Show the edit window (in place of any placeholder) before focusing it
            this._isFocused = true;
            this.RefreshPlaceholder();
            PInvoke.SetFocus(this._hwnd);
        }

        public override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this._textColor = theme.Colors.OnSurface;
            this._fieldColor = theme.Colors.Surface;
            this._placeholderColor = theme.Colors.SubtleForeground;
            this.ApplyFormat();
            this.InvalidateRender();
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;

            // Until RichEdit reports its content height, assume a single line
            float height = (this._contentHeight > 0) ? this._contentHeight : (FontSize * 1.4f);

            return new Size(width, Math.Min(height + (2 * Padding), availableSize.Height));
        }

        public override void ArrangeNative(
            Rect frame)
        {
            bool isWidthChanged = (frame.Width != this.Frame.Width);

            base.ArrangeNative(frame);

            this.UpdateWindowPosition();

            if (isWidthChanged)
            {
                this.RequestContentHeight();
            }
        }

        /// <summary>
        /// Places the child window over the view's frame. Called after layout and
        /// whenever an ancestor scrolls or moves.
        /// </summary>
        public void UpdateWindowPosition()
        {
            float scale = this.Host.Scale;
            var origin = this.AbsoluteOrigin();
            var frame = this.Frame;

            var bounds = new RECT()
            {
                left = (int)Math.Round(origin.X * scale),
                top = (int)Math.Round((origin.Y + Padding) * scale),
            };
            bounds.right = bounds.left + (int)Math.Round(frame.Width * scale);
            bounds.bottom = bounds.top + (int)Math.Round(Math.Max(0, frame.Height - (2 * Padding)) * scale);

            bool isShown = this.IsShownOnScreen && ((Environment.GetEnvironmentVariable("XFORMS_NOHIDE") == "1") || !this.IsShowingPlaceholder); // TEMP

            // Moving a window is relatively costly, so only do it when something changed
            if ((bounds.left == this._windowBounds.left) && (bounds.top == this._windowBounds.top)
                && (bounds.right == this._windowBounds.right) && (bounds.bottom == this._windowBounds.bottom)
                && (isShown == this._isWindowShown))
            {
                return;
            }

            this._windowBounds = bounds;
            this._isWindowShown = isShown;

            PInvoke.SetWindowPos(
                this._hwnd,
                HWND.Null,
                bounds.left,
                bounds.top,
                bounds.right - bounds.left,
                bounds.bottom - bounds.top,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
                    | (isShown ? SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW : SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW));
        }

        protected override bool HasContent
        {
            get
            {
                return true;
            }
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            var frame = this.Frame;

            // The field background behind the edit window's padding
            drawContext.FillRectangle(new Rect(0, 0, frame.Width, frame.Height), this._fieldColor);

            if (this.IsShowingPlaceholder)
            {
                var format = new TextFormat()
                {
                    FontSize = FontSize,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                };

                // NOTE: Offset to line up with RichEdit's own text margin
                drawContext.DrawText(
                    this._placeholderText,
                    new Rect(1, Padding, Math.Max(0, frame.Width - 2), Math.Max(0, frame.Height - (2 * Padding))),
                    this._placeholderColor,
                    format);
            }
        }

        private void RequestContentHeight()
        {
            // Bottomless RichEdit: it answers with EN_REQUESTRESIZE
            PInvoke.SendMessage(this._hwnd, EM_REQUESTRESIZE, 0, 0);
        }

        // NOTE: RichEdit sends its notifications in the middle of its own work, and
        // calling back into it from them (resizing, hiding or querying the window)
        // can leave it using freed memory. So notifications only record what
        // happened; the work is done once the notification has returned.

        public void OnCommand(
            uint notificationCode)
        {
            switch (notificationCode)
            {
                case EN_CHANGE:
                    this.Host.Post(this.RequestContentHeight);
                    this.Host.Post(((ITextBoxDelegate)this.Element).OnNativeTextChanged);
                    this.RequestPlaceholderRefresh();
                    break;

                case EN_SETFOCUS:
                    this._isFocused = true;
                    this.Host.Focus.OnTextBoxFocused(this);
                    this.RequestPlaceholderRefresh();
                    break;

                case EN_KILLFOCUS:
                    this._isFocused = false;
                    this.RequestPlaceholderRefresh();
                    break;
            }
        }

        /// <summary>
        /// Redraws the field and shows or hides the edit window on the next update.
        /// </summary>
        private void RequestPlaceholderRefresh()
        {
            this.InvalidateRender();
            this.Host.RequestTextBoxPositionUpdate();
        }

        public void OnNotify(
            NMHDR* header)
        {
            if (header->code == EN_SELCHANGE)
            {
                this.Host.Post(((ITextBoxDelegate)this.Element).OnNativeSelectionChanged);
                return;
            }

            if (header->code == EN_REQUESTRESIZE)
            {
                var request = (REQRESIZE*)header;
                float height = (request->rc.bottom - request->rc.top) / this.Host.Scale;

                if (Math.Abs(height - this._contentHeight) > 0.5f)
                {
                    this._contentHeight = height;
                    this.InvalidateMeasure();
                }
            }
        }
    }
}
