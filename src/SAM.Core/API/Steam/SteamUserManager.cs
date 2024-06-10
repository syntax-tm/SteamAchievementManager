using Microsoft.Win32;

namespace SAM.Core;

public static class SteamUserManager
{
    private const string STEAM_ACTIVE_PROCESS_REG_PATH = @"Software\Valve\Steam\ActiveProcess";
    private const string STEAM_ACTIVE_USER_ENTRY_NAME = @"ActiveUser";

    private const string STEAM_REG_PATH = @"Software\Valve\Steam";
    private const string STEAM_USERNAME_ENTRY_NAME = @"LastGameNameUsed";

    private static int? _activeUser;
    private static string _activeUserName;

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
