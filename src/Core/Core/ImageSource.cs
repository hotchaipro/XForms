using System;

namespace XForms
{
    public abstract class ImageSource
    {
        internal ImageSource()
        {
        }

        public static implicit operator ImageSource(
            string source)
        {
            return new FileImageSource(source);
        }

        public static implicit operator ImageSource(
            Bitmap source)
        {
            return new BitmapImageSource(source);
        }
    }

    public sealed class FileImageSource : ImageSource
    {
        public FileImageSource(
            string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.Path = path;
        }

        public string Path
        {
            get;
        }
    }

    public sealed class BitmapImageSource : ImageSource
    {
        public BitmapImageSource(
            Bitmap bitmap)
        {
            if (null == bitmap)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            this.Bitmap = bitmap;
        }

        public Bitmap Bitmap
        {
            get;
        }
    }
}
