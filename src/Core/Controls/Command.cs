using System;

namespace XForms.Controls
{
    public class Command : ICommand
    {
        public Command(
            Action action)
            : this(o => action())
        {
        }

        public Command(
            Action<object> action)
        {
            this.Action = action;
        }

        public Action<object> Action
        {
            get;
        }

        public bool CanExecute(
            object parameter)
        {
            return true;
        }

        public void Execute(
            object parameter)
        {
            this.Action?.Invoke(parameter);
        }
    }
}
