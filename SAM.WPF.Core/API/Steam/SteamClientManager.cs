using System;
using log4net;
using SAM.API;

namespace SAM.WPF.Core.API.Steam
{
    public static class SteamClientManager
    {

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamClientManager));

        private static readonly object syncLock = new object();
        private static Client _client;
        
        public static uint AppId { get; private set; }
        public static bool IsInitialized { get; private set; }
        public static string CurrentLanguage { get; private set; }

        public static Client Default
        {
            get
            {
                if (_client != null) return _client;
                lock (syncLock)
                {
                    _client = new Client();
                }
                return _client;
            }
        }

        public static void Init(uint appId)
        {
            if (IsInitialized)
            {
                var message = $"Client is already initialized with app id '{AppId}'.";
                throw new InvalidOperationException(message);
            }

            try
            {
                Default.Initialize(appId);

                AppId = appId;
                CurrentLanguage = Default.SteamApps008.GetCurrentGameLanguage();

                IsInitialized = true;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to initialize the Steam client with app ID '{appId}'. {e.Message}";
                log.Error(message, e);

                throw new SAMInitializationException(message, e);
            }
        }

    }
}
