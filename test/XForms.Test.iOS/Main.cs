using XForms.Themes.Basic;

namespace XForms.Test.iOS
{
    public static class Program
    {
        // The app's entry point
        static void Main(
            string[] args)
        {
            XFormsApp.Run(args, new TestApp(), new BasicTheme());
        }
    }
}
