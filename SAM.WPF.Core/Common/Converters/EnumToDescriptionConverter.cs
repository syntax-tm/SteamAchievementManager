using System;
using System.Globalization;
using System.Windows.Data;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == null) return null;

            var type = parameter as Type;

            if (type == null) return null;

            var enumValue = (Enum) Enum.Parse(type, value.ToString());

            value = enumValue.GetDescription();
            return value ?? enumValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = value as string;
            return !string.IsNullOrWhiteSpace(enumValue) ? Enum.Parse(targetType, enumValue) : null;
        }
    }
}
