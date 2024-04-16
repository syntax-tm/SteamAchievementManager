using DevExpress.Mvvm.CodeGenerators;
using SAM.Core;
using SAM.Core.ViewModels;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModel : MainWindowViewModelBase
{
    [GenerateProperty] private SteamUser _user;
    [GenerateProperty] private HomeViewModel _homeVm;

    public MainWindowViewModel()
    {
        User = new ();
        HomeVm = new ();
    }
}
