using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Converters;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Core.Messages;
using SAM.Managers;

namespace SAM.ViewModels
{
    [GenerateViewModel]
    public partial class HomeViewModel
    {
        private readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));

        private CollectionViewSource _itemsViewSource;
        private bool _loading = true;

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

        public SteamApp SelectedItem
        {
            get => (SteamApp) ItemsView!.CurrentItem;
            set => ItemsView!.MoveCurrentTo(value);
        }

        public HomeViewModel()
        {
            Refresh();

            Messenger.Default.Register<ActionMessage>(this, OnActionMessage);
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
        public void UnHideAll()
        {
            // TODO: consider adding confirmation before clearing the user's hidden apps
            var hidden = Enumerable.Where<SteamApp>(Library!.Items, a => a.IsHidden).ToList();

            hidden.ForEach(a => a.ToggleVisibility());

            Messenger.Default.Send<RequestMessage>(new (EntityType.Library, RequestType.Refresh));
        }

        [GenerateCommand]
        public void Refresh(bool force = false)
        {
            _loading = true;

            if (force)
            {
                SteamLibraryManager.DefaultLibrary.Refresh();
            }

            Library = SteamLibraryManager.DefaultLibrary;

            _itemsViewSource = new ()
            {
                Source = Library.Items
            };
            ItemsView = _itemsViewSource.View;

            using (_itemsViewSource.DeferRefresh())
            {
                _itemsViewSource.Filter += ItemsViewSourceOnFilter;

                _itemsViewSource.SortDescriptions.Clear();
                _itemsViewSource.SortDescriptions.Add(new (nameof(SteamApp.Name), ListSortDirection.Ascending));

                _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsHidden));
                _itemsViewSource.LiveFilteringProperties.Add(nameof(SteamApp.IsFavorite));

                _itemsViewSource.IsLiveFilteringRequested = true;
                _itemsViewSource.IsLiveSortingRequested = true;
                _itemsViewSource.IsLiveGroupingRequested = false;
            }

            // suggestions are sorted by favorites first, then normal (non-favorite & non-hidden) apps,
            // and then any hidden apps
            Suggestions = Enumerable
                                .OrderByDescending<SteamApp, bool>(Library.Items, a => a.IsFavorite)
                                .ThenBy(a => a.IsHidden)
                                .ThenBy(a => a.Name)
                                .Select(a => a.Name).ToList();

            _loading = false;
        }

        protected void OnFilterTextChanged()
        {
            if (_loading) return;

            ItemsView!.Refresh();
        }

        protected void OnShowHiddenChanged()
        {
            if (_loading) return;

            ItemsView!.Refresh();
        }

        protected void OnFilterFavoritesChanged()
        {
            if (_loading) return;

            ItemsView!.Refresh();
        }

        protected void OnEnableGroupingChanged()
        {
            if (_loading) return;

            using (_itemsViewSource.DeferRefresh())
            {
                _itemsViewSource.GroupDescriptions.Clear();
                _itemsViewSource.IsLiveGroupingRequested = EnableGrouping;

                if (EnableGrouping)
                {
                    ItemsView!.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));
                }
            }
        }

        private void OnActionMessage(ActionMessage message)
        {
            // on library refresh completed
            if (message.EntityType == EntityType.Library && message.ActionType == ActionType.Refreshed)
            {
                Refresh();
            }
        }

        private void ItemsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not SteamApp app) throw new ArgumentException(nameof(e.Item));

            var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
            var isNameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText) || app.Id.ToString().Contains((string)FilterText);
            var isJunkFiltered = !FilterJunk || app.IsJunk;
            var isHiddenFiltered = ShowHidden || !app.IsHidden;
            var isNonFavoriteFiltered = !FilterFavorites || app.IsFavorite;

            e.Accepted = isNameMatch && isJunkFiltered && isHiddenFiltered && isNonFavoriteFiltered;
        }
    }
}
