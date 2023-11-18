using System;
using System.ComponentModel;
using System.Windows.Data;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.Core.Converters;
using SAM.Core.Extensions;

namespace SAM.Core.ViewModels
{
    public class HomeViewModel
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));
        
        private CollectionViewSource _itemsViewSource;
        private bool _loading = true;

        public virtual bool EnableGrouping { get; set; }
        public virtual string FilterText { get; set; }
        public virtual string FilterNormal { get; set; }
        public virtual string FilterDemos { get; set; }
        public virtual string FilterMods { get; set; }
        public virtual bool FilterJunk { get; set; }
        public virtual bool ShowHidden { get; set; }
        public virtual bool FilterFavorites { get; set; }
        public virtual string FilterTool { get; set; }
        public virtual int TileWidth { get; set; } = 100;
        public virtual ICollectionView ItemsView { get; set; }

        public SteamApp SelectedItem
        {
            get => (SteamApp) ItemsView.CurrentItem;
            set => ItemsView.MoveCurrentTo(value);
        }
        public virtual SteamLibrary Library { get; set; }

        protected HomeViewModel()
        {
            Refresh();
        }
        
        public static HomeViewModel Create()
        {
            return ViewModelSource.Create(() => new HomeViewModel());
        }

        private void ItemsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not SteamApp app) throw new ArgumentException(nameof(e.Item));

            var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
            var isNameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText);
            var isJunkFiltered = !FilterJunk || app.IsJunk;
            var isHiddenFiltered = ShowHidden || !app.IsHidden;
            var isNonFavoriteFiltered = !FilterFavorites || app.IsFavorite;
            
            e.Accepted = isNameMatch && isJunkFiltered && isHiddenFiltered && isNonFavoriteFiltered;
        }

        public void Loaded()
        {
        }

        public void Refresh(bool force = true)
        {
            _loading = true;

            if (force)
            {
                //SteamLibraryManager.Refresh();
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

                //_itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));

                _itemsViewSource.IsLiveFilteringRequested = true;
                _itemsViewSource.IsLiveSortingRequested = true;
                _itemsViewSource.IsLiveGroupingRequested = false;
            }

            _loading = false;
        }

        protected void OnFilterTextChanged()
        {
            if (_loading) return;

            using (_itemsViewSource.DeferRefresh())
            {
                _itemsViewSource.IsLiveFilteringRequested = !string.IsNullOrWhiteSpace(FilterText) || FilterJunk;
            }
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
                    ItemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));
                }
            }
        }
    }
}