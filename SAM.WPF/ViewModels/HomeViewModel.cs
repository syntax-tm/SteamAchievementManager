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
        
        public virtual int Columns { get; set; }
        public virtual bool EnableGrouping { get; set; }
        public virtual string FilterText { get; set; }
        public CollectionViewSource ItemsViewSource { get; set; }
        public ICollectionView ItemsView { get; set; }

        public SteamApp SelectedItem
        {
            get => (SteamApp) ItemsView.CurrentItem;
            set => ItemsView.MoveCurrentTo(value);
        }
        public virtual SteamLibrary Library { get; set; }

        protected HomeViewModel()
        {
            //Library = SteamLibrary.Create();
            //Library.Refresh(true);

            Library = SteamLibraryManager.DefaultLibrary;
            
            ItemsViewSource = new CollectionViewSource();
            ItemsViewSource.Source = Library.Items;
            ItemsView = ItemsViewSource.View;

            ItemsViewSource.IsLiveSortingRequested = true;

            //ItemsView.SortDescriptions.Clear();
            //ItemsView.SortDescriptions.Add(new SortDescription(nameof(SteamApp.Name), ListSortDirection.Ascending));

            using (ItemsViewSource.DeferRefresh())
            {
                ItemsViewSource.SortDescriptions.Clear();
                ItemsViewSource.SortDescriptions.Add(new SortDescription(nameof(SteamApp.Name), ListSortDirection.Ascending));
            }

            //ItemsViewSource.LiveSortingProperties.Add(nameof(SteamApp.Name));

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

            ItemsViewSource.IsLiveGroupingRequested = EnableGrouping;

            if (EnableGrouping)
            {
                ItemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SteamApp.Name), new StringToGroupConverter()));
            }
        }

        protected void OnFilterTextChanged()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                ItemsView.Filter += o => true;
                return;
            }

            ItemsView.Filter += o =>
            {
                if (!(o is SteamApp app))
                {
                    return false;
                }

                return app.Name.ContainsIgnoreCase(FilterText);
            };
        }

    }
}