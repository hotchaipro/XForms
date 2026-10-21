using XForms.iOS;

namespace XForms.Test.iOS
{
    public static class Program
    {
        // The app's entry point
        static void Main(
            string[] args)
        {
            IosApplication.Run(args, new TestApp());
        }
    }
}
