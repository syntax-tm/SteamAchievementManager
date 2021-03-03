using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.Mvvm;
using Newtonsoft.Json;

namespace SAM.WPF.Core.Settings
{
    public class UserSettings : BindableBase
    {
        [JsonProperty(nameof(ManagerSettings))]
        public ManagerSettings ManagerSettings
        {
            get => GetProperty(() => ManagerSettings);
            set => SetProperty(() => ManagerSettings, value);
        }

        protected UserSettings()
        {
            ManagerSettings = new ManagerSettings();
        }

        public static UserSettings Default => new UserSettings();

        public static UserSettings Load()
        {
            var key = new UserSettingsKey();

            if (CacheManager.TryGetObject<UserSettings>(key, out var settings))
            {
                return settings;
            }

            // no settings were found in cache, create default
            var defaultSettings = new UserSettings();

            CacheManager.CacheObject(key, defaultSettings);

            return defaultSettings;
        }

        public void Save()
        {
            // TODO: save user settings to isolated storage
        }

    }
}
