using SAM.Core.Settings;

namespace SAM.Core
{
    public class UserSettingsKey : CacheKeyBase
    {
        public UserSettingsKey()
            : base(nameof(UserSettings), "Settings")
        {

        }
    }
}
