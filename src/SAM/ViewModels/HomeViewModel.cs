using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Core.Messages;
using SAM.Core.Storage;
using SAM.Managers;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class HomeViewModel
{
    private readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));

    private CollectionViewSource _itemsViewSource;
    private bool _loading = true;
    private bool _imagesLoaded;

    [GenerateProperty] private bool _enableGrouping;
    [GenerateProperty] private string _filterText;
    [GenerateProperty] private string _filterNormal;
    [GenerateProperty] private string _filterDemos;
    [GenerateProperty] private string _filterMods;
    [GenerateProperty] private bool _filterJunk;
    [GenerateProperty] private bool _showHidden;
    [GenerateProperty] private bool _filterFavorites;
    [GenerateProperty] private string _filterTool;
    [GenerateProperty] private int _tileWidth = 100;
    [GenerateProperty] private ICollectionView _itemsView;
    [GenerateProperty] private List<string> _suggestions;
    [GenerateProperty] private SteamLibrary _library;
    [GenerateProperty] private LibraryView _libraryViewType;
    [GenerateProperty] private bool _showImages;
    [GenerateProperty] private bool _isTileView;
    [GenerateProperty] private bool _isGridView;
    [GenerateProperty] private bool _localImagesOnly;

    public SteamApp SelectedItem
    {
        get => (SteamApp) ItemsView!.CurrentItem;
        set => ItemsView!.MoveCurrentTo(value);
    }

    public HomeViewModel(HomeSettings settings = null)
    {
        // use the settings we were passed in (if any) or use current
        settings ??= new (true);

        LoadSettings(settings);

        Refresh();

        Messenger.Default.Register<ActionMessage>(this, OnActionMessage);

        _loading = false;
    }

    [GenerateCommand]
    public void ManageApp()
    {
        if (SelectedItem == null) return;

        SAMHelper.OpenManager(SelectedItem.Id);
    }

    [GenerateCommand]
    public void ToggleShowHidden()
    {
        ShowHidden = !ShowHidden;
    }
    
    [GenerateCommand]
    public void ToggleEnableGrouping()
    {
        EnableGrouping = !EnableGrouping;
        
        SaveSettings();
    }

    [GenerateCommand]
    public void UnHideAll()
    {
        // TODO: consider adding confirmation before clearing the user's hidden apps
        var hidden = Library!.Items.Where(a => a.IsHidden).ToList();

        hidden.ForEach(a => a.ToggleVisibility());

        ItemsView?.Refresh();
    }

    [GenerateCommand]
    public void Refresh(bool force = false)
    {
        _loading = true;

        if (force)
        {
            SteamLibraryManager.DefaultLibrary.Refresh();
        }

        Library ??= SteamLibraryManager.DefaultLibrary;

        if (_itemsViewSource != null)
        {
            _itemsViewSource.Filter -= ItemsViewSourceOnFilter;
            _itemsViewSource = null;
        }

        _itemsViewSource = new ()
        {
            Source = Library.Items
        };

        using (_itemsViewSource.DeferRefresh())
        {
            _itemsViewSource.Filter += ItemsViewSourceOnFilter;
            
            _itemsViewSource.SortDescriptions.Clear();

            if (EnableGrouping)
            {
                _itemsViewSource.SortDescriptions.Add(new (nameof(SteamApp.GroupSortIndex), ListSortDirection.Ascending));
            }

            _itemsViewSource.SortDescriptions.Add(new (nameof(SteamApp.Name), ListSortDirection.Ascending));

            _itemsViewSource.LiveFilteringProperties.Clear();
            _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsHidden));
            _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsFavorite));
            
            _itemsViewSource.GroupDescriptions.Clear();

            if (EnableGrouping)
            {
                _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Group)));
            }

            _itemsViewSource.IsLiveFilteringRequested = true;
            _itemsViewSource.IsLiveSortingRequested = true;
            _itemsViewSource.IsLiveGroupingRequested = EnableGrouping;
        }

        ItemsView = _itemsViewSource.View;

        // suggestions are sorted by favorites first, then normal (non-favorite & non-hidden) apps,
        // and then any hidden apps
        Suggestions = Library.Items
                             .OrderBy(a => a.GroupSortIndex)
                             .ThenBy(a => a.Name)
                             .Select(a => a.Name).ToList();

        _loading = false;
    }
        
    [GenerateCommand]
    public void ShowLibraryGrid()
    {
        LibraryViewType = LibraryView.DataGrid;
    }

    [GenerateCommand]
    public void ShowLibraryTile()
    {
        LibraryViewType = LibraryView.Tile;
    }
    
    [GenerateCommand]
    public void ToggleShowImages()
    {
        ShowImages = !ShowImages;
    }

    [GenerateCommand]
    public void ToggleLocalImagesOnly()
    {
        LocalImagesOnly = !LocalImagesOnly;
    }

    [GenerateCommand]
    public async Task LoadAllImages()
    {
        if (Library == null) return;
        if (_imagesLoaded) return;

        var apps = Library.Items.ToList();
        var loadImageTasks = apps.Select(a => a.LoadImagesAsync()).ToArray();
        await Task.WhenAll(loadImageTasks).ConfigureAwait(false);

        _imagesLoaded = true;
    }

    public bool CanLoadAllImages()
    {
        return !_imagesLoaded;
    }

    protected void OnLibraryViewTypeChanged()
    {
        IsGridView = LibraryViewType == LibraryView.DataGrid;
        IsTileView = LibraryViewType == LibraryView.Tile;

        SaveSettings();
    }

    protected void OnLocalImagesOnlyChanged()
    {
        if (_loading) return;

        SaveSettings();
    }

    protected void OnShowImagesChanged()
    {
        if (_loading) return;

        SaveSettings();

        Refresh();
    }

    protected void OnFilterTextChanged()
    {
        if (_loading) return;

        //ItemsView!.Refresh();
    }

    protected void OnShowHiddenChanged()
    {
        if (_loading) return;

        SaveSettings();

        Refresh();
    }

    protected void OnFilterFavoritesChanged()
    {
        if (_loading) return;

        SaveSettings();

        Refresh();
    }

    protected void OnEnableGroupingChanged()
    {
        if (_loading) return;

        Refresh();
    }

    private void OnActionMessage(ActionMessage message)
    {
        if (_loading) return;

        // on library refresh completed
        if (message.EntityType == EntityType.Library && message.ActionType == ActionType.Refreshed)
        {
            ItemsView?.Refresh();
        }
    }

    private void ItemsViewSourceOnFilter(object sender, FilterEventArgs e)
    {
        if (e.Item == null) return;
        if (e.Item is not SteamApp app) throw new ArgumentException(nameof(e.Item));

        var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
        var isNameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText) || app.Id.ToString().Contains((string)FilterText);
        var isJunkFiltered = !FilterJunk || app.IsJunk;
        var isHiddenFiltered = ShowHidden || !app.IsHidden;
        var isNonFavoriteFiltered = !FilterFavorites || app.IsFavorite;

        e.Accepted = isNameMatch && isJunkFiltered && isHiddenFiltered && isNonFavoriteFiltered;
    }

    private void LoadSettings(HomeSettings settings)
    {
        _loading = true;

        ShowImages = settings.ShowImages;
        LocalImagesOnly = settings.LocalImagesOnly;
        EnableGrouping = settings.EnableGrouping;
        LibraryViewType = settings.LibraryView;
        IsGridView = LibraryViewType == LibraryView.DataGrid;
        IsTileView = LibraryViewType == LibraryView.Tile;

        _loading = false;
    }

    private void SaveSettings()
    {
        var settings = new HomeSettings
        {
            EnableGrouping = EnableGrouping,
            ShowImages = ShowImages,
            LibraryView = LibraryViewType,
            LocalImagesOnly = LocalImagesOnly
        };

        settings.Save();
    }
}

