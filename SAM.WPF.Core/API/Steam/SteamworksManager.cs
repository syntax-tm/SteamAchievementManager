using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using SAM.WPF.Core.Cache;

namespace SAM.WPF.Core.API.Steam
{
    public static class SteamworksManager
    {

        public static Dictionary<uint, string> GetAppList()
        {
            try
            {
                var cacheKey = CacheKeyFactory.CreateAppListCacheKey();

                // if we have the file in the cache, then deserialize the cached json and
                // return that
                if (CacheManager.TryGetTextFile(cacheKey, out var text))
                {
                    return JsonConvert.DeserializeObject<Dictionary<uint, string>>(text);
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
                CacheManager.CacheText(cacheKey, appListJson);

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
                if (CacheManager.TryGetTextFile(cacheKey, out var text))
                {
                    return JsonConvert.DeserializeObject<SteamStoreApp>(text);
                }

                var storeUrl = $"https://store.steampowered.com/api/appdetails/?appids={id}";

                using var wc = new WebClient();
                var appInfoText = wc.DownloadData(storeUrl);

                //if (string.IsNullOrEmpty(appInfoText)) throw new ArgumentNullException(nameof(appInfoText));

                var jd = JsonDocument.Parse(appInfoText);
                var rootProperty = jd.RootElement.GetProperty(id.ToString());

                var isSuccess = rootProperty.GetProperty("success").GetBoolean();
                if (!isSuccess)
                {
                    return null;
                }

                var dataProperty = rootProperty.GetProperty("data");
                var dataText = dataProperty.GetRawText();

                var storeApp = JsonConvert.DeserializeObject<SteamStoreApp>(dataText);

                if (!loadDlc || !storeApp.Dlc.Any()) return storeApp;

                foreach (var dlc in storeApp.Dlc)
                {
                    var dlcApp = GetAppInfo(dlc);
                    storeApp.DlcInfo.Add(dlcApp);
                }

                // cache the app list
                var appJson = JsonConvert.SerializeObject(storeApp);
                CacheManager.CacheText(cacheKey, appJson);

                return storeApp;
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw e;
            }
        }
        
    }
}
