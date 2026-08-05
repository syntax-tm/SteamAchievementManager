using System;
using System.Linq;
using System.Reflection;

namespace SAM.Extensions;

public static class AssemblyExtensions
{
    public static Type[] GetImplementingTypes<T>(this Assembly assembly)
    {
        var searchType = typeof(T);

        if (!searchType.IsInterface)
        {
            var message = $"{nameof(Type)} {searchType.Name} is not an interface.";
            throw new ArgumentException(message, nameof(T));
        }

        var types = assembly.GetTypes();
        return types.Where(type => isImplemented(type, searchType.Name)).ToArray();

        static bool isImplemented(Type t, string name)
        {
            if (t.IsAbstract) return false;
            if (!t.IsClass) return false;

            return t.GetInterface(name) != null;
        }
    }
}