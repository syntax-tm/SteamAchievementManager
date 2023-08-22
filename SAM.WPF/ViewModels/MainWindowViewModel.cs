using System.Reflection;
using DevExpress.Mvvm.POCO;
using SAM.WPF.Core.ViewModels;

namespace SAM.WPF.ViewModels
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