using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;
using SAM.WPF.Core;
using SAM.WPF.Core.API;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Core.Themes;
using SAM.WPF.Manager.ViewModels;
using SAM.WPF.Manager.Views;

namespace SAM.WPF.Manager
{
    public partial class App
    {
        private readonly ILog log = LogManager.GetLogger(nameof(App));

        private static uint? _appID;

        private void App_OnStartup(object sender, StartupEventArgs startupArgs)
        {
            try
            {
                var commandLineArgs = Environment.GetCommandLineArgs();
                if (commandLineArgs.Length < 2)
                {
                    if (!SAMHelper.IsPickerRunning())
                    {
                        log.Warn(@"The SAM picker process is not running. Starting picker application...");

                        SAMHelper.OpenPicker();
                    }
                    
                    log.Fatal(@"No app ID argument was supplied. Application will now exit...");

                    Environment.Exit((int) SAMExitCode.NoAppIdArgument);
                }

                if (!uint.TryParse(commandLineArgs[1], out var appId))
                {
                    var message = $"Failed to parse the {nameof(appId)} from command line argument {commandLineArgs[1]}.";
                    throw new ArgumentException(message, nameof(StartupEventArgs));
                }

                _appID = appId;
                
                //  handle any WPF dispatcher exceptions
                Current.DispatcherUnhandledException += OnDispatcherException;

                //  handle any AppDomain exceptions
                var current = AppDomain.CurrentDomain;
                current.UnhandledException += OnAppDomainException;
                
                //  handle any TaskScheduler exceptions
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

                IsolatedStorageManager.Init();

                ThemeHelper.SetTheme(this);
                
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
                var message = $"An error occurred during SAM Manager application startup. {e.Message}";

                log.Fatal(message, e);

                MessageBox.Show(message, "Application Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit((int) SAMExitCode.UnhandledException);
            }
        }

        private void App_OnExit(object sender, ExitEventArgs args)
        {
            try
            {
                var appIdDisplay = _appID.HasValue
                    ? $"for app id {_appID}"
                    : "with no app id";

                log.Info(@$"SAM manager {appIdDisplay} is exiting.");
            }
            catch (Exception e)
            {
                log.Error($"An error occurred attempting to exit the SAM Managers. {e.Message}", e);
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            try
            {
                var exception = args.Exception;
                if (exception == null)
                {
                    throw new ArgumentNullException();
                }

                var message = $"An unobserved task exception occurred. {exception.Message}";

                log.Error(message, args.Exception);

                MessageBox.Show(message, $"Unhandled ${exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                args.SetObserved();
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred in {nameof(OnUnobservedTaskException)}. {e.Message}", e);

                Environment.Exit((int) SAMExitCode.UnhandledException);
            }
        }

        private void OnAppDomainException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                var exception = (Exception) args.ExceptionObject;
                var message = $"Dispatcher unhandled exception occurred. {exception.Message}";

                log.Fatal(message, exception);

                MessageBox.Show(message, $"Unhandled ${exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred in {nameof(OnAppDomainException)}. {e.Message}", e);
            }
            finally
            {
                Environment.Exit((int) SAMExitCode.AppDomainException);
            }
        }

        private void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            try
            {
                var message = $"Dispatcher unhandled exception occurred. {args.Exception.Message}";

                log.Error(message, args.Exception);

                MessageBox.Show(message, $"Unhandled ${args.Exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);

                args.Handled = true;
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred in {nameof(OnDispatcherException)}. {e.Message}", e);

                Environment.Exit((int) SAMExitCode.DispatcherException);
            }
        }
    }
}
