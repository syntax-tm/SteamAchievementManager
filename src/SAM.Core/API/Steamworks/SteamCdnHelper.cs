﻿using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using log4net;
using SAM.Core.Storage;

namespace SAM.Core;

public static class SteamCdnHelper
{
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

    private static readonly ILog log = LogManager.GetLogger(nameof(SteamCdnHelper));
    
    public static Uri CacheImage(uint id, SteamImageType type, string file = null, bool localOnly = false)
    {
        try
        {
            var url = GetImageUri(id, type, file);

            var fileName = Path.GetFileName(url.ToString());
            var cacheKey = CacheKeys.CreateAppImageCacheKey(id, fileName);

            if (localOnly)
            {
                var isLocal = CacheManager.TryGetFile(cacheKey, out var uri);

                // since this is local only we're returning the result either way
                return uri;
            }

            _ = WebManager.DownloadImage(url, cacheKey);

            // if the image was just downloaded then it's guaranteed to be in the cache so get the uri to it
            var cachedImage = CacheManager.GetFile(cacheKey);

            return cachedImage;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred downloading the {type} image for app id '{id}'.", e);

            throw;
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
            log.Error($"An error occurred downloading the {type} image for app id '{id}'.", e);

            throw;
        }
    }

    public static Uri GetImageUri(uint id, SteamImageType type, string file = null)
    {
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
}
