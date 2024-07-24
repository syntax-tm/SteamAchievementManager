using System;
using System.IO;
using System.Reflection;
using log4net;
using SAM.API;
using SAM.Core;

namespace SAM.Managers;

public static class SteamClientManager
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

    private static readonly object syncLock = new ();
    private static bool _isInitialized;
    private static string _steamInstallPath;
    private static string _cachePath;
    private static string _gridCachePath;

    public static uint AppId { get; private set; }
    public static string CurrentLanguage { get; private set; }

    public static string CachePath
    {
        get
        {
            if (_cachePath != null) return _cachePath;
            lock (syncLock)
            {
                _cachePath = Path.Join(SteamInstallPath, @"appcache", @"librarycache");
            }
            return _cachePath;
        }
    }

    public static string GridCachePath
    {
        get
        {
            if (_gridCachePath != null) return _gridCachePath;
            lock (syncLock)
            {
                var userId = SteamUserManager.GetActiveUser();

                _gridCachePath = Path.Join(SteamInstallPath, @"userdata", $"{userId}", @"config", @"grid");
            }
            return _gridCachePath;
        }
    }

    public static Client Default { get; private set; }

    public static string SteamInstallPath
    {
        get
        {
            if (_steamInstallPath != null) return _steamInstallPath;

            _steamInstallPath = Steam.GetInstallPath();

            return _steamInstallPath;
        }
    }

    public static void Init(uint appId)
    {
        if (_isInitialized) throw new SAMInitializationException($"The Steam {nameof(Client)} has already been initialized.");

        try
        {
            Default = new ();
            Default.Initialize(appId);

            AppId = appId;
            CurrentLanguage = Default.SteamApps008.GetCurrentGameLanguage();

            _isInitialized = true;
        }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to initialize the Steam client with app ID '{appId}'. {e.Message}";
            log.Error(message, e);

            throw new SAMInitializationException(message, e);
        }
    }
}
