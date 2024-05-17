using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Xml;
using log4net;
using SAM.Core.Storage;

namespace SAM.Core;

public static class SAMLibraryHelper
{
    // TODO: Make this a configurable setting
    private const string SAM_GAME_LIST_URL = @"http://gib.me/sam/games.xml";

    private static readonly ILog log = LogManager.GetLogger(nameof(SAMLibraryHelper));

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

    public static bool TryGetApp(uint id, out SupportedApp app)
    {
        return Apps.TryGetValue(id, out app);
    }

    // TODO: give this a different name so that it's distinct from TryGetApp
    public static SupportedApp GetApp(uint id)
    {
        // this method is used when SAM is managing an app so that it loads only the
        // requested app. in every other situation, all Apps are loaded into the list
        var apps = GetSupportedGames(id);

        if (apps.TryGetValue(id, out var app)) return app;

        var message = $"App '{id}' is not currently supported.";
        throw new SAMException(message);
    }

    private static IDictionary<uint, SupportedApp> GetSupportedGames(uint? appId = null)
    {
        if (_gameList != null) return _gameList;

        try
        {
            _gameList = [ ];

            var cacheKey = CacheKeys.Games;

            if (!CacheManager.TryGetTextFile(cacheKey, out var gamesXml))
            {
                var response = AsyncHelper.RunSync(() => client.GetAsync(SAM_GAME_LIST_URL));
                gamesXml = AsyncHelper.RunSync(() => response.Content.ReadAsStringAsync());

                CacheManager.CacheText(cacheKey, gamesXml);
            }

            var doc = new XmlDocument();
            doc.LoadXml(gamesXml);

            var query = appId == null
                ? "/games/game"
                : $"/games/game[text()=\"{appId}\"]";

            var nodes = doc.SelectNodes(query);

            foreach (XmlNode node in nodes)
            {
                Debug.Assert(node != null, $"{nameof(node)} is null");

                var id = node.FirstChild?.Value ?? throw new SAMException($"Invalid configuration data. Missing {nameof(XmlNode)} {nameof(XmlNode.Value)} for app.");
                var gameId = uint.Parse(id);
                    
                var type = node.Attributes?["type"]?.Value;
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
}
