﻿using System;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using log4net;

namespace SAM.Core
{
    public class IsolatedStorageManager : IStorageManager
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(IsolatedStorageManager));
        private static readonly object syncLock = new ();
        private static IsolatedStorageManager _instance;
        
        public string Path { get; }

        protected IsolatedStorageManager()
        {
            using var store = IsolatedStorageFile.GetMachineStoreForAssembly();

            var fi = store.GetType().GetField(@"_rootDirectory", BindingFlags.NonPublic | BindingFlags.Instance);
            var path = (string) fi!.GetValue(store);

            log.Debug($"IsolatedStorageFile Path: '{path}'");

            Path = path;

            if (!store.DirectoryExists(@"apps")) store.CreateDirectory(@"apps");
        }

        public static IsolatedStorageManager Default
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
            
            using var isoStorage = GetStore();
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);

            img.Save(file, img.RawFormat);
        }

        public void SaveText(string fileName, string text, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);
            
            using var isoStorage = GetStore();
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, isoStorage);
            using var writer = new StreamWriter(file);

            writer.WriteLine(text);
        }

        public Image GetImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));
            
            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);

            var img = Image.FromStream(file);
            
            return img;
        }

        public string GetTextFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();
            if (!isoStorage.FileExists(fileName)) throw new FileNotFoundException(nameof(fileName));

            using var file = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None, isoStorage);
            using var reader = new StreamReader(file);
            var fileText = reader.ReadToEnd();

            return fileText;
        }

        public void CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            using var isoStorage = GetStore();
            if (isoStorage.DirectoryExists(path)) return;

            isoStorage.CreateDirectory(path);
        }
        
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(fileName);

            using var isoStorage = GetStore();

            return isoStorage.FileExists(fileName);
        }
        
        public IsolatedStorageFile GetStore()
        {
            return IsolatedStorageFile.GetMachineStoreForAssembly();
        }
    }
}
