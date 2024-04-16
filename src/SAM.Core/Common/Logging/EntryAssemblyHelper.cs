using System.Reflection;

namespace SAM.Core.Logging;

public class EntryAssemblyHelper
{
    public const string KEY = @"EntryAssembly";

    public override string ToString()
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var appId = SteamClientManager.AppId == 0 ? string.Empty : $" ({SteamClientManager.AppId})";
        return $"{assemblyName}{appId}";
    }
}
