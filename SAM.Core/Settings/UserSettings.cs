using DevExpress.Mvvm;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SAM.Core.Settings
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserSettings : BindableBase
    {
        private static UserSettings _instance;
        private static readonly object syncLock = new ();

        private static readonly UserSettingsKey _settingsKey = new ();
        
        public ManagerSettings ManagerSettings
        {
            get => GetProperty(() => ManagerSettings);
            set => SetProperty(() => ManagerSettings, value);
        }
        
        public LibrarySettings LibrarySettings
        {
            get => GetProperty(() => LibrarySettings);
            set => SetProperty(() => LibrarySettings, value);
        }

        public static UserSettings Default
        {
            get
            {
                if (_instance != null) return _instance;
                lock (syncLock)
                {
                    _instance = new ();
                }
                return _instance;
            }
        }
        
        protected UserSettings()
        {
            ManagerSettings = new ();

            Load();
        }
        
        private void Load()
        {
            if (CacheManager.TryPopulateObject(_settingsKey, this))
            {
                return;
            }

            CacheManager.CacheObject(_settingsKey, this);
        }

        public void Save()
        {
            CacheManager.CacheObject(_settingsKey, this);
        }
    }
}
