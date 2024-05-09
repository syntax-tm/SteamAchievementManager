namespace SAM.Core;

public struct SAMExitCode
{
    public const int SteamNotRunning     = -7;
    public const int DispatcherException = -6;
    public const int AppDomainException  = -5;
    public const int TaskException       = -4;
    public const int InvalidAppId        = -3;
    public const int NoAppIdArgument     = -2;
    public const int UnhandledException  = -1;
    public const int Normal              = 0;
}
