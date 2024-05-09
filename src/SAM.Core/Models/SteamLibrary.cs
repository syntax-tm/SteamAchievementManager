using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.API;
using SAM.Core.Messages;
using SAM.Core.Storage;

namespace SAM.Core
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Multiple private readonly fields.")]
    [GenerateViewModel]
    public partial class SteamLibrary : BindableBase
    {
        private const int CACHE_INTERVAL = 25;
        private const int PROGRESS_INTERVAL = 10;

        private readonly ILog log = LogManager.GetLogger(nameof(SteamLibrary));

        private readonly BackgroundWorker _libraryWorker;
        private AutoResetEvent _resetEvent;

        private static readonly object _lock = new ();
        private readonly List<SupportedApp> _supportedGames;
        private Queue<SupportedApp> _refreshQueue;
        private List<SupportedApp> _addedGames;
        
        [GenerateProperty] private int _queueCount;
        [GenerateProperty] private int _completedCount;
        [GenerateProperty] private int _supportedGamesCount;
        [GenerateProperty] private int _totalCount;
        [GenerateProperty] private int _gamesCount;
        [GenerateProperty] private int _junkCount;
        [GenerateProperty] private int _toolCount;
        [GenerateProperty] private int _modCount;
        [GenerateProperty] private int _demoCount;
        [GenerateProperty] private int _hiddenCount;
        [GenerateProperty] private int _favoriteCount;
        [GenerateProperty] private decimal _percentComplete;
        [GenerateProperty] private bool _isLoading;
        
        public ObservableCollection<SteamApp> Items { get; }

        public SteamLibrary()
        {
            _supportedGames = SAMLibraryHelper.GetSupportedGames();

            SupportedGamesCount = _supportedGames.Count;
            
            Items = [];
            BindingOperations.EnableCollectionSynchronization(Items, _lock);
            
            Messenger.Default.Register<RequestMessage>(this, OnRequestMessage);

            _libraryWorker = new ()
            {
                Site = null,
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            _libraryWorker.WorkerSupportsCancellation = true;
            _libraryWorker.WorkerReportsProgress = true;
            _libraryWorker.DoWork += LibraryWorkerOnDoWork;
            _libraryWorker.RunWorkerCompleted += LibraryWorkerOnRunWorkerCompleted;
        }

        private void OnRequestMessage(RequestMessage message)
        {
            try
            {
                if (message == null) return;
                if (message.EntityType != EntityType.Library) return;

                // if we received a refresh request refresh the counts
                if (message.RequestType == RequestType.Refresh)
                {
                    RefreshCounts();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Refresh(bool loadCache = false)
        {
            _resetEvent ??= new (false);

            _refreshQueue = new (_supportedGames);
            _addedGames = [];

            Items.Clear();

            if (loadCache)
            {
                LoadLibrary();
                //LoadRefreshProgress();
            }

            CancelRefresh();

            if (!_libraryWorker.IsBusy)
            {
                _libraryWorker.RunWorkerAsync();
            }
        }
        
        public void CancelRefresh()
        {
            if (!_libraryWorker.IsBusy) return;

            _libraryWorker.CancelAsync();
            
            _ = _resetEvent?.WaitOne();
        }
        
        private async void LibraryWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                IsLoading = true;
                var checkedCount = 0;

                while (_refreshQueue.TryDequeue(out var game))
                {
                    if (_libraryWorker.CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }
                    
                    var added = AddGame(game);
                    
                    // TODO: remove after testing more since caching is likely not needed.
                    //var isCacheInterval = checkedCount % CACHE_INTERVAL == 0;
                    //if (added || isCacheInterval)
                    //{
                    //    // CacheRefreshProgress();
                    //}

                    var isRefreshCountInterval = checkedCount % PROGRESS_INTERVAL == 0;
                    if (added || isRefreshCountInterval)
                    {
                        RefreshCounts();
                    }
                    
                    checkedCount++;
                }

                var refreshTasks = Items.Select(async i => await i.Load().ConfigureAwait(false));

                await Task.WhenAll(refreshTasks);
            }
            catch (Exception e)
            {
                var message = $"An error occurred refreshing the Steam library. {e.Message}";
                log.Error(message, e);
            }
            finally
            {
                // TODO: remove after testing cache removal more
                //CacheLibrary();
                //CacheRefreshProgress();
                RefreshCounts();

#pragma warning disable IDE0058 // Expression value is never used
                _resetEvent.Set();
#pragma warning restore IDE0058 // Expression value is never used
            }
        }
        
        private void LibraryWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshCounts();

            Messenger.Default.Send(new ActionMessage(EntityType.Library, ActionType.Refreshed));

            IsLoading = false;
        }

        private bool AddGame(SupportedApp app)
        {
            try
            {
                var type = app.GameInfoType;

                // TODO: allow users to configure filters for the types
                if (type is not GameInfoType.Normal and not GameInfoType.Mod) return false;
                if (_addedGames.Contains(app)) return false;

                if (!SteamClientManager.Default.OwnsGame(app.Id)) return false;

                var steamGame = new SteamApp(app.Id, type);

                Items.Add(steamGame);

                _addedGames.Add(app);

                return true;
            }
            catch (Exception e)
            {
                var message = $"Error attempting to add app '{app?.Id}'. {e.Message}";
                log.Error(message, e);

                throw new SAMException(message, e);
            }
        }

        private void LoadRefreshProgress()
        {
            try
            {
                if (!CacheManager.TryGetObject<List<SupportedApp>>(CacheKeys.CheckedAppList, out var refreshQueue)) return;

                _refreshQueue = new (refreshQueue);
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to load refresh progress. {e.Message}";
                log.Error(message, e);
            }
        }

        private void CacheRefreshProgress()
        {
            try
            {
                var refreshItems = _refreshQueue.ToList();

                CacheManager.CacheObject(CacheKeys.CheckedAppList, refreshItems);
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to cache refresh progress. {e.Message}";
                log.Error(message, e);
            }
        }

        private void LoadLibrary()
        {
            try
            {
                if (!CacheManager.TryGetObject<List<SupportedApp>>(CacheKeys.UserLibrary, out var ownedApps)) return;

                foreach (var app in ownedApps)
                {
                    if (!SAMLibraryHelper.TryGetApp(app.Id, out var appInfo))
                    {
                        log.Warn($"App with ID '{app.Id}' was in the local cache but was not found in supported app list.");
                    }

                    var added = AddGame(appInfo);

                    if (!added)
                    {
                        log.Warn($"Failed to add app '{appInfo.Id}' from saved library.");
                    }

                    _ = _supportedGames.Remove(appInfo);
                }

                RefreshCounts();
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to load library cache. {e.Message}";
                log.Error(message, e);
            }
        }

        private void CacheLibrary()
        {
            try
            {
                var ownedApps = _addedGames.ToList();
            
                CacheManager.CacheObject(CacheKeys.UserLibrary, ownedApps);
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to cache user library. {e.Message}";
                log.Error(message, e);
            }
        }

        private void RefreshCounts()
        {
            QueueCount = _refreshQueue.Count;
            CompletedCount = SupportedGamesCount - QueueCount;
            PercentComplete = (decimal) CompletedCount / SupportedGamesCount;

            TotalCount = Items.Count;
            GamesCount = Items.Count(g => g.GameInfoType == GameInfoType.Normal);
            ModCount = Items.Count(g => g.GameInfoType == GameInfoType.Mod);
            ToolCount = Items.Count(g => g.GameInfoType == GameInfoType.Tool);
            JunkCount = Items.Count(g => g.GameInfoType == GameInfoType.Junk);
            DemoCount = Items.Count(g => g.GameInfoType == GameInfoType.Demo);
            HiddenCount = Items.Count(g => g.IsHidden);
            FavoriteCount = Items.Count(g => g.IsFavorite);
        }
    }
}
