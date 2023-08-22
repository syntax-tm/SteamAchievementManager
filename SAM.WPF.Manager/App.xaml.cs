using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SAM.WPF.Core;
using SAM.WPF.Core.API;
using SAM.WPF.Core.Extensions;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Core.ViewModels;
using SAM.WPF.Core.Views;

namespace SAM.WPF.Manager
{
    public partial class App
    {
        private static uint? _appID;

        protected override void App_OnStartup(object sender, StartupEventArgs startupArgs)
        {
            try
            {
                base.App_OnStartup(sender, startupArgs);

                var commandLineArgs = Environment.GetCommandLineArgs();
                if (commandLineArgs.Length < 2)
                {
                    if (!SAMHelper.IsPickerRunning())
                    {
                        log.Warn(@"The SAM picker process is not running. Starting picker application...");

                        SAMHelper.OpenPicker();
                    }

                    log.Fatal(@"No app ID argument was supplied. Application will now exit...");

                    Environment.Exit(SAMExitCode.NoAppIdArgument);
                }

                if (!uint.TryParse(commandLineArgs[1], out var appId))
                {
                    var message = $"Failed to parse the {nameof(appId)} from command line argument {commandLineArgs[1]}.";
                    throw new ArgumentException(message, nameof(startupArgs));
                }

                _appID = appId;
                
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

                MainWindow = new MainWindow
                {
                    Content = gameView,
                    DataContext = gameVm,
                    Title = $"Steam Achievement Manager | {appInfo.Name}",
                    Icon = appInfo.Icon?.ToImageSource()
                };

                MainWindow.Show();
            }
            catch (Exception e)
            {
                var message = $"An error occurred during SAM Manager application startup. {e.Message}";

                log.Fatal(message, e);

                MessageBox.Show(message, "Application Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(SAMExitCode.UnhandledException);
            }
            finally
            {
                SplashScreenHelper.Close();
            }
        }

        protected override void App_OnExit(object sender, ExitEventArgs args)
        {
            try
            {
                base.App_OnExit(sender, args);
                
                log.Info(@$"SAM manager ({_appID ?? 0}) is exiting.");
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred attempting to exit the SAM Manager. {e.Message}", e);
            }
        }
        
    }
}
