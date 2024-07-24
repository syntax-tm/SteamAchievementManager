using System.Linq;
using log4net;
using Microsoft.Win32;

namespace SAM.Core;

public static class SteamUserManager
{
    private const string STEAM_ACTIVE_PROCESS_REG_PATH = @"Software\Valve\Steam\ActiveProcess";
    private const string STEAM_ACTIVE_USER_ENTRY_NAME = @"ActiveUser";

    private const string STEAM_REG_PATH = @"Software\Valve\Steam";
    private const string STEAM_USERNAME_ENTRY_NAME = @"LastGameNameUsed";

    private const string STEAM_USERS_REG_PATH = @"SOFTWARE\Valve\Steam\Users";

    private static int? _activeUser;
    private static string _activeUserName;

    private static readonly ILog log = LogManager.GetLogger(typeof(SteamUserManager));

    public static int GetActiveUser()
    {
        if (_activeUser.HasValue) return _activeUser.Value;
        
        // TODO: consider switching this to HKCU:\SOFTWARE\Valve\Steam\ActiveProcess\SteamClientDll
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
        using var key = baseKey.OpenSubKey(STEAM_ACTIVE_PROCESS_REG_PATH);

        var value = key?.GetValue(STEAM_ACTIVE_USER_ENTRY_NAME);
        
        if (value == null)
        {
            var message = $"Unable to determine Steam's current {STEAM_ACTIVE_USER_ENTRY_NAME}.";
            throw new SAMException(message);
        }

        // this means that Steam is offline
        if ((int) value == 0)
        {
            // check the number of accounts on this device, if there's only one, then use that steam ID
            using var usersKey = baseKey.OpenSubKey(STEAM_USERS_REG_PATH);

            if (usersKey is { SubKeyCount: 1 })
            {
                value = int.Parse(usersKey.GetSubKeyNames().First());

                log.Warn($"Steam appears to be offline and no active user could be obtained. Using the only previous local user's Steam ID '{value}'.");
            }
        }

        _activeUser = (int) value;

        return _activeUser.Value;
    }

    public static string GetActiveUserName()
    {
        if (_activeUserName != null) return _activeUserName;
        
        // TODO: consider switching this to HKCU:\SOFTWARE\Valve\Steam\ActiveProcess\SteamClientDll
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
        using var key = baseKey.OpenSubKey(STEAM_REG_PATH);

        var value = key?.GetValue(STEAM_USERNAME_ENTRY_NAME);
        
        if (value == null)
        {
            var message = $"Unable to determine Steam's {STEAM_USERNAME_ENTRY_NAME}.";
            throw new SAMException(message);
        }

        _activeUserName = value.ToString();

        return _activeUserName;
    }
}
