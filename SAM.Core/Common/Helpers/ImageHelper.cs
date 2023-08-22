using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;

namespace SAM.Core
{
    public static class ImageHelper
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(ImageHelper));
        
        // TODO: cache images from urls locally
        public static ImageSource CreateSource(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            var uri = new Uri(url);

            return CreateSource(uri);
        }

        // TODO: cache images from urls locally
        public static ImageSource CreateSource(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var bmp = new BitmapImage();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.EndInit();

            return bmp;
        }
    }
}
