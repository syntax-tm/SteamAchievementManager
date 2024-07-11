using System.Windows;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Behaviors;
using SAM.SplashScreen;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModel
{
    private const string TITLE_BASE = "Steam Achievement Manager";

    private readonly ILog log = LogManager.GetLogger(typeof(MainWindowViewModel));

    [GenerateProperty] private string title = TITLE_BASE;
    [GenerateProperty] private string subTitle;
    [GenerateProperty] private bool _isManager;
    [GenerateProperty] private bool _isLibrary;
    [GenerateProperty] private ApplicationMode _mode;
    [GenerateProperty] private SteamUser _user;
    [GenerateProperty] private HomeViewModel _homeVm;
    [GenerateProperty] private SteamGameViewModel gameVm;
    [GenerateProperty] private MenuViewModel _menuVm;
    [GenerateProperty] private WindowSettings config;
    [GenerateProperty] private object _currentVm;

    public MainWindowViewModel()
    {
        //User = new (SteamClientManager.Default);
        HomeVm = new ();

        CurrentVm = HomeVm;
        MenuVm = new (HomeVm);

        Init();
    }
    
    public MainWindowViewModel(HomeSettings settings)
    {
        //User = new (SteamClientManager.Default);
        HomeVm = new (settings);

        CurrentVm = HomeVm;
        MenuVm = new (HomeVm);

        Init();
    }

    public MainWindowViewModel(SteamGameViewModel gameVm)
    {
        //User = new (SteamClientManager.Default);
        GameVm = gameVm;

        CurrentVm = GameVm;
        MenuVm = new (GameVm);

        Init();
    }
    
    [GenerateCommand]
    protected void OnLoaded()
    {
        SplashScreenHelper.Close();

        // activate the main window after closing the splash screen and shutting its dispatcher down
        Application.Current.MainWindow?.Activate();
    }

    private void Init()
    {
        Mode = GameVm != null ? ApplicationMode.Manager : ApplicationMode.Default;
        IsManager = Mode == ApplicationMode.Manager;
        IsLibrary = Mode == ApplicationMode.Default;
    }

    private void OnSubTitleChanged()
    {
        if (string.IsNullOrWhiteSpace(SubTitle))
        {
            Title = TITLE_BASE;
            return;
        }

        Title = $"{TITLE_BASE} | {SubTitle}";
    }
}
