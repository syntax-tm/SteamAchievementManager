using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using log4net;

namespace SAM.Core
{
    // TODO: Consider moving all HttpClient functionality here and support caching
    public static class WebManager
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(WebManager));
        private static readonly HttpClient _wc = new ();

        // TODO: Add support for async
        public static Image DownloadImage(string imageUrl, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the image from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetImageFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }

                var data = _wc.GetStreamAsync(imageUrl).Result;

                var image = Image.FromStream(data);
                
                // if we were passed a key, cache the image so we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheImage(cacheKey, image);
                }

                return image;
            }
            catch (WebException we)
            {
                switch (we.Response)
                {
                    case HttpWebResponse {StatusCode: HttpStatusCode.NotFound}:
                        log.Error($"Failed to download image '{imageUrl}' ({HttpStatusCode.NotFound}).", we);
                        return null;
                    case HttpWebResponse {StatusCode: HttpStatusCode.TooManyRequests}:
                        log.Error($"Failed to download image '{imageUrl}' ({HttpStatusCode.TooManyRequests}).", we);
                        return null;
                    default:
                        throw;
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{imageUrl}'. {e.Message}";
                throw new (message, e);
            }
        }

    }
}
