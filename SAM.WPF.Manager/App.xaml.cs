using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ControlzEx.Theming;
using log4net;
using SAM.WPF.Core;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Manager.ViewModels;
using SAM.WPF.Manager.Views;

namespace SAM.WPF.Manager
{
    public partial class App
    {

        private readonly ILog log = LogManager.GetLogger(nameof(App));

        private void App_OnStartup(object sender, StartupEventArgs startupArgs)
        {
            try
            {
                var commandLineArgs = Environment.GetCommandLineArgs();
                if (commandLineArgs.Length < 2)
                {
                    //if (!Process.GetProcessesByName("SAM.WPF.exe").Any())
                    //{
                    //    Process.Start("SAM.WPF.exe");
                    //}

                    throw new ArgumentException(nameof(commandLineArgs));
                }

                if (!uint.TryParse(commandLineArgs[1], out var appId))
                {
                    var message = $"Failed to parse the {nameof(appId)} from command line argument {commandLineArgs[1]}.";
                    throw new ArgumentException(message, nameof(StartupEventArgs));
                }
                
                IsolatedStorageManager.Init();

                ThemeHelper.SetTheme();
                
                SplashScreenHelper.Show("Loading game info...");

                SteamClientManager.Init(appId);

                var supportedApp = SAMLibraryHelper.GetApp(appId);
                var appInfo = new SteamApp(supportedApp);
                
                appInfo.LoadClientInfo();

                SplashScreenHelper.SetStatus(appInfo.Name);

                var gameVm = SteamGameViewModel.Create(appInfo);
                gameVm.RefreshStats();

                var gameView = new SteamGameView
                {
                    DataContext = gameVm
                };

                MainWindow = new MainWindow();
                MainWindow.Content = gameView;
                MainWindow.Title = $"Steam Achievement Manager | {appInfo.Name}";
                MainWindow.Icon = appInfo.Icon.ToImageSource();

                MainWindow.Show();

                SplashScreenHelper.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Application Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
