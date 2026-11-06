using XForma;

namespace AppName
{
    /// <summary>
    /// The app: its pages, by the paths that open them.
    /// </summary>
    public sealed class App : Application
    {
        protected override void OnConfigureRoutes(
            Routes routes)
        {
            base.OnConfigureRoutes(routes);
            routes.Add("/", _ => new MainPage());
        }
    }
}
