using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;

namespace SAM.Core.Converters;

[ValueConversion(typeof(string), typeof(Image))]
public class StringToImageConverter : IValueConverter
{
	private readonly ILog log = LogManager.GetLogger(typeof(StringToImageConverter));

	public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not string filePath)
			{
			return null;
		}

		try
		{
			return Image.FromFile(filePath);
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to convert '{filePath}' to {nameof(Image)}. {e.Message}";
			log.Error(message, e);
			return null;
		}
	}

	public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class StringToImageConverterExtension : MarkupExtension
{
	public IValueConverter ItemConverter
	{
		get; set;
	}

	public StringToImageConverterExtension ()
	{
	}
	public StringToImageConverterExtension (IValueConverter itemConverter)
	{
		ItemConverter = itemConverter;
	}

	public override object ProvideValue (IServiceProvider serviceProvider) {
		return new StringToImageConverter();
	}
}

[ValueConversion(typeof(string), typeof(ImageSource))]
public class StringToImageSourceConverter : IValueConverter
{
	private readonly ILog log = LogManager.GetLogger(typeof(StringToImageSourceConverter));

	public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not string filePath)
		{
			return null;
		}

		try
		{
			return new BitmapImage(new(filePath));
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to convert '{filePath}' to {nameof(ImageSource)}. {e.Message}";
			log.Error(message, e);
			return null;
		}
	}

	public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class StringToImageSourceConverterExtension : MarkupExtension
{
	public IValueConverter ItemConverter
	{
		get; set;
	}

	public StringToImageSourceConverterExtension ()
	{
	}
	public StringToImageSourceConverterExtension (IValueConverter itemConverter)
	{
		ItemConverter = itemConverter;
	}

	public override object ProvideValue (IServiceProvider serviceProvider) {
		return new StringToImageSourceConverter();
	}
}
