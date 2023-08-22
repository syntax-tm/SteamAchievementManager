using SAM.WPF.Core.Settings;

namespace SAM.WPF.Core
{
    public class UserSettingsKey : CacheKeyBase
    {
        public UserSettingsKey()
            : base(nameof(UserSettings), "Settings")
        {

        }
    }
}
