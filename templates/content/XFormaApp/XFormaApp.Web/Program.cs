using System.Threading.Tasks;
using XForma;
using XForma.Themes.Basic;

namespace XFormaApp
{
    public static class Program
    {
        public static async Task Main()
        {
            await XFormaApp.RunAsync("XFormaApp", new App(), new BasicTheme());
        }
    }
}
