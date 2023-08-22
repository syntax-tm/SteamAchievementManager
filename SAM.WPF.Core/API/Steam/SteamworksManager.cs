using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAM.WPF.Core.Cache;

namespace SAM.WPF.Core.API.Steam
{
    public static class SteamworksManager
    {

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

                const string url = @"https://api.steampowered.com/ISteamApps/GetAppList/v2/";

                using var wc = new WebClient();
                var apiResponse = wc.DownloadString(url);
                
                if (string.IsNullOrEmpty(apiResponse)) throw new ArgumentNullException(nameof(apiResponse));

                var jd = JsonDocument.Parse(apiResponse);
                var apps = new Dictionary<uint, string>();

                foreach (var item in jd.RootElement.GetProperty("applist").GetProperty("apps").EnumerateArray())
                {
                    var appid = item.GetProperty("appid").GetUInt32();

                    if (apps.ContainsKey(appid)) continue;
                    
                    var name = item.GetProperty("name").GetString();

                    apps.Add(appid, name);
                }

                // cache the app list
                var appListJson = JsonConvert.SerializeObject(apps);
                CacheManager.CacheObject(cacheKey, appListJson);

                return apps;
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception e)
            {
                Console.WriteLine(e);

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
                //if (CacheManager.TryGetObject<SteamStoreApp>(cacheKey, out var cachedApp))
                //{
                //    return cachedApp;
                //}

                var storeUrl = $"https://store.steampowered.com/api/appdetails/?appids={id}";

                using var wc = new WebClient();
                var appInfoText = wc.DownloadData(storeUrl);

                //if (string.IsNullOrEmpty(appInfoText)) throw new ArgumentNullException(nameof(appInfoText));

                var convertedString = System.Text.Encoding.Default.GetString(appInfoText);
                var jo = JObject.Parse(convertedString);
                var appInfo = jo[id.ToString()]["data"];

                //var jd = JsonDocument.Parse(unescapedText);
                //var rootProperty = jd.RootElement.GetProperty(id.ToString());

                //var isSuccess = rootProperty.GetProperty("success").GetBoolean();
                //if (!isSuccess)
                //{
                //    return null;
                //}
                
                //var dataProperty = rootProperty.GetProperty("data");
                //var dataText = dataProperty.GetRawText();

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
            catch (ArgumentNullException) { throw; }
            catch (Exception e)
            {
                log.Error(e);

                throw;
            }
        }
        
    }
}
