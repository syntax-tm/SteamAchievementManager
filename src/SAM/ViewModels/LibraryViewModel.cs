using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Data;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Core.Extensions;
using SAM.Core.Messages;
using SAM.Core;
using SAM.Managers;
using SAM.Services;

namespace SAM.ViewModels;

[GenerateViewModel(ImplementISupportServices = true)]
public partial class LibraryViewModel
{
    protected readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));
    protected IGroupViewService groupViewService => GetService<IGroupViewService>();

    private readonly ObservableHandler<LibrarySettings> _settingsHandler;

    protected CollectionViewSource _itemsViewSource;
    protected bool _loading = true;
    private LibrarySettings _settings;
    
    [GenerateProperty] protected string _filterText;
    [GenerateProperty] protected bool _filterNormal;
    [GenerateProperty] protected bool _filterDemos;
    [GenerateProperty] protected bool _filterMods;
    [GenerateProperty] protected bool _filterJunk;
    [GenerateProperty] protected string _filterTool;
    [GenerateProperty] protected ICollectionView _itemsView;
    [GenerateProperty] protected List<string> _suggestions;
    [GenerateProperty] protected SteamApp _selectedItem;
    [GenerateProperty] protected SteamLibrary _library;

    protected LibraryViewModel(LibrarySettings settings)
    {
        _settings = settings;

        _settingsHandler = new ObservableHandler<LibrarySettings>(settings)
            .Add(s => s.EnableGrouping, OnEnableGroupingChanged)
            .Add(s => s.ShowHidden, OnShowHiddenChanged)
            .Add(s => s.ShowFavoritesOnly, OnFilterFavoritesChanged);

        Messenger.Default.Register<ActionMessage>(this, OnActionMessage);
    }
    
    [GenerateCommand]
    public void ToggleShowHidden()
    {
        if (_settings == null) return;

        _settings.ShowHidden = !_settings.ShowHidden;
    }
    
    [GenerateCommand]
    public void ToggleEnableGrouping()
    {
        if (_settings == null) return;

        _settings.EnableGrouping = !_settings.EnableGrouping;

        Refresh();
    }

    [GenerateCommand]
    public void UnHideAll()
    {
        // TODO: consider adding confirmation before clearing the user's hidden apps
        var hidden = _library!.Items.Where(a => a.IsHidden).ToList();

        hidden.ForEach(a => a.ToggleVisibility());
    }

    [GenerateCommand]
    public void ManageApp()
    {
        if (SelectedItem == null) return;

        SAMHelper.OpenManager(SelectedItem.Id);
    }

    [GenerateCommand]
    public void Refresh(bool force = false)
    {
        if (_settings == null) return;

        _loading = true;

        if (force)
        {
            SteamLibraryManager.DefaultLibrary.Refresh();
        }

        Library ??= SteamLibraryManager.DefaultLibrary;

        if (_itemsViewSource != null)
        {
            //_itemsViewSource.Filter -= ItemsViewSourceOnFilter;
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

            if (_settings.EnableGrouping)
            {
                _itemsViewSource.SortDescriptions.Add(new (nameof(SteamApp.GroupSortIndex), ListSortDirection.Ascending));
            }

            _itemsViewSource.SortDescriptions.Add(new (nameof(SteamApp.Name), ListSortDirection.Ascending));

            _itemsViewSource.LiveFilteringProperties.Clear();
            _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsHidden));
            _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsFavorite));
            _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.GameInfoType));
            
            _itemsViewSource.GroupDescriptions.Clear();

            if (_settings.EnableGrouping)
            {
                _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Group)));
            }

            _itemsViewSource.IsLiveFilteringRequested = true;
            _itemsViewSource.IsLiveSortingRequested = true;
            _itemsViewSource.IsLiveGroupingRequested = _settings.EnableGrouping;
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

    protected void OnFilterTextChanged()
    {
        if (_loading) return;

        ItemsView?.Refresh();
    }

    protected void OnShowHiddenChanged()
    {
        if (_loading) return;
        
        ItemsView?.Refresh();
    }

    protected void OnFilterFavoritesChanged()
    {
        if (_loading) return;
        
        ItemsView?.Refresh();
    }

    protected void OnEnableGroupingChanged()
    {
        if (_loading) return;

        Refresh();
    }

    protected virtual void OnActionMessage(ActionMessage message)
    {
        if (_loading) return;

        // on library refresh completed
        if (message.EntityType == EntityType.Library && message.ActionType == ActionType.Refreshed)
        {
            ItemsView?.Refresh();
        }
    }

    protected virtual void ItemsViewSourceOnFilter(object sender, FilterEventArgs e)
    {
        if (e.Item == null) return;
        if (e.Item is not SteamApp app) throw new ArgumentException(nameof(e.Item));
        if (_settings == null) return;

        var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
        var isNameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText) || app.Id.ToString().Contains(FilterText);
        var isJunkFiltered = !FilterJunk || app.IsJunk;
        var isHiddenFiltered = _settings.ShowHidden || !app.IsHidden;
        var isNonFavoriteFiltered = !_settings.ShowFavoritesOnly || app.IsFavorite;

        e.Accepted = isNameMatch && isJunkFiltered && isHiddenFiltered && isNonFavoriteFiltered;
    }
}
