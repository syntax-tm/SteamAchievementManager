using System;
using System.Drawing;
using System.IO;
using log4net;

namespace SAM.Core.Storage
{
    public class LocalStorageManager : IStorageManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LocalStorageManager));

        private static readonly object syncLock = new ();
        private static LocalStorageManager _instance;
        
        public string LocalAppDataPath { get; }
        public string ApplicationStoragePath { get; }
        
        protected LocalStorageManager()
        {
            LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ApplicationStoragePath = Path.Combine(LocalAppDataPath, nameof(SAM));
        }

        public static LocalStorageManager Default
        {
            get
            {
                if (_instance != null) return _instance;
                lock (syncLock)
                {
                    _instance = new ();
                }
                return _instance;
            }
        }

        public void SaveImage(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            img.Save(path, img.RawFormat);
        }

        public void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            File.WriteAllText(path, text);
        }
        
        public void SaveBytes(string fileName, byte[] bytes, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            File.WriteAllBytes(path, bytes);
        }

        public Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var img = Image.FromFile(path);
            
            return img;
        }

        public string GetTextFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var fileText = File.ReadAllText(path);

            return fileText;
        }
        
        public byte[] GetBytes(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var bytes = File.ReadAllBytes(path);
            
            return bytes;
        }

        public void CreateDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            
            var path = Path.Combine(ApplicationStoragePath, directory);
            
            Directory.CreateDirectory(path);
        }
        
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            var exists = File.Exists(path);

            return exists;
        }

    }
}
