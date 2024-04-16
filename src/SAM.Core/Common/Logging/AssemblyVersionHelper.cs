using System.Reflection;

namespace SAM.Core.Logging;

public class AssemblyVersionHelper
{
    public const string KEY = @"AssemblyVersion";

    public override string ToString()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

        return version.ProductVersion;
    }
}
