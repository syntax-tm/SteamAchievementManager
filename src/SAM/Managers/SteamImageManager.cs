using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using log4net;
using SAM.API;
using SAM.Core;

namespace SAM.Managers;

[Flags]
public enum SteamImageSource
{
    Grid = 1,
    SteamCache = 2,
    SAMCache = 4,
    SteamCDN = 8,
    SteamStore = 16,
    Local = Grid | SteamCache | SAMCache,
    Web = SteamCDN | SteamStore
}

public enum SteamImage
{
    Header,
    Portrait,
    Logo,
    Icon,
    Hero
}

public class SteamImageRequest
{
    public int AppId { get; set; }
    public bool LocalOnly { get; set; }
    public SteamImageSource Source { get; set; }
    public SteamImageType Type { get; set; }
}

public class SteamImageResponse
{
    public int AppId { get; set; }
    public bool Cached { get; set; }
    public SteamImageRequest Request { get; set; }
}

public static class SteamImageManager
{
    private record SteamAppImageCacheKey(uint Id, SteamImageType Type);

    // TODO: Move this to a separate factory or builder class (or rename this)
    private const string GAME_CLIENT_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.ico";
    private const string GAME_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
    private const string GAME_LOGO_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
    private const string GAME_HEADER_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/header.jpg";
    private const string GAME_LIBRARY_HERO_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/library_hero.jpg";
    private const string GAME_SMALL_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_231x87.jpg";
    private const string GAME_MEDIUM_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_467x181.jpg";
    private const string GAME_LARGE_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_616x353.jpg";
    private const string GAME_ACHIEVEMENT_URI = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";

    private static readonly ILog log = LogManager.GetLogger(typeof(SteamImageManager));
    
    private static readonly object syncLock = new ();
    private static readonly List<uint> gridCacheAppWarnings = [];
    private static readonly Dictionary<SteamAppImageCacheKey, string> cachedAppImagePaths = [];
    private static bool _isInitialized;
    private static string _steamInstallPath;
    private static string _steamLibraryCachePath;
    private static string _gridCachePath;

    public static string SteamLibraryCachePath
    {
        get
        {
            if (_steamLibraryCachePath != null) return _steamLibraryCachePath;
            lock (syncLock)
            {
                _steamLibraryCachePath = Path.Join(SteamInstallPath, @"appcache", @"librarycache");
            }
            return _steamLibraryCachePath;
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
    
    public static string SteamInstallPath
    {
        get
        {
            if (_steamInstallPath != null) return _steamInstallPath;

            _steamInstallPath = Steam.GetInstallPath();

            return _steamInstallPath;
        }
    }

    public static Uri GetImage(SteamImageRequest request)
    {
        throw new NotImplementedException();
    }

    public static Uri GetImage(SteamImageType type, bool localOnly = false)
    {
        throw new NotImplementedException();
    }

    private static Uri GetImage(SteamImageType type, SteamImageSource source, bool localOnly = false)
    {
        throw new NotImplementedException();
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

        var imagePath = Path.Join(SteamLibraryCachePath, fileName);

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
