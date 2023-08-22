using System;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using Image = System.Drawing.Image;

namespace SAM.WPF.Core
{
    public class IsolatedStorageManager
    {
        
        public static void SaveImage(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (isoStorage.FileExists(fileName))
            {
                if (!overwrite) throw new ArgumentException(nameof(fileName));
                isoStorage.DeleteFile(fileName);
            }

            //using var file = isoStorage.CreateFile(fileName);
            
            //IImageEncoder encoder;

            //var ext = Path.GetExtension(fileName);

            //switch (ext)
            //{
            //    case "bmp":
            //        encoder = new BmpEncoder();
            //        break;
            //    case "jpg":
            //    case "jpeg":
            //        encoder = new JpegEncoder();
            //        break;
            //    case "png":
            //        encoder = new PngEncoder();
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(ext), ext, $"'{ext}' file type is not supported.");
            //}

            //isImg.Save(file, encoder);

            //using var writer = new StreamWriter(file);
            //img.Save(file, img.RawFormat);
        }

        public static void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (isoStorage.FileExists(fileName))
            {
                if (!overwrite) throw new ArgumentException(nameof(fileName));
                isoStorage.DeleteFile(fileName);
            }

            using var file = isoStorage.CreateFile(fileName);
            using var writer = new StreamWriter(file);
            writer.WriteLine(text);
        }

        public static Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

            using var file = isoStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read);
            //using var reader = new StreamReader(file);

            var bmp = new Bitmap(file); //Image.FromStream(file);

            return bmp;
        }

        public static string GetTextFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

            using var file = isoStorage.OpenFile(fileName, FileMode.Open);
            using var reader = new StreamReader(file);

            var fileText = reader.ReadToEnd();

            return fileText;
        }

        public static void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            if (DirectoryExists(path))
            {
                return;
            }

            using var isoStorage = GetStore();

            isoStorage.CreateDirectory(path);
        }

        public static bool DirectoryExists(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) return true;

            using var isoStorage = GetStore();

            return isoStorage.DirectoryExists(path);
        }

        public static bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            return isoStorage.FileExists(fileName);
        }

        public static IsolatedStorageFile GetStore()
        {
            var isoStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

            if (!isoStorage.DirectoryExists("apps")) isoStorage.CreateDirectory("apps");

            return isoStorage;
        }

    }
}
