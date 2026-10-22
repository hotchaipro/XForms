using System;
using System.Collections.Generic;
using System.Windows.Input;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    internal sealed class CommandBarButtonCollection
    {
        private CommandBar _commandBar;
        private List<CommandBarButton> _buttons;
        private Dock _buttonRegion;

        internal CommandBarButtonCollection(
            CommandBar commandBar,
            Dock buttonRegion)
        {
            if (null == commandBar)
            {
                throw new ArgumentNullException(nameof(commandBar));
            }

            this._commandBar = commandBar;
            this._buttons = new List<CommandBarButton>();
            this._buttonRegion = buttonRegion;
        }

        public int Count
        {
            get
            {
                return this._buttons.Count;
            }
        }

        public void Add(
            CommandBarButton button)
        {
            if (null == button)
            {
                throw new ArgumentNullException(nameof(button));
            }

            button.Width = 48f;
            button.Height = 48f;

            this._commandBar.ButtonsLayout.Children.Add(button, this._buttonRegion);
        }
    }

    /// <summary>
    /// A <see cref="CommandBar"/>'s button: an icon and a label, showing while
    /// its command can run. Its theme's template draws it.
    /// </summary>
    public sealed class CommandBarButton : TemplatedControl, ITapGestureDelegate
    {
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
        private Bitmap _icon;

        public CommandBarButton()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public ICommand Command
        {
            get;
            set;
        }

        private object _commandParameter;

        /// <summary>
        /// The value passed to the command. The button shows only while the
        /// command can run with it.
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

                if (null != this.Command)
                {
                    this.IsVisible = this.Command.CanExecute(value);
                }
            }
        }

        public Bitmap Icon
        {
            get
            {
                return this._icon;
            }

            set
            {
                this._icon = value;

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
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.Command?.Execute(this.CommandParameter);
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

        #endregion
    }
}
