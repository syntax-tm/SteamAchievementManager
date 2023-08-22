using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.API;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Cache;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    public class SteamLibrary
    {

        private readonly ILog log = LogManager.GetLogger(nameof(SteamLibrary));

        private readonly BackgroundWorker _libraryWorker;

        private static readonly object _lock = new object();
        private readonly List<SupportedApp> _supportedGames;
        private Queue<SupportedApp> _refreshQueue;
        private List<SupportedApp> _addedGames;

        public virtual int QueueCount { get; set; }
        public virtual int CompletedCount { get; set; }
        public virtual int SupportedGamesCount { get; set; }

        public virtual int TotalCount { get; set; }
        public virtual int GamesCount { get; set; }
        public virtual int JunkCount { get; set; }
        public virtual int ToolCount { get; set; }
        public virtual int ModCount { get; set; }
        public virtual int DemoCount { get; set; }
        public virtual decimal PercentComplete { get; set; }
        public virtual bool IsLoading { get; set; }
        public virtual ObservableCollection<SteamApp> Items { get; set; }

        protected SteamLibrary()
        {
            _supportedGames = SAMLibraryHelper.GetSupportedGames();

            SupportedGamesCount = _supportedGames.Count;

            _libraryWorker = new BackgroundWorker();
            _libraryWorker.WorkerSupportsCancellation = true;
            _libraryWorker.WorkerReportsProgress = true;
            _libraryWorker.DoWork += LibraryWorkerOnDoWork;
            _libraryWorker.RunWorkerCompleted += LibraryWorkerOnRunWorkerCompleted;
        }

        public static SteamLibrary Create()
        {
            return ViewModelSource.Create(() => new SteamLibrary());
        }

        public void Refresh(bool loadCache = false)
        {
            _refreshQueue = new Queue<SupportedApp>(_supportedGames);
            _addedGames = new List<SupportedApp>();

            Items = new ObservableCollection<SteamApp>();
            BindingOperations.EnableCollectionSynchronization(Items, _lock);

            //if (loadCache)
            //{
            //    LoadLibraryCache();
            //    LoadRefreshProgress();
            //}

            CancelRefresh();

            _libraryWorker.RunWorkerAsync();
        }

        public void CancelRefresh()
        {
            if (!_libraryWorker.IsBusy) return;

            _libraryWorker.CancelAsync();

            while (_libraryWorker.IsBusy)
            {
                System.Threading.Thread.Sleep(250);
            }
        }

        private void ProgressUpdate()
        {
            QueueCount = _refreshQueue.Count;
            CompletedCount = SupportedGamesCount - QueueCount;
            PercentComplete = (decimal) CompletedCount / SupportedGamesCount;
        }
        
        private void LibraryWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            IsLoading = true;
            
            while (_refreshQueue.TryDequeue(out var game))
            {
                if (_libraryWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                AddGame(game);

                if (_refreshQueue.Count % 5 == 0) ProgressUpdate();
                //if (_refreshQueue.Count % 10 == 0) CacheRefreshProgress();
            }
        }
        
        private void LibraryWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressUpdate();

            IsLoading = false;
        }

        private void AddGame(SupportedApp app)
        {
            var type = Enum.Parse<GameInfoType>(app.Type, true);

            if (type != GameInfoType.Normal && type != GameInfoType.Mod) return;
            if (_addedGames.Contains(app)) return;
            
            if (!SteamClientManager.Default.OwnsGame(app.Id)) return;

            var steamGame = new SteamApp(app.Id, type);

            Items.Add(steamGame);
                
            _addedGames.Add(app);

            TotalCount = Items.Count;
            GamesCount = Items.Count(g => g.GameInfoType == GameInfoType.Normal);
            ModCount = Items.Count(g => g.GameInfoType == GameInfoType.Mod);
            ToolCount = Items.Count(g => g.GameInfoType == GameInfoType.Tool);
            JunkCount = Items.Count(g => g.GameInfoType == GameInfoType.Junk);
            DemoCount = Items.Count(g => g.GameInfoType == GameInfoType.Demo);

            //CacheLibrary();
        }

        private void LoadRefreshProgress()
        {
            var cacheKey = CacheKeyFactory.CreateCheckedAppsCacheKey();
            if (!CacheManager.TryGetObject<Queue<SupportedApp>>(cacheKey, out var refreshQueue)) return;

            _refreshQueue = refreshQueue;
        }

        private void CacheRefreshProgress()
        {
            var cacheKey = CacheKeyFactory.CreateCheckedAppsCacheKey();

            CacheManager.CacheObject(cacheKey, _refreshQueue);
        }

        private void LoadLibraryCache()
        {
            var cacheKey = CacheKeyFactory.CreateUserLibraryCacheKey();
            if (!CacheManager.TryGetObject<List<SupportedApp>>(cacheKey, out var ownedApps)) return;

            foreach (var app in ownedApps)
            {
                if (!SAMLibraryHelper.TryGetApp(app.Id, out var appInfo))
                {
                    log.Warn($"App with ID '{app.Id}' was in the local cache but was not found in supported app list.");
                }

                AddGame(appInfo);
                
                _supportedGames.Remove(appInfo);
            }
        }

        private void CacheLibrary()
        {
            var ownedApps = _addedGames.ToList();
            var cacheKey = CacheKeyFactory.CreateUserLibraryCacheKey();
            
            CacheManager.CacheObject(cacheKey, ownedApps);
        }
    }
}
