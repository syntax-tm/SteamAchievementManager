using DevExpress.Mvvm.CodeGenerators;
using SAM.Core.ViewModels;

namespace SAM.Manager.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModel : MainWindowViewModelBase
{
    [GenerateProperty] private SteamGameViewModel gameVm;

    public MainWindowViewModel(SteamGameViewModel gameVm)
    {
        GameVm = gameVm;
    }
}
