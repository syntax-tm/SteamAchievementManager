using System.ComponentModel;

namespace SAM.API
{
    public enum ClientInitFailure : byte
    {
        Unknown = 0,
        [Description("Failed to locate the Steam client install path")]
        GetInstallPath,
        [Description("Failed to load steam client")]
        Load,
        [Description("Failed to create steam client")]
        CreateSteamClient,
        [Description("Failed to create pipe")]
        CreateSteamPipe,
        [Description("Failed to connect to global user")]
        ConnectToGlobalUser,
        [Description("AppID mismatch")]
        AppIdMismatch
    }
}
