using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Xml;
using log4net;
using SAM.Core;
using SAM.Core.Interfaces;
using SAM.Core.Storage;

namespace SAM.Managers;

public class SteamLibraryManager
{
    // TODO: Make this a configurable setting
    private const string SAM_GAME_LIST_URL = @"http://gib.me/sam/games.xml";


    private readonly ILog log = LogManager.GetLogger(nameof(SteamLibraryManager));

    private ConcurrentDictionary<uint, ISupportedApp> _gameList;
    private readonly HttpClient client = new ();
    private static readonly object syncLock = new ();
    private static SteamLibraryManager _instance;

    public IDictionary<uint, ISupportedApp> Apps
    {
        get
        {
            if (_gameList != null) return _gameList;

            _gameList = new (GetSupportedGames());

            return _gameList;
        }
    }
    public bool IsInitialized { get; private set; }
    public SteamLibrary Library { get; set; }

    public static SteamLibraryManager Default
    {
        get
        {
            if (_instance != null) return _instance;
            lock (syncLock)
            {
                _instance = new ();
            }
            return _instance;
        }
    }
    public static SteamLibrary DefaultLibrary => Default.Library;

    protected SteamLibraryManager()
    {

    }

    public void Init()
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Steam library is already initialized.");
        }

        try
        {
            var library = new SteamLibrary();
            library.Refresh();

            Default.Library = library;

            IsInitialized = true;
        }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to initialize the Steam library. {e.Message}";
            log.Error(message, e);

            throw new SAMInitializationException(message, e);
        }
    }

    public bool TryGetApp(uint id, out ISupportedApp app)
    {
        return Apps.TryGetValue(id, out app);
    }

    // TODO: give this a different name so that it's distinct from TryGetApp
    public ISupportedApp GetApp(uint id)
    {
        // this method is used when SAM is managing an app so that it loads only the
        // requested app. in every other situation, all Apps are loaded into the list
        var apps = GetSupportedGames(id);

        if (apps.TryGetValue(id, out var app)) return app;

        var message = $"App '{id}' is not currently supported.";
        throw new SAMException(message);
    }

    private IDictionary<uint, ISupportedApp> GetSupportedGames(uint? appId = null)
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

                _gameList[gameId] = new SupportedApp(gameId, type);
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
