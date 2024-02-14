using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace SAM.Core.Converters;

[ValueConversion(typeof(Bitmap), typeof(ImageSource))]
public class BitmapToImageSourceConverter : IValueConverter
{
	public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Bitmap bmp)
		{
			return null;
		}

		var handle = bmp.GetHbitmap();
		var hBmp = new HGDIOBJ(handle);
		try
		{
			return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}
		finally { PInvoke.DeleteObject(hBmp); }
	}

	public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class BitmapToImageSourceConverterExtension : MarkupExtension
{
	public IValueConverter ItemConverter
	{
		get; set;
	}

	public BitmapToImageSourceConverterExtension ()
	{
	}
#pragma warning disable IDE0021 // Use block body for constructors
	public BitmapToImageSourceConverterExtension (IValueConverter itemConverter) => ItemConverter = itemConverter;
#pragma warning restore IDE0021 // Use block body for constructors

	public override object ProvideValue (IServiceProvider serviceProvider)
	{
		return new BitmapToImageSourceConverter();
	}
}
