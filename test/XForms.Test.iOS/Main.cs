using XForms.iOS;
using XForms.Toolkit;

namespace XForms.Test.iOS
{
    public static class Program
    {
        // The app's entry point
        static void Main(
            string[] args)
        {
            IosToolkitServices.Register(() => IosApplication.Current?.RootViewController);
            IosApplication.Run(args, new TestApp());
        }
    }
}
