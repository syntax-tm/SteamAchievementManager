using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SAM.Core.Storage;

namespace SAM;

public static class LocalStorageManagerExtensions
{

    public static async Task<ImageSource> GetImageSourceFileAsync(this LocalStorageManager manager, string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

        var bytes = await manager.GetBytesAsync(fileName);
        using var ms = new MemoryStream(bytes);
        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.StreamSource = ms;
        img.EndInit();

        return img;
    }

}