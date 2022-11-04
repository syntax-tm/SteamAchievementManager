using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAM.Core
{
    [DefaultValue(All)]
    public enum AchievementFilter
    {
        [Display(Name = "All", Description = "All achievements.")]
        All = 0,
        [Display(Name = "Unlocked", Description = "Unlocked achievements")]
        Unlocked = 1,
        [Display(Name = "Locked", Description = "Locked achievements")]
        Locked = 2,
        [Display(Name = "Modified", Description = "Modified achievements")]
        Modified = 3,
        [Display(Name = "Unmodified", Description = "Unmodified achievements")]
        Unmodified = 4
    }
}
