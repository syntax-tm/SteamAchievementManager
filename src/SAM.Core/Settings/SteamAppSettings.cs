using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SAM.Core.Settings
{
    [DebuggerDisplay("{AppId}")]
    public class SteamAppSettings
    {
        public uint AppId { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsHidden { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{AppId}");

            var attributes = new List<string>();

            if (IsFavorite) attributes.Add("Favorite");
            if (IsHidden) attributes.Add("Hidden");

            if (!attributes.Any()) return sb.ToString();

            sb.Append(" (");
            sb.Append(string.Join(", ", attributes));
            sb.Append(")");

            return sb.ToString();
        }
    }
}
