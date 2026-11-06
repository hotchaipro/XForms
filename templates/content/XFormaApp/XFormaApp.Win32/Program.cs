using System;
using XForma;
using XForma.Themes.Basic;

namespace XFormaApp
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            XFormaApp.Run("XFormaApp", new App(), new BasicTheme());
        }
    }
}
