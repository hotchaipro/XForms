using System;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// Whether a password box can show its password.
    /// </summary>
    public enum PasswordRevealMode
    {
        /// <summary>
        /// A button at the end of the box shows it while it's pressed.
        /// </summary>
        Peek,

        /// <summary>
        /// Never.
        /// </summary>
        Hidden,

        /// <summary>
        /// Always.
        /// </summary>
        Visible,
    }

    /// <summary>
    /// A text box for a password, its characters hidden, with a button that
    /// shows them while it's pressed. Its theme's template draws the button,
    /// <see cref="RevealButtonWidth"/> wide, at the end of the box.
    /// </summary>
    public class PasswordBox : TemplatedControl
    {
        public static readonly ViewProperty<PasswordBox, string> PasswordProperty = new(nameof(Password), v => v.Password, (v, x) => v.Password = x);

        public const float RevealButtonWidth = 36.0f;

        private TextBox _textBox;
        private string _password = string.Empty;
        private string _placeholderText;
        private PasswordRevealMode _passwordRevealMode;
        private bool _isRevealPressed;

        public PasswordBox()
        {
        }

        /// <summary>
        /// Raised when the password changes.
        /// </summary>
        public event EventHandler PasswordChanged;

        public string Password
        {
            get
            {
                return this._password;
            }

            set
            {
                value ??= string.Empty;
                if (value == this._password)
                {
                    return;
                }

                bool hadPassword = this._password.Length > 0;
                this._password = value;
                if ((null != this._textBox) && (this._textBox.Text != value))
                {
                    this._textBox.Text = value;
                }

                if (hadPassword != (value.Length > 0))
                {
                    this.OnStateChanged();
                }

                this.OnPropertyChanged();
                this.PasswordChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Shown while the box is empty; it also names the box for assistive
        /// technology.
        /// </summary>
        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }

            set
            {
                this._placeholderText = value;
                this._textBox?.PlaceholderText = value;
            }
        }

        public PasswordRevealMode PasswordRevealMode
        {
            get
            {
                return this._passwordRevealMode;
            }

            set
            {
                this._passwordRevealMode = value;
                this.ApplyReveal();
            }
        }

        /// <summary>
        /// Whether the reveal button shows: peeking, with a password to show.
        /// </summary>
        public bool IsRevealButtonVisible
        {
            get
            {
                return (this._passwordRevealMode == PasswordRevealMode.Peek) && (this._password.Length > 0);
            }
        }

        /// <summary>
        /// Whether the password is showing.
        /// </summary>
        public bool IsPasswordRevealed
        {
            get
            {
                return (this._passwordRevealMode == PasswordRevealMode.Visible) || this._isRevealPressed;
            }
        }

        private Thickness TextBoxMargin
        {
            get
            {
                float end = (this._passwordRevealMode == PasswordRevealMode.Peek) ? RevealButtonWidth : 0;
                return new Thickness(0, 0, end, 0);
            }
        }

        protected override View Build()
        {
            this._textBox = new TextBox()
            {
                Text = this._password,
                PlaceholderText = this._placeholderText,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                KeyboardOptions = KeyboardOptions.None,
            };
            this._textBox.TextChanged += (sender, e) => this.Password = this._textBox.Text;
            this.ApplyReveal();
            return this._textBox;
        }

        private void ApplyReveal()
        {
            if (null != this._textBox)
            {
                this._textBox.IsPassword = !this.IsPasswordRevealed;
                this._textBox.Margin = this.TextBoxMargin;
            }

            this.OnStateChanged();
        }

        /// <summary>
        /// Where the reveal button is: at the end.
        /// </summary>
        public Rect RevealButtonBounds
        {
            get
            {
                float x = (this.FlowDirection == FlowDirection.RightToLeft) ? 0 : this.Bounds.Width - RevealButtonWidth;
                return new Rect(x, 0, RevealButtonWidth, this.Bounds.Height);
            }
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            var bounds = this.RevealButtonBounds;
            var point = e.Position;
            if (!this.IsEnabled || !this.IsRevealButtonVisible
                || (point.X < bounds.X) || (point.X >= bounds.Right) || (point.Y < 0) || (point.Y >= bounds.Bottom))
            {
                return;
            }

            this._isRevealPressed = true;
            this.CapturePointer(e);
            this.ApplyReveal();
            e.Handled = true;
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            this.EndReveal();
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this.EndReveal();
        }

        private void EndReveal()
        {
            if (this._isRevealPressed)
            {
                this._isRevealPressed = false;
                this.ApplyReveal();
            }
        }
    }
}
