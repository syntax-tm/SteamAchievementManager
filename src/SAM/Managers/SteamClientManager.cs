using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using SAM.API;
using SAM.Core;

namespace SAM;

public static class SteamClientManager
{
    private record SteamAppImageCacheKey(uint Id, SteamImageType Type);

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

    private static readonly object syncLock = new ();
    private static readonly List<uint> gridCacheAppWarnings = [];
    private static readonly Dictionary<SteamAppImageCacheKey, string> cachedAppImagePaths = [];
    private static bool _isInitialized;
    private static string _steamInstallPath;
    private static string _cachePath;
    private static string _gridCachePath;

    public static uint AppId { get; private set; }
    public static string CurrentLanguage { get; private set; }

    public static string CachePath
    {
        get
        {
            if (_cachePath != null) return _cachePath;
            lock (syncLock)
            {
                _cachePath = Path.Join(SteamInstallPath, @"appcache", @"librarycache");
            }
            return _cachePath;
        }
    }

    public static string GridCachePath
    {
        get
        {
            if (_gridCachePath != null) return _gridCachePath;
            lock (syncLock)
            {
                var userId = SteamUserManager.GetActiveUser();

                _gridCachePath = Path.Join(SteamInstallPath, @"userdata", $"{userId}", @"config", @"grid");
            }
            return _gridCachePath;
        }
    }

    public static Client Default { get; private set; }

    public static string SteamInstallPath
    {
        get
        {
            if (_steamInstallPath != null) return _steamInstallPath;

            _steamInstallPath = Steam.GetInstallPath();

            return _steamInstallPath;
        }
    }

    public static void Init(uint appId)
    {
        if (_isInitialized) throw new SAMInitializationException($"The Steam {nameof(Client)} has already been initialized.");

        try
        {
            Default = new ();
            Default.Initialize(appId);

            AppId = appId;
            CurrentLanguage = Default.SteamApps008.GetCurrentGameLanguage();

            _isInitialized = true;
        }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to initialize the Steam client with app ID '{appId}'. {e.Message}";
            log.Error(message, e);

            throw new SAMInitializationException(message, e);
        }
    }

    // TODO: load user's grid images (if present)
    public static bool TryGetCachedAppImage(uint appId, SteamImageType type, out Image image)
    {
        try
        {
            if (!CachedAppImageExists(appId, type))
            {
                image = null;
                return false;
            }

            var imagePath = GetCachedAppImagePath(appId, type);

            image = Image.FromFile(imagePath);

            return true;
        }
        catch
        {
            image = null;
            return false;
        }
    }

    // TODO: load user's grid images (if present)
    public static bool TryGetCachedAppImageSource(uint appId, SteamImageType type, out ImageSource image)
    {
        try
        {
            if (!CachedAppImageExists(appId, type))
            {
                image = null;
                return false;
            }

            var imagePath = GetCachedAppImagePath(appId, type);

            image = new BitmapImage(new (imagePath));

            return true;
        }
        catch
        {
            image = null;
            return false;
        }
    }

    public static bool TryGetCachedAppImageUri(uint appId, SteamImageType type, out Uri uri)
    {
        try
        {
            if (!CachedAppImageExists(appId, type))
            {
                uri = null;
                return false;
            }

            var imagePath = GetCachedAppImagePath(appId, type);

            uri = new (imagePath);

            return true;
        }
        catch
        {
            uri = null;
            return false;
        }
    }

    public static bool CachedAppImageExists(uint appId, SteamImageType type)
    {
        try
        {
            var imagePath = GetCachedAppImagePath(appId, type);

            return File.Exists(imagePath);
        }
        catch
        {
            return false;
        }
    }

    public static Image GetCachedAppImage(uint appId, SteamImageType type)
    {
        string imagePath = null;

        try
        {
            if (!CachedAppImageExists(appId, type)) throw new FileNotFoundException($"A cached {type} image for app '{appId}' was not found.");

            imagePath = GetCachedAppImagePath(appId, type);

            return Image.FromFile(imagePath);
        }

        // images that aren't supported will throw an OutOfMemoryException
        catch (OutOfMemoryException oom)
        {
            var message = $"Failed to load {type:G} for app {appId} from '{imagePath}'.";

            throw new SAMException(message, oom);
        }
        catch (FileNotFoundException) { throw; }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to load the {type:G} for app {appId}. {e.Message}";

            throw new SAMException(message, e);
        }
    }

    private static string GetCachedAppImagePath(uint appId, SteamImageType type)
    {
        var fileName = GetAppImageName(appId, type);

        // if this is a grid icon the extension can vary, so search for a match
        if (SteamImageType.Grid.HasFlag(type))
        {
            // first check and see if we've already cached a valid image for this app and type
            var key = new SteamAppImageCacheKey(appId, type);

            if (cachedAppImagePaths.TryGetValue(key, out var path))
            {
                return path;
            }

            // search for a matching file name with any extension
            var searchPattern = $"{fileName}.*";

            var results = Directory.GetFiles(GridCachePath, searchPattern);

            // TODO: this will show multiple warnings for the same file
            // if they have multiple files saved for the app (with different extensions), just use
            // the first result after logging a warning
            if (results.Length > 1)
            {
                var warnedPreviously = gridCacheAppWarnings.Contains(appId);

                // first time warning for this app id
                if (!warnedPreviously)
                {
                    log.Warn($"Found {results.Length} {type:G} images for app {appId}.");

                    gridCacheAppWarnings.Add(appId);
                }
            }

            var result = results.FirstOrDefault();
            if (result == null)
            {
                return null;
            }

            var gridFileName = Path.GetFileName(result);

            // cache the result so that we don't have to calculate it again
            cachedAppImagePaths[key] = result;

            log.Info($"Found {SteamImageType.Grid} image '{gridFileName}' for app {appId}.");

            return result;
        }

        var imagePath = Path.Join(CachePath, fileName);

        return imagePath;
    }

    private static string GetAppImageName(uint appId, SteamImageType type)
    {
        return type switch
        {
            SteamImageType.GridLandscape   => $"{appId}",
            SteamImageType.GridPortrait    => $"{appId}p",
            SteamImageType.GridIcon        => $"{appId}_icon",
            SteamImageType.GridHero        => $"{appId}_hero",
            SteamImageType.Header          => $"{appId}_header.jpg",
            SteamImageType.Icon            => $"{appId}_icon.jpg",
            SteamImageType.Logo            => $"{appId}_logo.jpg",
            SteamImageType.LibraryHero     => $"{appId}_library_hero.jpg",
            SteamImageType.LibraryHeroBlur => $"{appId}_library_hero_blur.jpg",
            _                              => throw new NotSupportedException($"{type} is not available in the local cache. Use the {nameof(SteamCdnHelper)} instead.")
        };
    }
}
