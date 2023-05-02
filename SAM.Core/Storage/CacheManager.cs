using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace SAM.Core.Storage
{
    public static class CacheManager
    {
        public static IStorageManager StorageManager { get; } = LocalStorageManager.Default;
        
        public static void CacheBytes(ICacheKey key, byte[] bytes, bool overwrite = true)
        {
            var filePath = key?.GetFullPath();
            
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

            StorageManager.SaveBytes(filePath, bytes, overwrite);
        }

        public static void CacheObject(ICacheKey key, object target, bool overwrite = true)
        {
            var filePath = key?.GetFullPath();
            
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

            var targetObjectJson = JsonConvert.SerializeObject(target, Formatting.Indented);

            StorageManager.SaveText(filePath, targetObjectJson, overwrite);
        }

        public static void CacheText(ICacheKey key, string text, bool overwrite = true)
        {
            var filePath = key?.GetFullPath();
            
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

            StorageManager.SaveText(filePath, text, overwrite);
        }

        public static void CacheImage(ICacheKey key, Image img, bool overwrite = true)
        {
            var filePath = key?.GetFullPath();
            
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

            StorageManager.SaveImage(filePath, img, overwrite);
        }
        
        public static bool TryGetBytes(ICacheKey key, out byte[] bytes)
        {
            var filePath = key?.GetFullPath();

            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
            if (!StorageManager.FileExists(filePath))
            {
                bytes = null;
                return false;
            }

            bytes = StorageManager.GetBytes(filePath);

            return true;
        }

        public static bool TryGetImageFile(ICacheKey key, out Image img)
        {
            var filePath = key?.GetFullPath();

            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
            if (!StorageManager.FileExists(filePath))
            {
                img = null;
                return false;
            }

            img = StorageManager.GetImageFile(filePath);

            return true;
        }

        public static Image GetImageFile(ICacheKey key)
        {
            var filePath = key.GetFullPath();

            if (!StorageManager.FileExists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            return StorageManager.GetImageFile(filePath);
        }
        
        public static bool TryPopulateObject<T>(ICacheKey key, T target)
        {
            var filePath = key?.GetFullPath();

            ArgumentException.ThrowIfNullOrEmpty(filePath);
            
            if (!StorageManager.FileExists(filePath))
            {
                return false;
            }
            
            var fileText = StorageManager.GetTextFile(filePath);
            
            JsonConvert.PopulateObject(fileText, target);

            return true;
        }

        public static bool TryGetObject<T>(ICacheKey key, out T cachedObject)
        {
            var filePath = key?.GetFullPath();

            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
            if (!StorageManager.FileExists(filePath))
            {
                cachedObject = default;
                return false;
            }
            
            var fileText = StorageManager.GetTextFile(filePath);

            cachedObject = JsonConvert.DeserializeObject<T>(fileText);

            return true;
        }
        
        public static bool TryGetTextFile(ICacheKey key, out string fileText)
        {
            var filePath = key?.GetFullPath();

            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
            if (!StorageManager.FileExists(filePath))
            {
                fileText = null;
                return false;
            }
            
            fileText = StorageManager.GetTextFile(filePath);
            
            return true;
        }

        public static string GetTextFile(ICacheKey key)
        {
            var filePath = key.GetFullPath();

            if (!StorageManager.FileExists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            return StorageManager.GetTextFile(filePath);
        }
    }
}
