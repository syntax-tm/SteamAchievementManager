using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.XPath;

namespace SAM.WPF.Core.API.Steam
{
    public static class SAMLibraryHelper
    {

        private const string SAM_GAME_LIST_URL = @"http://gib.me/sam/games.xml";

        private static List<SupportedApp> _gameList;

        public static List<SupportedApp> GetSupportedGames()
        {
            if (_gameList != null) return _gameList;

            var pairs = new List<SupportedApp>();
            byte[] bytes;

            using (var wc = new WebClient())
            {
                bytes = wc.DownloadData(new Uri(SAM_GAME_LIST_URL));
            }
            
            var ignoredApps = GetIgnoredApps();

            using (var stream = new MemoryStream(bytes, false))
            {
                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext())
                {
                    var gameId = (uint) nodes.Current.ValueAsLong;

                    if (ignoredApps.Contains(gameId)) continue;
                    
                    var type = nodes.Current.GetAttribute("type", string.Empty);
                    if (string.IsNullOrEmpty(type))
                    {
                        type = "normal";
                    }
                    pairs.Add(new SupportedApp((uint)nodes.Current.ValueAsLong, type));
                }
            }

            foreach (var ignoredApp in ignoredApps)
            {
                pairs.RemoveAll(p => p.Id == ignoredApp);
            }

            _gameList = pairs;

            return pairs;
        }

        public static bool TryGetApp(uint id, out SupportedApp app)
        {
            var apps = GetSupportedGames();

            app = apps.FirstOrDefault(a => a.Id == id);

            return app != null;
        }

        public static SupportedApp GetApp(uint id)
        {
            var apps = GetSupportedGames();
            var app = apps.FirstOrDefault(a => a.Id == id);

            if (app == null) throw new ArgumentException(nameof(id));

            return app;
        }
        
        public static List<uint> GetIgnoredApps()
        {
            return new List<uint>
            {
                13260 // unreal development kit
            };
        }


    }
}
