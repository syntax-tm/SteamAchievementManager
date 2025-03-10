using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Core.Storage;

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

public class SteamImageResponse : ISteamImageSource
{
    public uint AppId { get; set; }
    public bool Cached { get; set; }
    public Uri Uri { get; set; }
    public bool IsAnimated { get; set; }
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
    
    public static Image DownloadImage(uint id, SteamImageType type, string file = null, bool localOnly = false)
    {
        try
        {
            var url = GetImageUri(id, type, file);

            var fileName = Path.GetFileName(url.ToString());
            var cacheKey = CacheKeys.CreateAppImageCacheKey(id, fileName);
                
            var img = WebManager.DownloadImage(url, cacheKey, localOnly);
                
            return img;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred downloading the {type} image for app id '{id}'.", e);

            throw;
        }
    }

    public static async Task<Image> DownloadImageAsync(uint id, SteamImageType type, string file = null, bool localOnly = false)
    {
        try
        {
            var url = GetImageUri(id, type, file);

            var fileName = Path.GetFileName(url.ToString());
            var cacheKey = CacheKeys.CreateAppImageCacheKey(id, fileName);

            var img = await WebManager.DownloadImageAsync(url, cacheKey, localOnly);
                
            return img;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred downloading the {type:G} image for app id '{id}'.", e);

            throw;
        }
    }

    public static bool TryGetCachedAppImage(uint appId, SteamImageType type, out SteamImageResponse response)
    {
        try
        {
            var imagePath = GetCachedAppImagePath(appId, type);
            if (imagePath == null)
            {
                response = null;
                return false;
            }

            if (!File.Exists(imagePath))
            {
                response = null;
                return false;
            }
            
            response = new ()
            {
                AppId = appId,
                Cached = true,
                Uri = new (imagePath),
                IsAnimated = IsAnimated(imagePath)
            };

            return true;
        }
        catch
        {
            response = null;
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

        var appCachePath = Path.Join(SteamLibraryCachePath, $"{appId}");

        var exists = Directory.Exists(appCachePath);
        var searchTarget = exists ? appCachePath : SteamLibraryCachePath;
        var searchName = GetAppImageSearchName(appId, type);

        var imageResults = Directory.GetFiles(searchTarget, searchName, SearchOption.AllDirectories);

        return imageResults.FirstOrDefault();
    }

    private static bool IsAnimated(string imagePath)
    {
        // TODO: there's definitely better ways to do this and this should be refactored once a better animated image solution is implemented
        if (string.IsNullOrEmpty(imagePath)) throw new ArgumentNullException(nameof(imagePath));
        if (!File.Exists(imagePath)) throw new FileNotFoundException($"Image file '{imagePath}' does not exist.", imagePath);

        const string GIF_FILE_HEADER = @"GIF";
        const string ANIMATED_WEBP_SECTION = @"ANMF";
        const string ANIMATED_PNG_SECTION = @"acTL";

        var content = File.ReadAllText(imagePath);

        // only check the first 256 characters
        if (content.Length > 256)
        {
            content = content[..256];
        }

        // animated GIF
        if (content.StartsWith(GIF_FILE_HEADER))
        {
            return true;
        }
        // animated WEBP
        if (content.Contains(ANIMATED_WEBP_SECTION))
        {
            return true;
        }
        // animated PNG
        if (content.Contains(ANIMATED_PNG_SECTION))
        {
            return true;
        }

        return false;
    }

    private static Uri GetImageUri(uint id, SteamImageType type, string file = null)
    {
        // NOTE: this method returns Uris for CDN-downloaded images (i.e. achievement icons)
        var url = type switch
        {
            SteamImageType.ClientIcon      => string.Format(GAME_CLIENT_ICON_URI, id, file),
            SteamImageType.Icon            => string.Format(GAME_ICON_URI, id, file),
            SteamImageType.Logo            => string.Format(GAME_LOGO_URI, id, file),
            SteamImageType.Header          => string.Format(GAME_HEADER_URI, id),
            SteamImageType.LibraryHero     => string.Format(GAME_LIBRARY_HERO_URI, id),
            SteamImageType.SmallCapsule    => string.Format(GAME_SMALL_CAPSULE_URI, id),
            SteamImageType.MediumCapsule   => string.Format(GAME_MEDIUM_CAPSULE_URI, id),
            SteamImageType.LargeCapsule    => string.Format(GAME_LARGE_CAPSULE_URI, id),
            SteamImageType.AchievementIcon => string.Format(GAME_ACHIEVEMENT_URI, id, file),
            SteamImageType.GridLandscape or
            SteamImageType.GridPortrait or
            SteamImageType.GridIcon or
            SteamImageType.GridHero or
            SteamImageType.Grid            => throw new NotSupportedException($"{nameof(SteamCdnHelper)} only supports CDN images. {nameof(SteamImageType)} {type:G} is not supported."),
            // ReSharper disable PatternIsRedundant
            SteamImageType.Library or
            SteamImageType.LibraryHeroBlur or
            // ReSharper enable PatternIsRedundant
            _                              => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return new (url);
    }

    private static string GetAppImageName(uint appId, SteamImageType type)
    {
        // NOTE: this method is for local images (i.e. Steam's library cache, Grid, etc.)
        return type switch
        {
            SteamImageType.GridLandscape   => $"{appId}",
            SteamImageType.GridPortrait    => $"{appId}p",
            SteamImageType.GridIcon        => $"{appId}_icon",
            SteamImageType.GridHero        => $"{appId}_hero",
            SteamImageType.Header          => @"header.jpg",
            SteamImageType.Icon            => $"{appId}_icon.jpg",
            SteamImageType.Logo            => $"logo.png",
            SteamImageType.LibraryHero     => @"library_hero.jpg",
            SteamImageType.LibraryHeroBlur => @"library_hero_blur.jpg",
            SteamImageType.Library         => @"library_600x900.jpg",
            _                              => throw new NotSupportedException($"{type} is not available in the local cache. Use {nameof(GetImageUri)} instead.")
        };
    }

    private static string GetAppImageSearchName(uint appId, SteamImageType type)
    {
        // NOTE: this method is for local images (i.e. Steam's library cache, Grid, etc.)
        return type switch
        {
            SteamImageType.GridLandscape   => $"{appId}",
            SteamImageType.GridPortrait    => $"{appId}p",
            SteamImageType.GridIcon        => $"{appId}_icon",
            SteamImageType.GridHero        => $"{appId}_hero",
            SteamImageType.Header          => @"*header.jpg",
            SteamImageType.Icon            => $"{appId}_icon.jpg",
            SteamImageType.Logo            => $"logo.png",
            SteamImageType.LibraryHero     => @"library_hero.jpg",
            SteamImageType.LibraryHeroBlur => @"library_hero_blur.jpg",
            SteamImageType.Library         => @"library_600x900.jpg",
            _                              => throw new NotSupportedException($"{type} is not available in the local cache. Use {nameof(GetImageUri)} instead.")
        };
    }
}
