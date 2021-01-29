using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core.Converters
{
    [ValueConversion(typeof (byte?), typeof (string))]
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (!(value is Enum valueEnum))
            {
                return value.ToString();
            }
            return valueEnum.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EnumDescriptionConverterExtension : MarkupExtension
    {
        public IValueConverter ItemConverter { get; set; }

        public EnumDescriptionConverterExtension()
        {

        }

        public EnumDescriptionConverterExtension(IValueConverter itemConverter)
        {
            ItemConverter = itemConverter;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new EnumDescriptionConverter();
        }
    }
}
