using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SAM.WPF.Core.Converters
{

    [ValueConversion(typeof(string), typeof(Image))]
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string filePath)
            {
                return null;
            }

            if (!File.Exists(filePath)) return null;

            return Image.FromFile(filePath);
        }
 
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToImageConverterExtension : MarkupExtension
    {
        public IValueConverter ItemConverter { get; set; }

        public StringToImageConverterExtension() { }
        public StringToImageConverterExtension(IValueConverter itemConverter) => ItemConverter = itemConverter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new StringToImageConverter();
        }
    }

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string filePath)
            {
                return null;
            }

            var uri = new Uri(filePath);
            
            if (uri.IsFile && !File.Exists(uri.ToString())) return null;

            return new BitmapImage(uri);
        }
 
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToImageSourceConverterExtension : MarkupExtension
    {
        public IValueConverter ItemConverter { get; set; }

        public StringToImageSourceConverterExtension() { }
        public StringToImageSourceConverterExtension(IValueConverter itemConverter) => ItemConverter = itemConverter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new StringToImageSourceConverter();
        }
    }
}
