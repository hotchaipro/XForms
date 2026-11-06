using System;
using XForma;
using XForma.Themes.Basic;

namespace AppName
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            XFormaApp.Run("AppName", new App(), new BasicTheme());
        }
    }
}
