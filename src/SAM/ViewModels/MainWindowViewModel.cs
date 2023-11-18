using DevExpress.Mvvm.POCO;
using SAM.Core;
using SAM.Core.ViewModels;

namespace SAM.ViewModels
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        public virtual SteamUser User { get; set; }
        public virtual HomeViewModel HomeVm { get; set; }

        protected MainWindowViewModel()
        {
            User = SteamUser.Create();
            HomeVm = HomeViewModel.Create();
        }

        public static MainWindowViewModel Create()
        {
            return ViewModelSource.Create(() => new MainWindowViewModel());
        }
    }
}