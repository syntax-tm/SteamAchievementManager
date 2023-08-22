using System;
using log4net;
using SAM.API;

namespace SAM.Core
{
    public static class SteamClientManager
    {

        private static readonly ILog log = LogManager.GetLogger(nameof(SteamClientManager));

        private static readonly object syncLock = new ();
        private static Client _client;
        
        public static uint AppId { get; private set; }
        public static string CurrentLanguage { get; private set; }

        public static Client Default => _client;

        public static void Init(uint appId)
        {
            try
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                lock (syncLock)
                {
                    _client = new Client();
                    _client.Initialize(appId);

                    AppId = appId;
                    CurrentLanguage = Default.SteamApps008.GetCurrentGameLanguage();
                }
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
