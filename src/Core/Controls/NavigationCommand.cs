using System;

namespace XForms.Controls
{
    public class UriNavigationCommand : ICommand
    {
        public UriNavigationCommand()
        {
        }

        public Uri NavigationUri
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

        public bool CanExecute(
            object parameter)
        {
            return true;
        }

        public void Execute(
            object parameter)
        {
            Uri navigationUri = this.NavigationUri;
            if (null != navigationUri)
            {
                Application.Current.Platform.NavigateToUri(navigationUri);
            }
        }
    }
}
