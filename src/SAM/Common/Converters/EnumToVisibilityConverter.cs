using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using SAM.Core.Extensions;

namespace SAM.Converters;

[ValueConversion(typeof (Enum), typeof (Visibility))]
public class EnumToVisibilityConverter : IValueConverter
{
    public bool Inverse { get; set; }
    public bool HiddenInsteadOfCollapsed { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var trueResult = Visibility.Visible;
        var falseResult = HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed;

        if (value == null) return null;
        if (value is not Enum valueEnum)
        {
            return Inverse ? falseResult : trueResult;
        }

        if (parameter == null) return null;
        if (parameter is not Enum paramEnum)
        {
            return Inverse ? falseResult : trueResult;
        }

        var equal = Equals(valueEnum, paramEnum);

        if (Inverse)
        {
            return equal ? falseResult : trueResult;
        }

        return equal
            ? trueResult
            : falseResult;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class EnumToVisibilityConverterExtension : MarkupExtension
{
    public bool Inverse { get; set; }
    public bool HiddenInsteadOfCollapsed { get; set; }
    public IValueConverter ItemConverter { get; set; }

    public EnumToVisibilityConverterExtension() { }
    public EnumToVisibilityConverterExtension(IValueConverter itemConverter)
    {
        ItemConverter = itemConverter;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new EnumToVisibilityConverter
        {
            Inverse = Inverse,
            HiddenInsteadOfCollapsed = HiddenInsteadOfCollapsed
        };
    }
}
