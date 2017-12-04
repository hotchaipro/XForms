using System;

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
