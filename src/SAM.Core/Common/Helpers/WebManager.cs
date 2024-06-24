using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
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

        public static byte[] DownloadBytes(string url, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetBytes(cacheKey, out var cachedBytes);
                    if (loadedFromCache) return cachedBytes;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = client.Send(request);

                var responseStream = response.Content.ReadAsStream();

                using var ms = new MemoryStream();

                responseStream.CopyTo(ms);

                var bytes = ms.ToArray();

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

        public static async Task<byte[]> DownloadBytesAsync(string url, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetBytes(cacheKey, out var cachedBytes);
                    if (loadedFromCache) return cachedBytes;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;

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

        public static string DownloadString(string url, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetTextFile(cacheKey, out var cachedText);
                    if (loadedFromCache) return cachedText;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = client.Send(request);

                using var responseStream = response.Content.ReadAsStream();

                using var reader = new StreamReader(responseStream);

                var data = reader.ReadToEnd();

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

        public static async Task<string> DownloadStringAsync(string url, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the text from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetTextFile(cacheKey, out var cachedText);
                    if (loadedFromCache) return cachedText;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;

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

        public static Image DownloadImage(Uri imageUri, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the image from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetImageFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;
                
                using var request = new HttpRequestMessage(HttpMethod.Get, imageUri);
                using var response = client.Send(request);

                var responseStream = response.Content.ReadAsStream();

                var image = Image.FromStream(responseStream);
                
                // if we were passed a key, cache the image, so we can load it from cache next time
                if (cacheKey != null)
                {
                    CacheManager.CacheImage(cacheKey, image);
                }

                return image;
            }
            catch (HttpRequestException hre)
            {
                var message = $"Failed to download image '{imageUri}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{imageUri}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

        public static async Task<Image> DownloadImageAsync(string imageUrl, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the image from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetImageFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }
                
                // if we didn't find it in the cache return null
                if (localOnly) return null;

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

        public static async Task<Image> DownloadImageAsync(Uri imageUri, ICacheKey cacheKey = null, bool localOnly = false)
        {
            try
            {
                // if we were passed a key, try and load the image from cache
                if (cacheKey != null)
                {
                    var loadedFromCache = CacheManager.TryGetImageFile(cacheKey, out var cachedImage);
                    if (loadedFromCache) return cachedImage;
                }

                // if we didn't find it in the cache return null
                if (localOnly) return null;

                var data = await client.GetStreamAsync(imageUri);

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
                var message = $"Failed to download image '{imageUri}' ({hre.StatusCode:G}).";

                if (hre.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized))
                {
                    throw new SAMException(message, hre);
                }

                log.Warn(message, hre);

                return null;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to download '{imageUri}'. {e.Message}";
                throw new SAMException(message, e);
            }
        }

    }
}
