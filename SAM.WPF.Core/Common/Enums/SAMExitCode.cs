using System.ComponentModel;

namespace SAM.WPF.Core
{
    [DefaultValue(Normal)]
    public enum SAMExitCode : int
    {
        DispatcherException = -6,
        AppDomainException = -5,
        TaskException = -4,
        InvalidAppId = -3,
        NoAppIdArgument = -2,
        UnhandledException = -1,
        Normal = 0
    }
}
