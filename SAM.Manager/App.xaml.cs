using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Core.ViewModels;
using SAM.Core.Views;
using SAM.Manager.ViewModels;

namespace SAM.Manager
{
    public partial class App
    {
        private readonly ILog log = LogManager.GetLogger(nameof(App));

        private static uint? _appID;

        [STAThread]
        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);
            
            WalkDictionary(Resources);

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

                    Environment.Exit(SAMExitCode.NoAppIdArgument);
                }

                if (!uint.TryParse(commandLineArgs[1], out var appId))
                {
                    var message = $"Failed to parse the {nameof(appId)} from command line argument {commandLineArgs[1]}.";
                    throw new ArgumentException(message, nameof(args));
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
                
                // TODO: move this to the MainWindowViewModel via passing the app id
                var supportedApp = SAMLibraryHelper.GetApp(appId);
                var appInfo = SteamApp.Create(supportedApp);
                
                appInfo.LoadClientInfo();

                SplashScreenHelper.SetStatus(appInfo.Name);

                var gameVm = SteamGameViewModel.Create(appInfo);
                gameVm.RefreshStats();

                var mainWindowVm = MainWindowViewModel.Create(gameVm);
                
                MainWindow = new MainWindow
                {
                    DataContext = mainWindowVm,
                    Title = $"Steam Achievement Manager | {appInfo.Name}",
                    Icon = appInfo.Icon?.ToImageSource()
                };

                MainWindow.Show();

                SplashScreenHelper.Close();
            }
            catch (Exception e)
            {
                var message = $"An error occurred during SAM Manager application startup. {e.Message}";

                log.Fatal(message, e);

                MessageBox.Show(message, "Application Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(SAMExitCode.UnhandledException);
            }
        }

        protected override void OnExit(ExitEventArgs args)
        {
            base.OnExit(args);

            log.Info(@$"SAM manager for app {_appID} is exiting.");
        }
        
        private static void WalkDictionary(ResourceDictionary resources)
        {
            foreach (var _ in resources) { }
            foreach (var rd in resources.MergedDictionaries)
            {
                WalkDictionary(rd);
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
                Environment.Exit(SAMExitCode.AppDomainException);
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

                Environment.Exit(SAMExitCode.DispatcherException);
            }
        }
    }
}
