using System.ComponentModel;

namespace SAM.WPF.Core
{
    [DefaultValue(All)]
    public enum AchievementFilter
    {
        All = 0,
        Unlocked = 1,
        Locked = 2,
        Modified = 3,
        Unmodified = 4
    }
}
