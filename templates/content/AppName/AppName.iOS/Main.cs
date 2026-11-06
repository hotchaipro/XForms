using XForma;
using XForma.Themes.Basic;

namespace AppName
{
    public static class Program
    {
        static void Main(
            string[] args)
        {
            XFormaApp.Run(args, new App(), new BasicTheme());
        }
    }
}
