using System;

namespace SAM.Core.Extensions;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string a, string b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) return false;

        return a.Equals(b, StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool ContainsIgnoreCase(this string a, string b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));

        if (a == string.Empty) return b == string.Empty;

        return a.Contains(b, StringComparison.CurrentCultureIgnoreCase);
    }
}
