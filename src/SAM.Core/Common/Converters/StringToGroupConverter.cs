using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SAM.Core.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class StringToGroupConverter : IValueConverter
{
	public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not string text)
		{
			return null;
		}

		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}

		var formatted = text.Trim().ToUpper();
		var firstChar = formatted [0];

		if (char.IsLetter(firstChar))
		{
			return firstChar;
		}

		if (char.IsDigit(firstChar))
		{
			return "#";
		}

		if (char.IsSeparator(firstChar) ||
			char.IsPunctuation(firstChar) ||
			char.IsSymbol(firstChar))
		{
			return "-";
		}

		return "-";
	}

	public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

public class StringToGroupConverterExtension : MarkupExtension
{
	public IValueConverter ItemConverter
	{
		get; set;
	}

	public StringToGroupConverterExtension ()
	{

	}

	public StringToGroupConverterExtension (IValueConverter itemConverter)
	{
		ItemConverter = itemConverter;
	}

	public override object ProvideValue (IServiceProvider serviceProvider)
	{
		return new StringToGroupConverter();
	}
}
