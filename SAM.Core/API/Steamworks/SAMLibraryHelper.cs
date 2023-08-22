﻿using System;
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

        private const string SAM_GAME_LIST_URL = @"http://gib.me/sam/games.xml";

        private static readonly ILog log = LogManager.GetLogger(nameof(SAMLibraryHelper));

        private static List<SupportedApp> _gameList;

        public static List<SupportedApp> GetSupportedGames()
        {
            if (_gameList != null) return _gameList;

            try
            {
                var pairs = new List<SupportedApp>();
                
                using var wc = new HttpClient();
                var bytes = wc.GetByteArrayAsync(new Uri(SAM_GAME_LIST_URL)).Result;

                var ignoredApps = GetIgnoredApps();

                using var stream = new MemoryStream(bytes, false);

                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();

                Debug.Assert(navigator is not null, $"The {nameof(XPathDocument)}'s {nameof(navigator)} cannot be null.");

                var nodes = navigator.Select("/games/game");

                while (nodes.MoveNext())
                {
                    var gameId = (uint) nodes.Current.ValueAsLong;

                    if (ignoredApps.Contains(gameId))
                    {
                        log.Debug($"Ignoring Steam app with ID '{gameId}'.");
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
                var message = $"An error occurred attempting to get app {id}. {e.Message}";

                log.Warn(message, e);
                
                return false;
            }
        }

        public static SupportedApp GetApp(uint id)
        {
            if (TryGetApp(id, out var app)) return app;

            var message = $"App '{id}' is not currently supported.";
            throw new SAMException(message);
        }
        
        public static List<uint> GetIgnoredApps()
        {
            return new()
            {
                13260 // unreal development kit
            };
        }


    }
}
