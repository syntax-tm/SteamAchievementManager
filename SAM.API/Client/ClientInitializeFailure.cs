namespace SAM.API
{
    public enum ClientInitializeFailure : byte
    {
        Unknown = 0,
        GetInstallPath,
        Load,
        CreateSteamClient,
        CreateSteamPipe,
        ConnectToGlobalUser,
        AppIdMismatch,
    }
}