using DevExpress.Mvvm.POCO;
using SAM.Core.ViewModels;

namespace SAM.ViewModels
{
    public class MainWindowViewModel : MainWindowViewModelBase
    {
        public virtual HomeViewModel HomeVm { get; set; }

        protected MainWindowViewModel()
        {
            HomeVm = HomeViewModel.Create();
        }

        public static MainWindowViewModel Create()
        {
            return ViewModelSource.Create(() => new MainWindowViewModel());
        }
    }
}