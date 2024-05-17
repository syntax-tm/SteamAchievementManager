using System;
using System.Reflection;

namespace SAM.Helpers;

public static class ApplicationHelper
{
    private static Version _version;

    public static Version Version
    {
        get
        {
            if (_version != null) return _version;

            _version = Assembly.GetEntryAssembly()?.GetName().Version;

            return _version;
        }
    }
}
