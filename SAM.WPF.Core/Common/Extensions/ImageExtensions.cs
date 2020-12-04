using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Size = System.Drawing.Size;

namespace SAM.WPF.Core.Extensions
{
    public static class ImageExtensions
    {

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public static ImageSource ToImageSource([NotNull] this Image value)
        {
            //var image = value;
            //var bitmap = new BitmapImage();

            //using var ms = new MemoryStream();

            //image.Save(ms, value.RawFormat);
            //ms.Seek(0, SeekOrigin.Begin);

            //bitmap.BeginInit();
            //bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //bitmap.UriSource = null;
            //bitmap.StreamSource = ms;
            //bitmap.EndInit();

            //return bitmap;

            var bitmap = new Bitmap(value);
            var bmpPtr = bitmap.GetHbitmap();
            var bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmpPtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bmpSource.Freeze();

            DeleteObject(bmpPtr);

            return bmpSource;
        }

        public static Image ChangeDpiAndSize([NotNull] this Image value, float dpi)
        {
            return ChangeDpiAndSize(value,dpi,dpi);
        }

        public static Image ChangeDpiAndSize([NotNull] this Image value, float xDpi,float yDpi)
        {
            var img = (Bitmap) value;
            var oldDpiX = img.HorizontalResolution;
            var oldDpiY = img.VerticalResolution;
            var scaleX = xDpi / oldDpiX;
            var scaleY = yDpi / oldDpiY;
            var newSize = new Size((int)(img.Width * scaleX), (int)(img.Height * scaleY));
            var result = new Bitmap(newSize.Width, newSize.Height);

            using var canvas = Graphics.FromImage(result);

            img.SetResolution(xDpi, yDpi);

            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
            canvas.DrawImage(img, 0, 0, newSize.Width, newSize.Height);

            return result;

        }


    }
}
