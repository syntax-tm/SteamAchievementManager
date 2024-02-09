using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using log4net;
using SAM.API;
using SAM.Core.Storage;

namespace SAM.Core;

public class SteamLibrary : BindableBase
{
	private const int CACHE_INTERVAL = 25;
	private const int PROGRESS_INTERVAL = 10;

	private readonly ILog log = LogManager.GetLogger(nameof(SteamLibrary));

	private readonly BackgroundWorker _libraryWorker;
	private AutoResetEvent _resetEvent;

	private static readonly object _lock = new();
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
	public int HiddenCount
	{
		get => GetProperty(() => HiddenCount);
		set => SetProperty(() => HiddenCount, value);
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

	public ObservableCollection<SteamApp> Items
	{
		get;
	}

	public SteamLibrary ()
	{
		_supportedGames = SAMLibraryHelper.GetSupportedGames();

		SupportedGamesCount = _supportedGames.Count;

		Items = [];
		BindingOperations.EnableCollectionSynchronization(Items, _lock);

		_libraryWorker = new();
		_libraryWorker.WorkerSupportsCancellation = true;
		_libraryWorker.WorkerReportsProgress = true;
		_libraryWorker.DoWork += LibraryWorkerOnDoWork;
		_libraryWorker.RunWorkerCompleted += LibraryWorkerOnRunWorkerCompleted;
	}

	public void Refresh (bool loadCache = false)
	{
		_resetEvent ??= new(false);

		_refreshQueue = new(_supportedGames);
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

	public void CancelRefresh ()
	{
		if (!_libraryWorker.IsBusy)
			return;

		_libraryWorker.CancelAsync();

		_resetEvent?.WaitOne();
	}

	private async void LibraryWorkerOnDoWork (object sender, DoWorkEventArgs args)
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

			var refreshTasks = Items.Select(async i => await i.Load());

			await Task.WhenAll(refreshTasks);
		}
		catch (Exception e)
		{
			var message = $"An error occurred refreshing the Steam library. {e.Message}";
			log.Error(message, e);
		}
		finally
		{
			CacheLibrary();
			//CacheRefreshProgress();
			RefreshCounts();

			_resetEvent.Set();
		}
	}

	private void LibraryWorkerOnRunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
	{
		RefreshCounts();

		IsLoading = false;
	}

	private bool AddGame (SupportedApp app)
	{
		try
		{
			var type = Enum.Parse<GameInfoType>(app.Type, true);

			if (type is not GameInfoType.Normal and not GameInfoType.Mod)
				return false;
			if (_addedGames.Contains(app))
				return false;

			if (!SteamClientManager.Default.OwnsGame(app.Id))
				return false;

			var steamGame = new SteamApp(app.Id, type);

			Items.Add(steamGame);

			_addedGames.Add(app);

			CacheLibrary();

			return true;
		}
		catch (Exception e)
		{
			var message = $"Error attempting to add app '{app?.Id}'. {e.Message}";
			log.Error(message, e);

			throw new SAMException(message, e);
		}
	}

	private void LoadRefreshProgress ()
	{
		try
		{
			if (!CacheManager.TryGetObject<List<SupportedApp>>(CacheKeys.CheckedAppList, out var refreshQueue))
				return;

			_refreshQueue = new(refreshQueue);
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to load refresh progress. {e.Message}";
			log.Error(message, e);
		}
	}

	private void CacheRefreshProgress ()
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

	private void LoadLibrary ()
	{
		try
		{
			if (!CacheManager.TryGetObject<List<SupportedApp>>(CacheKeys.UserLibrary, out var ownedApps))
				return;

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
		catch (Exception e)
		{
			var message = $"An error occurred attempting to load library cache. {e.Message}";
			log.Error(message, e);
		}
	}

	private void CacheLibrary ()
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

	private void RefreshCounts ()
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
	}
}
