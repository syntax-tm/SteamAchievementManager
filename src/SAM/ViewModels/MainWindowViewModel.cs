using DevExpress.Mvvm.CodeGenerators;
using SAM.Core;
using SAM.Core.ViewModels;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModel : MainWindowViewModelBase
{
	[GenerateProperty] private SteamUser user;
	[GenerateProperty] private HomeViewModel homeVm;

	public MainWindowViewModel ()
	{
		User = new();
		HomeVm = new();
	}
}
