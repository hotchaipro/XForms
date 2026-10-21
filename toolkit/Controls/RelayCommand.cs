using System;
using System.Windows.Input;

namespace XForms.Toolkit
{
    /// <summary>
    /// A command that runs a delegate, optionally only when another delegate
    /// says it can (as CommunityToolkit.Mvvm's RelayCommand).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(
            Action execute,
            Func<bool> canExecute = null)
            : this(
                  (_) => execute(),
                  (null != canExecute) ? (_) => canExecute() : null)
        {
            if (null == execute)
            {
                throw new ArgumentNullException(nameof(execute));
            }
        }

        public RelayCommand(
            Action<object> execute,
            Func<object, bool> canExecute = null)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(
            object parameter)
        {
            return this._canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(
            object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this._execute(parameter);
            }
        }

        /// <summary>
        /// Tells the controls using the command to ask <see cref="CanExecute"/>
        /// again.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
