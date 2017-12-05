using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForms.Test
{
    public sealed class ThemeResources
    {
        private readonly List<Bitmap> _bitmapResources = new List<Bitmap>();

        public static ThemeResources Default = new ThemeResources();

        private ThemeResources()
        {
            this.AboutLogo = this.AddBitmap("AboutLogo.png");
        }

        public Bitmap AboutLogo { get; }

        public async Task LoadResourcesAsync()
        {
            foreach (Bitmap bitmap in this._bitmapResources)
            {
                await bitmap.LoadAsync();
            }
        }

        private Bitmap AddBitmap(
            ImageSource source)
        {
            var resource = new Bitmap(source);
            this._bitmapResources.Add(resource);
            return resource;
        }
    }
}
