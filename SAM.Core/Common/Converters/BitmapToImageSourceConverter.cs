using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SAM.Core.Converters
{
    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Bitmap bmp)
                return null;

            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
 
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BitmapToImageSourceConverterExtension : MarkupExtension
    {
        public IValueConverter ItemConverter { get; set; }

        public BitmapToImageSourceConverterExtension() { }
#pragma warning disable IDE0021 // Use block body for constructors
        public BitmapToImageSourceConverterExtension(IValueConverter itemConverter) => ItemConverter = itemConverter;
#pragma warning restore IDE0021 // Use block body for constructors

        public override object ProvideValue(IServiceProvider serviceProvider) => new BitmapToImageSourceConverter();
    }
}
