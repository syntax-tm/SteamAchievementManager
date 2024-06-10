using System.Reflection;

namespace SAM.Core.Logging;

public class EntryAssemblyHelper
{
    public const string KEY = @"EntryAssembly";

    public override string ToString()
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var appId = string.Empty;
        return $"{assemblyName}{appId}";
    }
}
