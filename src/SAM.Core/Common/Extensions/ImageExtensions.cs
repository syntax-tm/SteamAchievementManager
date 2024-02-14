using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Size = System.Drawing.Size;

namespace SAM.Core.Extensions;

public static class ImageExtensions
{
	public static ImageSource ToImageSource ([NotNull] this Image value)
	{
		var bitmap = new Bitmap(value);
		var hBmp = bitmap.GetHbitmap();
		var bmpPtr = new HGDIOBJ(hBmp);
		var bmpSource = Imaging.CreateBitmapSourceFromHBitmap(bmpPtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		bmpSource.Freeze();

		PInvoke.DeleteObject(bmpPtr);

		return bmpSource;
	}

	public static Image ChangeDpiAndSize ([NotNull] this Image value, float dpi)
	{
		return ChangeDpiAndSize(value, dpi, dpi);
	}

	public static Image ChangeDpiAndSize ([NotNull] this Image value, float xDpi, float yDpi)
	{
		var img = (Bitmap) value;
		var oldDpiX = img.HorizontalResolution;
		var oldDpiY = img.VerticalResolution;
		var scaleX = xDpi / oldDpiX;
		var scaleY = yDpi / oldDpiY;
		var newSize = new Size((int) (img.Width * scaleX), (int) (img.Height * scaleY));
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
