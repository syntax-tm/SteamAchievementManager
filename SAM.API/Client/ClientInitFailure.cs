using System.ComponentModel;

namespace SAM.API
{
    public enum ClientInitFailure : byte
    {
        Unknown = 0,
        GetInstallPath,
        Load,
        [Description("Failed to create ISteamClient018.")]
        CreateSteamClient,
        CreateSteamPipe,
        ConnectToGlobalUser,
        AppIdMismatch
    }
}
