using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using log4net;
using SAM.Core.Storage;

namespace SAM.Core
{
    // TODO: Consider moving all HttpClient functionality here and support caching
    public static class WebManager
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(WebManager));
        private static readonly HttpClient _wc = new ();

        public static byte[] DownloadBytes(string url, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetBytes(cacheKey, out var cachedBytes);
                    if (loadedFromCache) return cachedBytes;
                }

                var bytes = _wc.GetByteArrayAsync(url).Result;
                
                // if we were passed a key, cache the text so we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheBytes(cacheKey, bytes);
                }

                return bytes;
            }
            catch (WebException we)
            {
                switch (we.Response)
                {
                    case HttpWebResponse {StatusCode: HttpStatusCode.NotFound}:
                        log.Error($"Failed to download '{url}' ({HttpStatusCode.NotFound}).", we);
                        return null;
                    case HttpWebResponse {StatusCode: HttpStatusCode.TooManyRequests}:
                        log.Error($"Failed to download '{url}' ({HttpStatusCode.TooManyRequests}).", we);
                        return null;
                    default:
                        throw;
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new (message, e);
            }
        }

        public static string DownloadString(string url, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetTextFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }

                var data = _wc.GetStringAsync(url).Result;
                
                // if we were passed a key, cache the text so we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheText(cacheKey, data);
                }

                return data;
            }
            catch (WebException we)
            {
                switch (we.Response)
                {
                    case HttpWebResponse {StatusCode: HttpStatusCode.NotFound}:
                        log.Error($"Failed to download '{url}' ({HttpStatusCode.NotFound}).", we);
                        return null;
                    case HttpWebResponse {StatusCode: HttpStatusCode.TooManyRequests}:
                        log.Error($"Failed to download '{url}' ({HttpStatusCode.TooManyRequests}).", we);
                        return null;
                    default:
                        throw;
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new (message, e);
            }
        }

        // TODO: add support for async
        // TODO: add delayed automatic retry for status 429 (too many requests)
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
