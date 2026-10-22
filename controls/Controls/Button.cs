using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A button with text, which raises <see cref="Clicked"/> and runs its
    /// <see cref="Command"/> when tapped. Its theme's template draws it.
    /// </summary>
    public class Button : TemplatedControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<Button, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);
        public static readonly ViewProperty<Button, bool> IsEnabledProperty = new(nameof(IsEnabled), v => v.IsEnabled, (v, x) => v.IsEnabled = x);
        public static readonly ViewProperty<Button, ICommand> CommandProperty = new(nameof(Command), v => v.Command, (v, x) => v.Command = x);

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
                return this.Text;
            }
        }

        private string _text;
        private bool _isAlert;

        private ICommand _command;

        public event EventHandler Clicked;

        public Button()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        /// <summary>
        /// The command the button runs when clicked. The button is enabled
        /// while the command can run.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                if (null != this._command)
                {
                    this._command.CanExecuteChanged -= this.Command_CanExecuteChanged;
                }

                this._command = value;

                if (null != value)
                {
                    value.CanExecuteChanged += this.Command_CanExecuteChanged;
                }

                this.UpdateCanExecute();
                this.OnPropertyChanged();
            }
        }

        private object _commandParameter;

        /// <summary>
        /// The value passed to the command.
        /// </summary>
        public object CommandParameter
        {
            get
            {
                return this._commandParameter;
            }

            set
            {
                this._commandParameter = value;
                this.UpdateCanExecute();
                this.OnPropertyChanged();
            }
        }

        private void Command_CanExecuteChanged(
            object sender,
            EventArgs e)
        {
            this.UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            if (null != this._command)
            {
                this.IsEnabled = this._command.CanExecute(this._commandParameter);
            }
        }

        /// <summary>
        /// Whether the button does something destructive, which its look
        /// warns of.
        /// </summary>
        public bool IsAlertStyle
        {
            get
            {
                return this._isAlert;
            }

            set
            {
                this._isAlert = value;

                this.InvalidateVisual();
            }
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

                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
        }

        void ITapGestureDelegate.OnTapped()
        {
            if (this.IsEnabled)
            {
                this.Clicked?.Invoke(this, EventArgs.Empty);

                if ((null != this._command) && this._command.CanExecute(this._commandParameter))
                {
                    this._command.Execute(this._commandParameter);
                }
            }
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

        #endregion
    }
}