[GenerateViewModel(ImplementISupportParentViewModel = true)]
public partial class HomeSettings
{
    private const string FILENAME = @"homeSettings.json";

    private readonly ILog log = LogManager.GetLogger(typeof(HomeSettings));

    private bool _loaded;
    private bool _loading;
    private readonly object syncLock = new ();
    private readonly CacheKey cacheKey = new (FILENAME, CacheKeyType.Settings);

    [JsonConverter(typeof(StringEnumConverter))]
    public LibraryView LibraryView { get; set; } = LibraryView.Tile;
    public bool ShowImages { get; set; } = true;
    public bool LocalImagesOnly { get; set; } = true;
    public bool EnableGrouping { get; set; } = true;
    
    public HomeSettings()
    {

    }

    public HomeSettings(bool load)
    {
        if (!load) return;

        Load();
    }

    public void Load()
    {
        if (_loaded) return;

        try
        {
            _loading = true;

            var exists = CacheManager.TryGetObject<HomeSettings>(cacheKey, out var homeSettings);

            if (!exists)
            {
                // use defaults
                return;
            }

            LibraryView = homeSettings.LibraryView;
            ShowImages = homeSettings.ShowImages;
            EnableGrouping = homeSettings.EnableGrouping;
            LocalImagesOnly = homeSettings.LocalImagesOnly;

            _loaded = true;
        }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to load the {nameof(HomeSettings)}. {e.Message}";
            log.Error(message, e);
        }
        finally
        {
            _loading = false;
        }
    }

    public void Save()
    {
        try
        {
            lock (syncLock)
            {
                CacheManager.CacheObject(cacheKey, this);

                log.Debug($"Saved {nameof(HomeSettings)} to '{FILENAME}'.");
            }
        }
        catch (Exception e)
        {
            var message = $"An error occurred attempting to save the {nameof(HomeSettings)}. {e.Message}";
            log.Error(message, e);
        }
    }
}
