using System;
using System.ComponentModel;
using System.Windows.Data;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.WPF.Core;
using SAM.WPF.Core.API;
using SAM.WPF.Core.Converters;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.ViewModels
{
    public class HomeViewModel
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));
        
        private readonly CollectionViewSource _itemsViewSource;
        
        public virtual bool EnableGrouping { get; set; }
        public virtual bool FilterJunk { get; set; }
        public virtual string FilterText { get; set; }
        public virtual ICollectionView ItemsView { get; set; }

        public SteamApp SelectedItem
        {
            get => (SteamApp) ItemsView.CurrentItem;
            set => ItemsView.MoveCurrentTo(value);
        }
        public virtual SteamLibrary Library { get; set; }

        protected HomeViewModel()
        {
            Library = SteamLibraryManager.DefaultLibrary;
            
            _itemsViewSource = new CollectionViewSource();
            _itemsViewSource.Source = Library.Items;
            ItemsView = _itemsViewSource.View;
            ItemsView.Filter += Filter;

            _itemsViewSource.IsLiveFilteringRequested = true;
            _itemsViewSource.IsLiveSortingRequested = true;
            
            using (_itemsViewSource.DeferRefresh())
            {
                _itemsViewSource.SortDescriptions.Clear();
                _itemsViewSource.SortDescriptions.Add(new SortDescription(nameof(SteamApp.Name), ListSortDirection.Ascending));
            }
            
            EnableGrouping = true;
        }

        public static HomeViewModel Create()
        {
            return ViewModelSource.Create(() => new HomeViewModel());
        }

        public void Loaded()
        {
        }
        
        protected void OnEnableGroupingChanged()
        {
            ItemsView.GroupDescriptions.Clear();

            _itemsViewSource.IsLiveGroupingRequested = EnableGrouping;

            if (EnableGrouping)
            {
                ItemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));
            }
        }
        
        private bool Filter(object obj)
        {
            if (obj is not SteamApp app) throw new ArgumentException(nameof(obj));

            var hasNameFilter = !string.IsNullOrWhiteSpace(FilterText);
            var nameMatch = !hasNameFilter || app.Name.ContainsIgnoreCase(FilterText);
            var isJunkFiltered = !FilterJunk || app.IsJunk;

            return nameMatch && isJunkFiltered;
        }
    }
}