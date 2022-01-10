using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SAM.Core.Controls
{
    public class AutoDisableImage : Image
    {
        protected bool IsGrayscaled => Source is FormatConvertedBitmap;
        
        static AutoDisableImage()
        {
            IsEnabledProperty.OverrideMetadata(typeof(AutoDisableImage), new FrameworkPropertyMetadata(true, OnAutoDisableImagePropertyChanged));
            SourceProperty.OverrideMetadata(typeof(AutoDisableImage), new FrameworkPropertyMetadata(null, OnAutoDisableImagePropertyChanged));
        }
        
        private static void OnAutoDisableImagePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            if (source is not AutoDisableImage img || img.IsEnabled != img.IsGrayscaled) return;

            img.UpdateImage();
        }

        protected void UpdateImage()
        {
            if (Source == null) return;

            if (IsEnabled)
            {
                if (!IsGrayscaled) return;

                Source = ((FormatConvertedBitmap)Source).Source;
                OpacityMask = null;
            }
            else
            {
                if (IsGrayscaled) return;
                if (Source is not BitmapSource bitmapImage) return;

                Source = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray8, null, 0);
                OpacityMask = new ImageBrush(bitmapImage);
            }
        }
    }

}
