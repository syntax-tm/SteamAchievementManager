using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAM.Core.Storage;

namespace SAM.Core
{
    public static class SteamworksManager
    {
        private const string GETAPPLIST_URL = @"https://api.steampowered.com/ISteamApps/GetAppList/v2/";
        private const string APPDETAILS_URL = @"https://store.steampowered.com/api/appdetails/?appids={0}";

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamworksManager));

        private static readonly object syncLock = new ();
        // ReSharper disable once InconsistentNaming
        private static readonly HttpClient _client = new ();
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentQueue<SteamApp> _refreshQueue = new ();
        private static BackgroundWorker _refreshWorker;

        public static Dictionary<uint, string> GetAppList()
        {
            try
            {
                var cacheKey = CacheKeys.AppList;

                // if we have the file in the cache, then deserialize the cached json and
                // return that
                if (CacheManager.TryGetObject<Dictionary<uint, string>>(cacheKey, out var cachedApps))
                {
                    return cachedApps;
                }
                
                var apiResponse = _client.GetStringAsync(GETAPPLIST_URL).Result;
                
                if (string.IsNullOrEmpty(apiResponse))
                {
                    throw new SAMInitializationException(@"The Steam API request for GetAppList returned nothing.");
                }

                var jd = JsonDocument.Parse(apiResponse);
                var apps = new Dictionary<uint, string>();

                foreach (var item in jd.RootElement.GetProperty(@"applist").GetProperty(@"apps").EnumerateArray())
                {
                    var appid = item.GetProperty(@"appid").GetUInt32();

                    if (apps.ContainsKey(appid)) continue;
                    
                    var name = item.GetProperty(@"name").GetString();

                    apps.Add(appid, name);
                }

                // cache the app list
                var appListJson = JsonConvert.SerializeObject(apps);
                CacheManager.CacheObject(cacheKey, appListJson);

                return apps;
            }
            catch (SAMInitializationException) { throw; }
            catch (Exception e)
            {
                var message = $"An error occurred loading the app list from the Steam Web API. {e.Message}";

                log.Error(message, e);

                throw;
            }
        }

        public static SteamStoreApp GetAppInfo(uint id, bool loadDlc = false)
        {

            try
            {
                if (ShouldSkip(id))
                {
                    log.Debug($"Skipping {nameof(GetAppInfo)} for app id '{id}'.");
                    return null;
                }

                var cacheKey = CacheKeys.CreateAppCacheKey(id);

                // if we have the file in the cache, then deserialize the cached json and
                // return that
                if (CacheManager.TryGetObject<SteamStoreApp>(cacheKey, out var cachedApp))
                {
                    return cachedApp;
                }

                var storeUrl = string.Format(APPDETAILS_URL, id);
                
                var appInfoText = _client.GetStringAsync(storeUrl).Result;
                var jo = JObject.Parse(appInfoText);

                var successElement = jo.SelectToken($"{id}.success");
                var success = successElement != null && successElement.Value<bool>();
                
                if (!success)
                {
                    log.Warn($@"Steam Web API appdetails for app id '{id}' failed.");

                    return null;
                }
                
                var appData = jo.SelectToken($"{id}.data")?.ToString();

                if (string.IsNullOrWhiteSpace(appData))
                {
                    log.Warn($@"Steam Web API appdetails for app id '{id}' returned no data.");

                    return null;
                }

                var storeApp = JsonConvert.DeserializeObject<SteamStoreApp>(appData);

                // TODO: Add caching for DLC items
                //if (loadDlc && storeApp.Dlc.Any())
                //{
                //    foreach (var dlc in storeApp.Dlc)
                //    {
                //        var dlcApp = GetAppInfo(dlc);
                //        storeApp.DlcInfo.Add(dlcApp);
                //    }
                //}
                
                // cache the app list
                CacheManager.CacheObject(cacheKey, storeApp);

                return storeApp;
            }
            catch (HttpRequestException) { throw; }
            catch (Exception e)
            {
                var message = $"Failed to get app info for app id '{id}'. {e.Message}";

                log.Error(message, e);

                throw;
            }
        }

        public static async Task<SteamStoreApp> GetAppInfoAsync(uint id, bool loadDlc = false)
        {

            try
            {
                if (ShouldSkip(id))
                {
                    log.Debug($"Skipping {nameof(GetAppInfo)} for app id '{id}'.");
                    return null;
                }

                var cacheKey = CacheKeys.CreateAppCacheKey(id);

                // if we have the file in the cache, then deserialize the cached json and
                // return that
                if (CacheManager.TryGetObject<SteamStoreApp>(cacheKey, out var cachedApp))
                {
                    return cachedApp;
                }

                var storeUrl = string.Format(APPDETAILS_URL, id);
                
                var appInfoText = await _client.GetStringAsync(storeUrl);
                var jo = JObject.Parse(appInfoText);

                var successElement = jo.SelectToken($"{id}.success");
                var success = successElement != null && successElement.Value<bool>();
                
                if (!success)
                {
                    log.Warn($@"Steam Web API appdetails for app id '{id}' failed.");

                    return null;
                }
                
                var appData = jo.SelectToken($"{id}.data")?.ToString();

                if (string.IsNullOrWhiteSpace(appData))
                {
                    log.Warn($@"Steam Web API appdetails for app id '{id}' returned no data.");

                    return null;
                }

                var storeApp = JsonConvert.DeserializeObject<SteamStoreApp>(appData);

                // TODO: Add caching for DLC items
                //if (loadDlc && storeApp.Dlc.Any())
                //{
                //    foreach (var dlc in storeApp.Dlc)
                //    {
                //        var dlcApp = GetAppInfo(dlc);
                //        storeApp.DlcInfo.Add(dlcApp);
                //    }
                //}
                
                // cache the app list
                await CacheManager.CacheObjectAsync(cacheKey, storeApp);

                return storeApp;
            }
            catch (HttpRequestException) { throw; }
            catch (Exception e)
            {
                var message = $"Failed to get app info for app id '{id}'. {e.Message}";

                log.Error(message, e);

                throw;
            }
        }

        /// <summary>
        /// Queues a <see cref="SteamApp"/> for loading information from the Steam store.
        /// </summary>
        /// <param name="app"></param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="app" /> is <see langword="null" /></exception>
        public static void LoadStoreInfo(SteamApp app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // if we are going to skip it anyway, don't bother queueing it
            if (ShouldSkip(app.Id))
            {
                return;
            }

            _refreshQueue.Enqueue(app);
            
            // if we've already started the background worker then return
            if (_refreshWorker != null) return;

            lock (syncLock)
            {
                // re-checking after entering lock
                if (_refreshWorker != null) return;

                _refreshWorker = new ();
                _refreshWorker.WorkerSupportsCancellation = true;
                _refreshWorker.DoWork += OnRefreshWorkerDoWork;
                _refreshWorker.RunWorkerCompleted += OnRefreshWorkerCompleted;

                _refreshWorker.RunWorkerAsync();
            }
        }

        private static void OnRefreshWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                log.Warn($"{nameof(SteamworksManager)} refresh {nameof(BackgroundWorker)} stopped due to requested cancellation.");
            }

            log.Info($"The {nameof(SteamworksManager)} refresh {nameof(BackgroundWorker)} stopped.");
        }

        private static void OnRefreshWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                while (true)
                {
                    if (_refreshWorker.CancellationPending)
                    {
                        args.Cancel = true;
                        return;
                    }

                    while (_refreshQueue.TryDequeue(out var app))
                    {
                        // take the next app in the queue and load the store info, if successful this
                        // will automatically load its images
                        var result = LoadAppInfo(app);

                        if (result != SteamworksOperationResult.Success)
                        {
                            // re-queue the app to try again later
                            _refreshQueue.Enqueue(app);

                            continue;
                        }

                        log.Debug($"Completed store information refresh for {app.Name} ({app.Id}).");
                    }

                    // once we're done with everything in the queue, wait 10 seconds before checking it again
                    // in case new items are added
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred while refreshing app information. {e.Message}";
                log.Error(message, e);

                args.Cancel = true;
            }
        }

        private static SteamworksOperationResult LoadAppInfo(SteamApp app)
        {
            const int MAX_RETRIES = 3;
            var retries = 0;
            var rateLimited = false;

            do
            {
                // this function processes one individual app from the queue, including any
                // retries (up to the maximum allotted)
                try
                {
                    var storeInfo = GetAppInfo(app.Id);
                            
                    // this happens when an app is explicitly skipped (_skipStoreInfoAppIds)
                    if (storeInfo == null)
                    {
                        continue;
                    }

                    app.StoreInfo = storeInfo;

                    return SteamworksOperationResult.Success;
                }
                catch (HttpRequestException hre) when (hre.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
                {
                    rateLimited = true;

                    // if we are being blocked (Unauthorized or Forbidden) then wait at least 30 minutes
                    // otherwise if it's an intermittent failure or TooManyRequests then wait 5 minutes
                    var retryTime = hre.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                        ? TimeSpan.FromMinutes(30)
                        : TimeSpan.FromMinutes(5);
                    
                    var retrySb = new StringBuilder();

                    retrySb.Append($"Request for store info on app '{app.Id}' returned {nameof(HttpStatusCode)} {hre.StatusCode:D} ({hre.StatusCode:G}). ");
                    retrySb.Append($"Waiting {retryTime.TotalMinutes:N0} minute(s) and then retrying...");

                    log.Warn(retrySb);

                    Thread.Sleep(retryTime);
                }
                catch (Exception e)
                {
                    log.Error($"An error occurred attempting to load the store info for {app.Name} ({app.Id}). {e.Message}", e);
                    break;
                }
                finally
                {
                    retries++;
                }
            }
            while (retries < MAX_RETRIES);

            return rateLimited
                ? SteamworksOperationResult.RateLimited
                : SteamworksOperationResult.Failed;
        }

        private static bool ShouldSkip(uint id)
        {
            return _skipStoreInfoAppIds.Contains(id);
        }
        
        // ReSharper disable CommentTypo
        // TODO: Create a better way to skip non-queryable apps in the store API
        // these are all app ids that do not return successfully, so we can skip them
        // and reduce calls to the store API
        // ReSharper disable once InconsistentNaming
        private static readonly uint[] _skipStoreInfoAppIds =
        [
            //41010,  // Serious Sam HD: The Second Encounter
            //42160,  // War of the Roses
            //91310,  // Dead Island
            //92500,  // PC Gamer
            //200110, // Nosgoth
            //202270, // Leviathan: Warships
            //204080, // The Showdown Effect
            //218130, // Dungeonland
            //223390, // Forge
            //225140, // Duke Nukem 3D: Megaton Edition
            //254270, // Dungeonland - All access pass
            //321040  // DiRT 3 Complete Edition
        ];
        // ReSharper restore CommentTypo
    }
}
