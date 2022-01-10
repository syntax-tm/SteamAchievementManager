using DevExpress.Mvvm.POCO;
using SAM.Core.ViewModels;

namespace SAM.Manager.ViewModels
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        public virtual SteamGameViewModel GameVm { get; set; }

        protected MainWindowViewModel(SteamGameViewModel gameVm)
        {
            GameVm = gameVm;
        }

        public static MainWindowViewModel Create(SteamGameViewModel gameVm)
        {
            return ViewModelSource.Create(() => new MainWindowViewModel(gameVm));
        }
    }
}
