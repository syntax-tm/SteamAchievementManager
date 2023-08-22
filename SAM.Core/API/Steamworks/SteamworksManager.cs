using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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

        private static readonly HttpClient _client = new ();

        public static Dictionary<uint, string> GetAppList()
        {
            try
            {
                var cacheKey = CacheKeyFactory.CreateAppListCacheKey();

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

                var cacheKey = CacheKeyFactory.CreateAppCacheKey(id);

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

        private static bool ShouldSkip(uint id)
        {
            return _skipStoreInfoAppIds.Contains(id);
        }
        
        // TODO: Create a better way to skip non-queryable apps in the store API
        // these are all app ids that do not return successfully so we can skip them
        // and reduce calls to the store API
        private static readonly uint[] _skipStoreInfoAppIds =
        {
            41010,  // Serious Sam HD: The Second Encounter
            42160,  // War of the Roses
            91310,  // Dead Island
            92500,  // PC Gamer
            200110, // Nosgoth
            202270, // Leviathan: Warships
            204080, // The Showdown Effect
            218130, // Dungeonland
            223390, // Forge
            225140, // Duke Nukem 3D: Megaton Edition
            254270, // Dungeonland - All access pass
            321040  // DiRT 3 Complete Edition
        };
    }
}
