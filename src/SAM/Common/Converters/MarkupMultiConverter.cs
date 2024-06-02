using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SAM.Converters;

[MarkupExtensionReturnType(typeof(MarkupMultiConverter))]
public abstract class MarkupMultiConverter : MarkupExtension, IValueConverter, IMultiValueConverter
{
    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

    public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

    public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
