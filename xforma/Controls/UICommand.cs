using System;
using System.Windows.Input;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
{
    public class UICommand : ICommand
    {
        public ICommand? Command
        {
            get;
            set;
        }

        /// <summary>
        /// The command's icon and text on its <see cref="AccentColor"/>, or
        /// null (the default) for what its theme chooses.
        /// </summary>
        public Color? ForegroundColor
        {
            get;
            set;
        }

        public Color AccentColor
        {
            get;
            set;
        }

        public Bitmap? Icon
        {
            get;
            set;
        }

        public string? Text
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
            object? parameter)
        {
            return this.Command?.CanExecute(parameter) ?? false;
        }

        public void Execute(
            object? parameter)
        {
            this.Command?.Execute(parameter);
        }
    }
}
