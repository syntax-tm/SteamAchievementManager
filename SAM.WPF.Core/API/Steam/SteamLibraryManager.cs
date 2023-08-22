using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using log4net;

namespace SAM.WPF.Core.API.Steam
{
    public class SteamLibraryManager
    {

        
        private static readonly ILog log = LogManager.GetLogger(nameof(SteamClientManager));

        private static readonly object syncLock = new object();
        private static SteamLibraryManager _instance;

        public static bool IsInitialized { get; private set; }

        public SteamLibrary Library { get; set; }

        public static SteamLibraryManager Default
        {
            get
            {
                if (_instance != null) return _instance;
                lock (syncLock)
                {
                    _instance = new SteamLibraryManager();
                }
                return _instance;
            }
        }
        
        public static void Init()
        {
            try
            {
                if (IsInitialized)
                {
                    throw new InvalidOperationException("Steam library is already initialized.");
                }

                var library = new SteamLibrary();
                library.Refresh();

                Default.Library = library;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to initialize the Steam library. {e.Message}";
                log.Error(message, e);

                throw new ConfigurationErrorsException(message, e);
            }
        }

    }
}
