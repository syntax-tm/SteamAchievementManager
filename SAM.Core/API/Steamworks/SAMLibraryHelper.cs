using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.XPath;
using log4net;

namespace SAM.Core
{
    public static class SAMLibraryHelper
    {
        // TODO: Make this a configurable setting
        private const string SAM_GAME_LIST_URL = @"http://gib.me/sam/games.xml";

        private static readonly ILog log = LogManager.GetLogger(nameof(SAMLibraryHelper));
        
        // TODO: Store this somewhere outside of the application so that it can be actively maintained separately
        private static readonly uint[] _ignoredApps =
        {
            13260 // Unreal Development Kit
        };
        // TODO: This will probably be better off as a ConcurrentBag (for thread safety during Library init) or other keyed collection
        private static List<SupportedApp> _gameList;

        public static List<SupportedApp> GetSupportedGames()
        {
            if (_gameList != null) return _gameList;

            try
            {
                var pairs = new List<SupportedApp>();
                
                using var wc = new HttpClient();
                var bytes = wc.GetByteArrayAsync(new Uri(SAM_GAME_LIST_URL)).Result;
                
                using var stream = new MemoryStream(bytes, false);

                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();

                Debug.Assert(navigator is not null, $"The {nameof(XPathNavigator)} cannot be null.");

                var nodes = navigator.Select("/games/game");

                while (nodes.MoveNext())
                {
                    var gameId = (uint) nodes.Current!.ValueAsLong;

                    if (_ignoredApps.Contains(gameId))
                    {
                        log.Debug($"Skipping app id '{gameId}'.");
                        continue;
                    }

                    var type = nodes.Current.GetAttribute("type", string.Empty);
                    if (string.IsNullOrEmpty(type))
                    {
                        type = "normal";
                    }

                    pairs.Add(new ((uint) nodes.Current.ValueAsLong, type));
                }
                
                _gameList = pairs;

                return pairs;
            }
            catch (Exception e)
            {
                log.Error($"An error occurred getting the list of supported apps. {e.Message}", e);

                throw;
            }
        }

        public static bool TryGetApp(uint id, out SupportedApp app)
        {
            app = null;

            try
            {
                var apps = GetSupportedGames();

                app = apps.FirstOrDefault(a => a.Id == id);

                return app != null;
            }
            catch (Exception e)
            {
                var message = $"An error occurred getting the app id '{id}'. {e.Message}";

                log.Warn(message, e);
                
                return false;
            }
        }

        // TODO: Add a way to check and see if an app is supported instead of just trying it
        public static SupportedApp GetApp(uint id)
        {
            if (TryGetApp(id, out var app)) return app;

            var message = $"App '{id}' is not currently supported.";
            throw new SAMException(message);
        }
    }
}
