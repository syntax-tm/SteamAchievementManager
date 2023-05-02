using SAM.Core.Settings;

namespace SAM.Core.Storage
{
    public class UserSettingsKey : CacheKeyBase
    {
        public UserSettingsKey()
            : base(nameof(UserSettings), "Settings")
        {

        }
    }
}
