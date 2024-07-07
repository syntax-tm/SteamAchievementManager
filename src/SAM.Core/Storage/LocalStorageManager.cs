using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
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

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
            }

            img.Save(path, img.RawFormat);
        }

        public Task SaveImageAsync(string fileName, Image img, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            CreateFileDirectory(path);

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
            }

            img.Save(path, img.RawFormat);

            return Task.CompletedTask;
        }

        public void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            CreateFileDirectory(path);

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
            }

            File.WriteAllText(path, text);
        }

        public async Task SaveTextAsync(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            CreateFileDirectory(path);

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
            }

            await File.WriteAllTextAsync(path, text).ConfigureAwait(false);
        }

        public void SaveBytes(string fileName, byte[] bytes, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            CreateFileDirectory(path);

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
            }

            File.WriteAllBytes(path, bytes);
        }

        public Task SaveBytesAsync(string fileName, byte[] bytes, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            var path = Path.Combine(ApplicationStoragePath, fileName);

            CreateFileDirectory(path);

            if (!overwrite && File.Exists(path))
            {
                throw new InvalidOperationException($"File '{fileName}' exists and {nameof(overwrite)} was not specified.");
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

        public void CreateFileDirectory(string fullPath)
        {
            if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

            var path = Directory.GetParent(fullPath);

            Debug.Assert(path != null, $"{nameof(path)} is null");

            Directory.CreateDirectory(path.FullName);
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

        public DateTime? GetDateCreated(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!FileExists(fileName)) return null;

            var fullName = Path.Combine(ApplicationStoragePath, fileName);
            var fi = new FileInfo(fullName);

            return fi.CreationTime;
        }

        public DateTime? GetDateModified(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!FileExists(fileName)) return null;

            var fullName = Path.Combine(ApplicationStoragePath, fileName);
            var fi = new FileInfo(fullName);

            return fi.LastWriteTime;
        }

        public void UpdateCacheMetadata(string _)
        {
            // local storage does not need to update this metadata as it's handled by the file system
        }

        public Uri GetFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!FileExists(fileName)) return null;

            var fullName = Path.Combine(ApplicationStoragePath, fileName);

            return new (fullName);
        }
    }
}
