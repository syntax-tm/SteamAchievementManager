using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAM.WPF.Core.Cache;

namespace SAM.WPF.Core.API.Steam
{
    public static class SteamworksManager
    {

        private const string GETAPPLIST_URL = @"https://api.steampowered.com/ISteamApps/GetAppList/v2/";
        private const string APPDETAILS_URL = @"https://store.steampowered.com/api/appdetails/?appids={0}";

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamworksManager));

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

                using var wc = new WebClient();
                var apiResponse = wc.DownloadString(GETAPPLIST_URL);
                
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
                var cacheKey = CacheKeyFactory.CreateAppCacheKey(id);

                // if we have the file in the cache, then deserialize the cached json and
                // return that
                if (CacheManager.TryGetObject<SteamStoreApp>(cacheKey, out var cachedApp))
                {
                    return cachedApp;
                }

                var storeUrl = string.Format(APPDETAILS_URL, id);

                using var wc = new WebClient();

                var appInfoText = wc.DownloadData(storeUrl);
                var convertedString = System.Text.Encoding.Default.GetString(appInfoText);
                var jo = JObject.Parse(convertedString);

                var appElementName = id.ToString();
                var success = jo[appElementName]["success"].Value<bool>();

                if (!success)
                {
                    log.Warn($@"The Steam Web API appdetails call for app id '{id}' was not successful.");

                    return null;
                }

                var appInfo = jo[id.ToString()]["data"];
                var storeApp = JsonConvert.DeserializeObject<SteamStoreApp>(appInfo.ToString());

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
            catch (WebException) { throw; }
            catch (Exception e)
            {
                var message = $"An error occurred getting the app info for app id '{id}'. {e.Message}";

                log.Error(message, e);

                throw;
            }
        }
    }
}
