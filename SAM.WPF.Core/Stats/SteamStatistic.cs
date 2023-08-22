using System.Diagnostics;
using SAM.Game.Stats;

namespace SAM.WPF.Core.Stats
{
    [DebuggerDisplay("{DisplayName} ({Id})")]
    public class SteamStatistic
    {
        public string Id => StatInfo?.Id;
        public string DisplayName => StatInfo?.DisplayName;
        public object Value => StatInfo?.Value;
        public bool IsIncrementOnly => StatInfo?.IsIncrementOnly ?? default;
        public bool IsModified => StatInfo?.IsModified ?? default;
        public int Permission => StatInfo?.Permission ?? default;
        public StatInfo StatInfo { get; set; }

        public SteamStatistic()
        {

        }

        public SteamStatistic(StatInfo stat)
        {
            StatInfo = stat;
        }

    }
}
