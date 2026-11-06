using System;

namespace XForma
{
    /// <summary>
    /// Where an <see cref="Primitives.Image"/> gets its picture.
    /// </summary>
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

    /// <summary>
    /// An <see cref="ImageSource"/> showing an image file.
    /// </summary>
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

    /// <summary>
    /// An <see cref="ImageSource"/> showing a <see cref="Bitmap"/>.
    /// </summary>
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
