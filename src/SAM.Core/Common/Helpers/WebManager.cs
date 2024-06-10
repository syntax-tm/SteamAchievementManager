using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using SAM.Core.Storage;

namespace SAM.Core
{
    // TODO: Consider moving all HttpClient functionality here and support caching
    public static class WebManager
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(WebManager));
        private static readonly HttpClient client = new ();

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

                var bytes = AsyncHelper.RunSync(() => client.GetByteArrayAsync(url));
                
                // if we were passed a key, cache the text so that we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheBytes(cacheKey, bytes);
                }

                return bytes;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download '{url}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;

            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

        public static async Task<byte[]> DownloadBytesAsync(string url, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetBytes(cacheKey, out var cachedBytes);
                    if (loadedFromCache) return cachedBytes;
                }

                var bytes = await client.GetByteArrayAsync(url);
                
                // if we were passed a key, cache the text so that we can load it from cache next time
                if (cacheKey != null)
                {
                    await CacheManager.CacheBytesAsync(cacheKey, bytes);
                }

                return bytes;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download '{url}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;

            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new SAMException(message, e);
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

                var data = AsyncHelper.RunSync(() => client.GetStringAsync(url));

                // if we were passed a key, cache the text so that we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheText(cacheKey, data);
                }

                return data;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download '{url}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;

            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

        public static async Task<string> DownloadStringAsync(string url, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetTextFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }

                var data = await client.GetStringAsync(url);

                // if we were passed a key, cache the text so that we can load it from cache next time
                if (cacheKey != null)
                {
                    await CacheManager.CacheTextAsync(cacheKey, data);
                }

                return data;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download '{url}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;

            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{url}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

        // TODO: add support for async
        // TODO: add delayed automatic retry for status 429 (too many requests)
        public static async Task<Image> DownloadImageAsync(string imageUrl, ICacheKey cacheKey = null)
        {
            try
            {
                // if we were passed a key, try and load the image from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetImageFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }

                var data = await client.GetStreamAsync(imageUrl);

                var image = Image.FromStream(data);
                
                // if we were passed a key, cache the image, so we can load it from cache next time
                if (cacheKey != null)
                {
                    await CacheManager.CacheImageAsync(cacheKey, image);
                }

                return image;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download image '{imageUrl}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;

            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{imageUrl}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

    }
}
