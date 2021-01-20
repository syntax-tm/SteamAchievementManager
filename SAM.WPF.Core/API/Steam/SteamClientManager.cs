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

        public static bool IsInitialized { get; private set; }
        public static uint AppId { get; private set; }

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
                throw new InvalidOperationException($"Client is already initialized with app id '{AppId}'.");
            }

            try
            {
                Default.Initialize(appId);

                AppId = appId;

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
