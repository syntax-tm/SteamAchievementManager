using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        // TODO: Store this somewhere outside the application so that it can be actively maintained separately
        private static readonly uint[] _ignoredApps =
        {
            13260 // Unreal Development Kit
        };
        private static ConcurrentDictionary<uint, SupportedApp> _gameList;

        private static readonly HttpClient client = new ();

        public static IDictionary<uint, SupportedApp> Apps
        {
            get
            {
                if (_gameList != null) return _gameList;

                _gameList = new (GetSupportedGames());

                return _gameList;
            }
        }

        public static IDictionary<uint, SupportedApp> GetSupportedGames()
        {
            if (_gameList != null) return _gameList;

            try
            {
                _gameList = [ ];

                using var request = new HttpRequestMessage(HttpMethod.Get, SAM_GAME_LIST_URL);
                var response = client.Send(request);
                var responseStream = response.Content.ReadAsStream();

                var document = new XPathDocument(responseStream);
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

                    _gameList[gameId] = new (gameId, type);
                }

                return _gameList;
            }
            catch (Exception e)
            {
                var message = $"An error occurred getting the list of supported apps. {e.Message}";

                log.Error(message, e);

                throw new SAMException(message, e);
            }
        }

        public static bool TryGetApp(uint id, out SupportedApp app)
        {
            return Apps.TryGetValue(id, out app);
        }

        // TODO: Add a way to check and see if an app is supported instead of just trying it
        public static SupportedApp GetApp(uint id)
        {
            if (Apps.TryGetValue(id, out var app)) return app;

            var message = $"App '{id}' is not currently supported.";
            throw new SAMException(message);
        }
    }
}
