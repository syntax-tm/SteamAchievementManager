using System;
using JetBrains.Annotations;

namespace SAM.WPF.Core.Extensions
{
    public static class StringExtensions
    {

        public static bool EqualsIgnoreCase([NotNull] this string a, string b)
        {
            if (string.IsNullOrEmpty(b)) return false;

            return a.Equals(b, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ContainsIgnoreCase([NotNull] this string a, string b)
        {
            if (string.IsNullOrEmpty(b))
            {
                throw new ArgumentNullException(nameof(b));
            }

            return a.Contains(b, StringComparison.CurrentCultureIgnoreCase);
        }

    }
}
