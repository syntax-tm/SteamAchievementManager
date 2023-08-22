using System.ComponentModel;

namespace SAM.API
{
    public enum ClientInitFailure : byte
    {
        Unknown = 0,
        [Description("Failed to locate the Steam client install path")]
        GetInstallPath,
        Load,
        [Description($"Failed to create {nameof(ISteamClient018)}")]
        CreateSteamClient,
        CreateSteamPipe,
        ConnectToGlobalUser,
        AppIdMismatch
    }
}
