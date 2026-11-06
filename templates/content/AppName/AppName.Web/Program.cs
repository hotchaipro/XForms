using System.Threading.Tasks;
using XForma;
using XForma.Themes.Basic;

namespace AppName
{
    public static class Program
    {
        public static async Task Main()
        {
            await XFormaApp.RunAsync("AppName", new App(), new BasicTheme());
        }
    }
}
