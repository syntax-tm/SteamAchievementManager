using System;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using SAM.API;
using SAM.Core;

namespace SAM.Managers;

public static class SteamClientManager
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType!);

    private static readonly Lock syncLock = new ();
    private static bool _isInitialized;

    public static uint AppId { get; private set; }
    public static string CurrentLanguage { get; private set; }

    public static string CachePath
    {
        get
        {
            if (field != null) return field;
            lock (syncLock)
            {
                field = Path.Join(SteamInstallPath, @"appcache", @"librarycache");
            }
            return field;
        }
    }

    public static string GridCachePath
    {
        get
        {
            if (field != null) return field;
            lock (syncLock)
            {
                var userId = SteamUserManager.GetActiveUser();

                field = Path.Join(SteamInstallPath, @"userdata", $"{userId}", @"config", @"grid");
            }
            return field;
        }
    }

    public static Client Default { get; private set; }

    public static string SteamInstallPath
    {
        get
        {
            if (field != null) return field;

            field = Steam.GetInstallPath();

            return field;
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
