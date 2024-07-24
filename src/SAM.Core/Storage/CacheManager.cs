using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace SAM.Core.Storage;

public static class CacheManager
{
    private static readonly ILog log = LogManager.GetLogger(typeof(CacheManager));

    // TODO: remove public access to the StorageManager
    public static IStorageManager StorageManager { get; } = LocalStorageManager.Default;
        
    public static void CacheBytes(ICacheKey key, byte[] bytes, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

        StorageManager.SaveBytes(filePath, bytes, overwrite);

        if (!key.HasExpiration) return;

        StorageManager.UpdateCacheMetadata(filePath);
    }

    public static Task CacheBytesAsync(ICacheKey key, byte[] bytes, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

        var task = StorageManager.SaveBytesAsync(filePath, bytes, overwrite);

        if (!key.HasExpiration) return task;

        StorageManager.UpdateCacheMetadata(filePath);

        return task;
    }

    public static void CacheObject(ICacheKey key, object target, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var targetObjectJson = JsonConvert.SerializeObject(target, settings);

        StorageManager.SaveText(filePath, targetObjectJson, overwrite);

        if (!key.HasExpiration) return;

        StorageManager.UpdateCacheMetadata(filePath);
    }

    public static Task CacheObjectAsync(ICacheKey key, object target, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
        
        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var targetObjectJson = JsonConvert.SerializeObject(target, settings);

        var task = StorageManager.SaveTextAsync(filePath, targetObjectJson, overwrite);

        if (!key.HasExpiration) return task;

        StorageManager.UpdateCacheMetadata(filePath);

        return task;
    }

    public static void CacheText(ICacheKey key, string text, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
        StorageManager.SaveText(filePath, text, overwrite);

        if (!key.HasExpiration) return;

        StorageManager.UpdateCacheMetadata(filePath);
    }

    public static Task CacheTextAsync(ICacheKey key, string text, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
        var task = StorageManager.SaveTextAsync(filePath, text, overwrite);
            
        if (!key.HasExpiration) return task;

        StorageManager.UpdateCacheMetadata(filePath);

        return task;
    }

    public static Uri CacheImage(ICacheKey key, Image img, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

        StorageManager.SaveImage(filePath, img, overwrite);

        if (!key.HasExpiration)
        {
            var fullPath = Path.Join(StorageManager.ApplicationStoragePath, filePath);

            return new (fullPath);
        }

        StorageManager.UpdateCacheMetadata(filePath);

        return new (filePath);
    }

    public static async Task<Uri> CacheImageAsync(ICacheKey key, Image img, bool overwrite = true)
    {
        var filePath = key?.GetFullPath();
            
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));

        await StorageManager.SaveImageAsync(filePath, img, overwrite);

        if (!key.HasExpiration)
        {
            var fullPath = Path.Join(StorageManager.ApplicationStoragePath, filePath);

            return new (fullPath);
        }

        StorageManager.UpdateCacheMetadata(filePath);

        return new (filePath);
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
            
        if (IsExpired(key, filePath))
        {
            bytes = null;
            return false;
        }

        try
        {
            bytes = StorageManager.GetBytes(filePath);
            return bytes != null;
        }
        catch
        {
            bytes = null;
            return false;
        }
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
            
        if (IsExpired(key, filePath))
        {
            img = null;
            return false;
        }

        try
        {
            img = StorageManager.GetImageFile(filePath);
            return img != null;
        }
        catch
        {
            img = null;
            return false;
        }
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

    public static Task<Image> GetImageFileAsync(ICacheKey key)
    {
        var filePath = key.GetFullPath();

        if (!StorageManager.FileExists(filePath))
        {
            throw new FileNotFoundException(filePath);
        }
            
        if (IsExpired(key, filePath))
        {
            return null;
        }

        return StorageManager.GetImageFileAsync(filePath);
    }
        
    public static bool TryPopulateObject<T>(ICacheKey key, T target)
    {
        var filePath = key?.GetFullPath();

        if (string.IsNullOrEmpty(filePath)) return false;
        if (target == null) return false;

        if (!StorageManager.FileExists(filePath))
        {
            return false;
        }
        
        if (IsExpired(key, filePath))
        {
            return false;
        }

        try
        {
            var fileText = StorageManager.GetTextFile(filePath);

            JsonConvert.PopulateObject(fileText, target);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<T> GetObjectAsync<T>(ICacheKey key)
    {
        var filePath = key?.GetFullPath();

        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(key));
            
        if (!StorageManager.FileExists(filePath))
        {
            return default;
        }
            
        if (IsExpired(key, filePath))
        {
            return default;
        }

        try
        {
            var fileText = await StorageManager.GetTextFileAsync(filePath);

            var cachedObject = JsonConvert.DeserializeObject<T>(fileText);

            return cachedObject;
        }
        catch
        {
            return default;
        }
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
            
        if (IsExpired(key, filePath))
        {
            cachedObject = default;
            return false;
        }

        try
        {
            var fileText = StorageManager.GetTextFile(filePath);

            cachedObject = JsonConvert.DeserializeObject<T>(fileText);

            return cachedObject != null;
        }
        catch
        {
            cachedObject = default;

            return false;
        }
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

        if (IsExpired(key, filePath))
        {
            fileText = null;
            return false;
        }
            
        try
        {
            fileText = StorageManager.GetTextFile(filePath);
            return fileText != null;
        }
        catch
        {
            fileText = null;
            return false;
        }
    }

    public static string GetTextFile(ICacheKey key)
    {
        var filePath = key.GetFullPath();

        if (!StorageManager.FileExists(filePath))
        {
            throw new FileNotFoundException(filePath);
        }
            
        if (IsExpired(key, filePath))
        {
            // TODO: this should probably not return null in the event the cache is expired
            return null;
        }

        return StorageManager.GetTextFile(filePath);
    }

    public static Uri GetFile(ICacheKey key)
    {
        try
        {
            var filePath = key.GetFullPath();

            if (IsExpired(key, filePath))
            {
                return null;
            }
                
            var uri = StorageManager.GetFile(filePath);

            return uri;
        }
        catch
        {
            return null;
        }
    }

    public static bool TryGetFile(ICacheKey key, out Uri uri)
    {
        uri = default;

        try
        {
            var filePath = key.GetFullPath();

            if (IsExpired(key, filePath))
            {
                return false;
            }
                
            uri = StorageManager.GetFile(filePath);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsExpired(ICacheKey key, string fileName)
    {
        // if it doesn't have an expiration then it's never expired
        if (!key.HasExpiration)
        {
            return false;
        }

        var dateCreated = StorageManager.GetDateCreated(fileName) ?? DateTime.Now;
        var expireDate = dateCreated.AddDays(key.DaysValid!.Value);
        var isValid = DateTime.Now <= expireDate;

        return !isValid;
    }
}
