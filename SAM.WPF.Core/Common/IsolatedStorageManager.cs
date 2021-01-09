using System;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using log4net;

namespace SAM.WPF.Core
{
    public class IsolatedStorageManager
    {

        private static readonly ILog log = LogManager.GetLogger(nameof(IsolatedStorageManager));

        private static bool _shownPathMessage;
        private static bool _createdAppsDirectory;

        public static void SaveImage(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            //if (isoStorage.FileExists(fileName))
            //{
            //    if (!overwrite) throw new ArgumentException(nameof(fileName));
            //    isoStorage.DeleteFile(fileName);
            //}

            //using var file = isoStorage.CreateFile(fileName);

            using (var isoStorage = GetStore())
            using (var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage))
            {
                img.Save(file, img.RawFormat);
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

            //if (isoStorage.FileExists(fileName))
            //{
            //    if (!overwrite) throw new ArgumentException(nameof(fileName));
            //    isoStorage.DeleteFile(fileName);
            //}

            //using var file = isoStorage.CreateFile(fileName);
            
            using (var isoStorage = GetStore())
            using (var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, isoStorage))
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine(text);
            }
        }

        public static Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using (var isoStorage = GetStore())
            {
                if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

                //using var file = isoStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read);
                //using var reader = new StreamReader(file);

                using (var file = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.ReadWrite, isoStorage))
                {
                    var img = Image.FromStream(file);

                    //var bmp = new Bitmap(file); //Image.FromStream(file);

                    return img;
                }
            }
        }

        public static string GetTextFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using (var isoStorage = GetStore())
            {
                if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

                using (var file = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.ReadWrite, isoStorage))
                using (var reader = new StreamReader(file))
                {
                    var fileText = reader.ReadToEnd();

                    return fileText;
                }
            }
        }

        public static void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            using (var isoStorage = GetStore())
            {
                if (isoStorage.DirectoryExists(path)) return;

                isoStorage.CreateDirectory(path);
            }
        }
        
        public static bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            return isoStorage.FileExists(fileName);
        }

        public static IsolatedStorageFile GetStore()
        {
            var store = IsolatedStorageFile.GetMachineStoreForAssembly();

            //var path = isoStorage.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isoStorage).ToString();

            if (!_shownPathMessage)
            {
                var fi = store.GetType().GetField("_rootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                var path = (string) fi.GetValue(store);

                log.Debug($"IsolatedStorageFile Path: '{path}'");

                _shownPathMessage = true;
            }

            if (!_createdAppsDirectory)
            {
                if (!store.DirectoryExists("apps")) store.CreateDirectory("apps");

                _createdAppsDirectory = true;
            }

            return store;
        }

    }
}
