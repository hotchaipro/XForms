using System;
using System.Windows.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class UICommand : ICommand
    {
        public UICommand()
        {
            this.ForegroundColor = Colors.White;
        }

        public ICommand Command
        {
            get;
            set;
        }

        public Color ForegroundColor
        {
            get;
            set;
        }

        public Color AccentColor
        {
            get;
            set;
        }

        public Bitmap Icon
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Raised when the wrapped command's ability to run changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (null != this.Command)
                {
                    this.Command.CanExecuteChanged += value;
                }
            }

            remove
            {
                if (null != this.Command)
                {
                    this.Command.CanExecuteChanged -= value;
                }
            }
        }

        public bool CanExecute(
            object parameter)
        {
            return this.Command?.CanExecute(parameter) ?? false;
        }

        public void Execute(
            object parameter)
        {
            this.Command?.Execute(parameter);
        }
    }
}
