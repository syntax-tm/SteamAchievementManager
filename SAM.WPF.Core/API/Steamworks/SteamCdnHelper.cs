using System;
using System.Drawing;
using System.IO;
using System.Net;
using log4net;
using SAM.WPF.Core.Cache;

namespace SAM.WPF.Core.API
{
    public static class SteamCdnHelper
    {

        private const string GAME_CLIENT_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.ico";
        private const string GAME_ICON_URI = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
        private const string GAME_LOGO_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/logo.png";
        private const string GAME_HEADER_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/header.jpg";
        private const string GAME_LIBRARY_HERO_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/library_hero.jpg";
        private const string GAME_SMALL_CAPSULE_URI = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/capsule_231x87.jpg";
        private const string GAME_ACHIEVEMENT_URI = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamCdnHelper));

        public static Image DownloadImage(uint id, SteamImageType type, string file = null)
        {
            try
            {
                string url;

                switch (type)
                {
                    case SteamImageType.ClientIcon:
                        url = string.Format(GAME_CLIENT_ICON_URI, id, file);
                        break;
                    case SteamImageType.Icon:
                        url = string.Format(GAME_ICON_URI, id, file);
                        break;
                    case SteamImageType.Logo:
                        url = string.Format(GAME_LOGO_URI, id);
                        break;
                    case SteamImageType.Header:
                        url = string.Format(GAME_HEADER_URI, id);
                        break;
                    case SteamImageType.LibraryHero:
                        url = string.Format(GAME_LIBRARY_HERO_URI, id);
                        break;
                    case SteamImageType.SmallCapsule:
                        url = string.Format(GAME_SMALL_CAPSULE_URI, id);
                        break;
                    case SteamImageType.AchievementIcon:
                        url = string.Format(GAME_ACHIEVEMENT_URI, id, file);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                var fileName = Path.GetFileName(url);
                var cacheKey = CacheKeyFactory.CreateAppImageCacheKey(id, fileName);

                if (CacheManager.TryGetImageFile(cacheKey, out var cachedImg))
                {
                    return cachedImg;
                }

                var img = DownloadImage(url);

                if (img != null)
                {
                    CacheManager.CacheImage(cacheKey, img);
                }

                return img;
            }
            catch (Exception e)
            {
                log.Error($"An error occurred attempting to download the {type} image for app {id}.", e);

                throw;
            }
        }

        private static Image DownloadImage(string imageUrl)
        {
            try
            {
                using var wc = new WebClient();
                var data = wc.OpenRead(imageUrl);
                //using var stream = new MemoryStream(data, false);

                return Image.FromStream(data);
            }
            catch (WebException we)
            {
                if (we.Response is HttpWebResponse {StatusCode: HttpStatusCode.NotFound})
                {
                    return null;
                }
                if (we.Response is HttpWebResponse {StatusCode: HttpStatusCode.TooManyRequests})
                {
                    return null;
                }

                throw;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{imageUrl}'. {e.Message}";
                throw new Exception(message, e);
            }
        }

    }
}
