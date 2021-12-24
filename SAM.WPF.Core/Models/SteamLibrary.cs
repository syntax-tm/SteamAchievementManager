using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using DevExpress.Mvvm;
using log4net;
using SAM.WPF.Core.API;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    public class SteamLibrary : BindableBase
    {

        private readonly ILog log = LogManager.GetLogger(nameof(SteamLibrary));

        private readonly BackgroundWorker _libraryWorker;

        private static readonly object _lock = new ();
        private readonly List<SupportedApp> _supportedGames;
        private Queue<SupportedApp> _refreshQueue;
        private List<SupportedApp> _addedGames;

        public int QueueCount 
        {
            get => GetProperty(() => QueueCount);
            set => SetProperty(() => QueueCount, value);
        }
        public int CompletedCount 
        {
            get => GetProperty(() => CompletedCount);
            set => SetProperty(() => CompletedCount, value);
        }
        public int SupportedGamesCount
        {
            get => GetProperty(() => SupportedGamesCount);
            set => SetProperty(() => SupportedGamesCount, value);
        }

        public int TotalCount
        {
            get => GetProperty(() => TotalCount);
            set => SetProperty(() => TotalCount, value);
        }
        public int GamesCount 
        {
            get => GetProperty(() => GamesCount);
            set => SetProperty(() => GamesCount, value);
        }
        public int JunkCount 
        {
            get => GetProperty(() => JunkCount);
            set => SetProperty(() => JunkCount, value);
        }
        public int ToolCount 
        {
            get => GetProperty(() => ToolCount);
            set => SetProperty(() => ToolCount, value);
        }
        public int ModCount 
        {
            get => GetProperty(() => ModCount);
            set => SetProperty(() => ModCount, value);
        }
        public int DemoCount 
        {
            get => GetProperty(() => DemoCount);
            set => SetProperty(() => DemoCount, value);
        }
        public decimal PercentComplete
        {
            get => GetProperty(() => PercentComplete);
            set => SetProperty(() => PercentComplete, value);
        }
        public bool IsLoading
        {
            get => GetProperty(() => IsLoading);
            set => SetProperty(() => IsLoading, value);
        }
        
        public ObservableCollection<SteamApp> Items { get; private set; }

        public SteamLibrary()
        {
            _supportedGames = SAMLibraryHelper.GetSupportedGames();

            SupportedGamesCount = _supportedGames.Count;
            
            Items = new ObservableCollection<SteamApp>();
            BindingOperations.EnableCollectionSynchronization(Items, _lock);
            
            _libraryWorker = new BackgroundWorker();
            _libraryWorker.WorkerSupportsCancellation = true;
            _libraryWorker.WorkerReportsProgress = true;
            _libraryWorker.DoWork += LibraryWorkerOnDoWork;
            _libraryWorker.RunWorkerCompleted += LibraryWorkerOnRunWorkerCompleted;
        }

        //public static SteamLibrary Create()
        //{
        //    return ViewModelSource.Create(() => new SteamLibrary());
        //}

        public void Refresh(bool loadCache = false)
        {
            _refreshQueue = new Queue<SupportedApp>(_supportedGames);
            _addedGames = new List<SupportedApp>();

            Items.Clear();

            if (loadCache)
            {
                LoadLibraryCache();
                LoadRefreshProgress();
            }

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
        
        private void LibraryWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                IsLoading = true;

                while (_refreshQueue.TryDequeue(out var game))
                {
                    if (_libraryWorker.CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }

                    AddGame(game);

                    if (_refreshQueue.Count % 15 == 0) ProgressUpdate();
                    if (_refreshQueue.Count % 30 == 0) CacheRefreshProgress();
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred refreshing the Steam library. {e.Message}";
                log.Error(message, e);
            }
            finally
            {
                ProgressUpdate();
                CacheRefreshProgress();
                RefreshCounts();
            }
        }
        
        private void LibraryWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressUpdate();

            IsLoading = false;
        }

        private void AddGame(SupportedApp app)
        {
            try
            {
                var type = Enum.Parse<GameInfoType>(app.Type, true);

                if (type != GameInfoType.Normal && type != GameInfoType.Mod) return;
                if (_addedGames.Contains(app)) return;

                if (!SteamClientManager.Default.OwnsGame(app.Id)) return;

                var steamGame = new SteamApp(app.Id, type);

                Items.Add(steamGame);

                _addedGames.Add(app);

                TotalCount = Items.Count;

                CacheLibrary();
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to add app '{app?.Id}'. {e.Message}";
                log.Error(message, e);
            }
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

            RefreshCounts();
        }

        private void RefreshCounts()
        {
            TotalCount = Items.Count;
            GamesCount = Items.Count(g => g.GameInfoType == GameInfoType.Normal);
            ModCount = Items.Count(g => g.GameInfoType == GameInfoType.Mod);
            ToolCount = Items.Count(g => g.GameInfoType == GameInfoType.Tool);
            JunkCount = Items.Count(g => g.GameInfoType == GameInfoType.Junk);
            DemoCount = Items.Count(g => g.GameInfoType == GameInfoType.Demo);
        }

        private void CacheLibrary()
        {
            var ownedApps = _addedGames.ToList();
            var cacheKey = CacheKeyFactory.CreateUserLibraryCacheKey();
            
            CacheManager.CacheObject(cacheKey, ownedApps);
        }
    }
}
