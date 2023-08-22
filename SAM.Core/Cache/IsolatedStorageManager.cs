using System;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using log4net;

namespace SAM.Core
{
    public static class IsolatedStorageManager
    {

        private static readonly ILog log = LogManager.GetLogger(nameof(IsolatedStorageManager));

        public static void SaveImage(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            using var isoStorage = GetStore();
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

            img.Save(file, img.RawFormat);
        }

        public static void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            using var isoStorage = GetStore();
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);
            using var writer = new StreamWriter(file);

            writer.WriteLine(text);
        }

        public static Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));
            
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

            var img = Image.FromStream(file);
            
            return img;
        }

        public static string GetTextFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();
            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);
            using var reader = new StreamReader(file);
            var fileText = reader.ReadToEnd();

            return fileText;
        }

        public static void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            using var isoStorage = GetStore();
            if (isoStorage.DirectoryExists(path)) return;

            isoStorage.CreateDirectory(path);
        }
        
        public static bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            return isoStorage.FileExists(fileName);
        }

        private static readonly object _initLock = new ();

        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;

            lock (_initLock)
            {
                using var store = IsolatedStorageFile.GetMachineStoreForAssembly();

                var fi = store.GetType().GetField("_rootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
                var path = (string) fi.GetValue(store);

                log.Debug($"IsolatedStorageFile Path: '{path}'");
                    
                if (!store.DirectoryExists("apps")) store.CreateDirectory("apps");

                _initialized = true;
            }
        }

        public static IsolatedStorageFile GetStore()
        {
            //var store = IsolatedStorageFile.GetMachineStoreForAssembly();
            //var path = isoStorage.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isoStorage).ToString();
            
            return IsolatedStorageFile.GetMachineStoreForAssembly();
        }

    }
}
