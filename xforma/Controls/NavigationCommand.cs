using System;
using System.Windows.Input;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
{
    /// <summary>
    /// A command that opens a URI.
    /// </summary>
    public class UriNavigationCommand : ICommand
    {
        public UriNavigationCommand()
        {
        }

        public Uri? NavigationUri
        {
            get;
            set;
        }

        //public string Text
        //{
        //    get;
        //    set;
        //}

        //public Color AccentColor
        //{
        //    get;
        //    set;
        //}

        // A URI can always be opened
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(
            object? parameter)
        {
            return true;
        }

        public void Execute(
            object? parameter)
        {
            Uri? navigationUri = this.NavigationUri;
            if (null != navigationUri)
            {
                Application.Current.OpenUri(navigationUri);
            }
        }
    }
}
