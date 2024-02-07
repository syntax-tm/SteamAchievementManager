using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

			CreateFileDirectory(path);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            img.Save(path, img.RawFormat);
        }

        public Task SaveImageAsync(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            return Task.Run(() => SaveImage(fileName, img, overwrite));
        }

        public void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

			CreateFileDirectory(path);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            File.WriteAllText(path, text);
        }

        public Task SaveTextAsync(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

			CreateFileDirectory(path);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            return File.WriteAllTextAsync(path, text);
        }
        
        public void SaveBytes(string fileName, byte[] bytes, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

			CreateFileDirectory(path);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            File.WriteAllBytes(path, bytes);
        }

        [NotNull]
        public Task SaveBytesAsync(string fileName, byte[] bytes, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

			CreateFileDirectory(path);

            if (!overwrite)
            {
                if (File.Exists(path))
                {
                    throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
                }
            }

            return File.WriteAllBytesAsync(path, bytes);
        }

        public Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var img = Image.FromFile(path);
            
            return img;
        }

        public async Task<Image> GetImageFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var bytes = await GetBytesAsync(fileName);
            using var ms = new MemoryStream(bytes);
            var img = Image.FromStream(ms);

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
        
        public async Task<string> GetTextFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var fileText = await File.ReadAllTextAsync(path);

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
        
        public async Task<byte[]> GetBytesAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            var path = Path.Combine(ApplicationStoragePath, fileName);
            
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(fileName));
            
            var bytes = await File.ReadAllBytesAsync(path);
            
            return bytes;
        }

        public static void CreateFileDirectory(string fullPath)
        {
			ArgumentNullException.ThrowIfNull(fullPath);

			var path = Directory.GetParent(fullPath);
            
            Directory.CreateDirectory(path.FullName);
        }

        public void CreateDirectory(string directory)
        {
			ArgumentNullException.ThrowIfNull(directory);

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
