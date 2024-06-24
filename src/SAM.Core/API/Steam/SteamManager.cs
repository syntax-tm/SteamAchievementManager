using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace SAM.Core;

public static class SteamManager
{
    private const string STEAM_REG_PATH = @"Software\Valve\Steam";
    private const string STEAMPATH_ENTRY_NAME = @"SteamPath";

    private static readonly ILog log = LogManager.GetLogger(nameof(SteamManager));

    private static string _steamPath;
    private static ConcurrentDictionary<uint, SteamAppInfo> _appInfoCache = [ ];

    public static string GetSteamPath()
    {
        if (_steamPath != null) return _steamPath;

        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
        using var key = baseKey.OpenSubKey(STEAM_REG_PATH);

        var value = key?.GetValue(STEAMPATH_ENTRY_NAME);

        if (value == null)
        {
            var message = $"Unable to determine Steam's current {STEAMPATH_ENTRY_NAME}.";
            throw new SAMException(message);
        }

        _steamPath = value.ToString();

        return _steamPath;
    }

    public static SteamAppInfo LoadCachedAppInfo(uint id)
    {
        if (_appInfoCache.TryGetValue(id, out var info))
        {
            return info;
        }

        var converter = new SteamAppInfoConverter();
        var settings = new JsonSerializerSettings()
        {
            Converters = [ converter ]
        };

        var steamInstallPath = GetSteamPath();
        var userName = SteamUserManager.GetActiveUser();

        var path = Path.Join(steamInstallPath, @"userdata", $"{userName}", @"config", @"librarycache", $"{id}.json");

        if (!File.Exists(path))
        {
            log.Warn($"User's app info cache for '{id}' does not exist.");
            return null;
        }

        var json = File.ReadAllText(path);

        var appInfo = JsonConvert.DeserializeObject<SteamAppInfo>(json, settings);
        
        _appInfoCache.TryAdd(id, appInfo);

        return appInfo;
    }

    public static async Task<SteamAppInfo> LoadCachedAppInfoAsync(uint id)
    {
        if (_appInfoCache.TryGetValue(id, out var info))
        {
            return info;
        }

        var converter = new SteamAppInfoConverter();
        var settings = new JsonSerializerSettings()
        {
            Converters = [ converter ]
        };

        var steamInstallPath = GetSteamPath();
        var userName = SteamUserManager.GetActiveUser();

        var path = Path.Join(steamInstallPath, @"userdata", $"{userName}", @"config", @"librarycache", $"{id}.json");

        if (!File.Exists(path))
        {
            log.Warn($"User's app info cache for '{id}' does not exist.");
            return null;
        }

        var json = await File.ReadAllTextAsync(path);

        var appInfo = JsonConvert.DeserializeObject<SteamAppInfo>(json, settings);

        _appInfoCache.TryAdd(id, appInfo);

        return appInfo;
    }
}
