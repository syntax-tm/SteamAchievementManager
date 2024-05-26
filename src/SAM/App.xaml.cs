using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommandLine;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Core.Logging;
using SAM.ViewModels;
using SAM.SplashScreen;
using SteamGameViewModel = SAM.ViewModels.SteamGameViewModel;

namespace SAM;

public partial class App
{
    protected readonly ILog log = LogManager.GetLogger(nameof(App));

    protected override void OnStartup(StartupEventArgs args)
    {
        base.OnStartup(args);

        try
        {
            GlobalContext.Properties[AssemblyVersionHelper.KEY] = new AssemblyVersionHelper();
            GlobalContext.Properties[EntryAssemblyHelper.KEY] = new EntryAssemblyHelper();
            GlobalContext.Properties[SteamAppContextHelper.KEY] = new SteamAppContextHelper();

            log.Info("Application startup.");

            SplashScreenHelper.Show("Starting up...");

            SAMHelper.VerifySteamProcess();

            // handle any WPF dispatcher exceptions
            Current.DispatcherUnhandledException += OnDispatcherException;

            // handle any AppDomain exceptions
            var current = AppDomain.CurrentDomain;
            current.UnhandledException += OnAppDomainException;

            // handle any TaskScheduler exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            var helpWriter = new StringWriter();
            var parser = new Parser(with =>
            {
                //ignore case for enum values
                with.CaseInsensitiveEnumValues = true;
                with.HelpWriter = helpWriter;
            });
            
            var options = parser.ParseArguments<SAMOptions>(args.Args);

            HandleOptions(options.Value);
        }
        catch (Exception e)
        {
            var message = $"An error occurred on application startup. {e.Message}";

            log.Error(message, e);

            MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(SAMExitCode.UnhandledException);
        }
    }

    protected override void OnExit(ExitEventArgs args)
    {
        base.OnExit(args);

        // TODO: with there being only one executable now need to differentiate the process types
        //try
        //{
        //    log.Info(@"Application exiting. Ending any running manager processes...");

        //    SAMHelper.CloseAllManagers();
        //}
        //catch (Exception e)
        //{
        //    log.Fatal($"An error occurred attempting to exit the SAM Managers. {e.Message}", e);
        //}
    }

    private void HandleOptions(SAMOptions options)
    {
        var appId = options.AppId;
        var isApp = appId != 0;

        if (isApp)
        {
            SteamClientManager.Init(appId);

            if (!SteamClientManager.Default.OwnsGame(appId))
            {
                throw new SAMInitializationException($"The current Steam account does not have a license for app '{appId}'.");
            }

            var appInfo = new SteamApp(appId);
            
            SplashScreenHelper.SetStatus(appInfo.Name);

            var gameVm = new SteamGameViewModel(appInfo);
            gameVm.RefreshStats();

            var mainWindowVm = new MainWindowViewModel(gameVm)
            {
                SubTitle = appInfo.Name
            };

            MainWindow = new MainWindow
            {
                DataContext = mainWindowVm
            };
        }
        else
        {
            // create the default Client instance
            SteamClientManager.Init(0);

            SteamLibraryManager.Init();

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        MainWindow.Show();

        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    // ReSharper disable once InconsistentNaming
    private void DisplayHelp(IEnumerable<Error> err, TextWriter helpWriter)
    {
        var errors = err.ToList();

        if (errors.IsVersion() || errors.IsHelp())
        {
            Console.WriteLine(helpWriter.ToString());
        }
        else
        {
            Console.Error.WriteLine(helpWriter.ToString());
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        try
        {
            var exception = args.Exception;
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var message = $"An unobserved task exception occurred. {exception.Message}";

            log.Error(message, args.Exception);

            MessageBox.Show(message, $"Unhandled ${exception.GetType().Name}", MessageBoxButton.OK, MessageBoxImage.Error);
                    
            args.SetObserved();
        }
        catch (Exception e)
        {
            log.Fatal($"An error occurred in {nameof(OnUnobservedTaskException)}. {e.Message}", e);

            Environment.Exit(SAMExitCode.TaskException);
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

            log.Fatal(message, args.Exception);

            Environment.Exit(SAMExitCode.DispatcherException);
        }
        catch (Exception e)
        {
            var message = $"An error occurred in {nameof(OnDispatcherException)}. {e.Message}";

            Environment.FailFast(message);
        }
    }
}
