using CommandLine;
using CommandLine.Text;
using log4net;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using SAM.API;
using SAM.Core;
using SAM.Core.Logging;
using SAM.Managers;
using SAM.SplashScreen;
using SAM.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.ApplicationModel.Activation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;
using SAM.Core.Extensions;
using SteamGameViewModel = SAM.ViewModels.SteamGameViewModel;

namespace SAM;

public partial class App
{
    protected readonly ILog log = LogManager.GetLogger(nameof(App));

    private Mutex _managerMutex;

    protected override void OnStartup(StartupEventArgs startupArgs)
    {
        base.OnStartup(startupArgs);

        try
        {
            GlobalContext.Properties[AssemblyVersionHelper.KEY] = new AssemblyVersionHelper();
            GlobalContext.Properties[EntryAssemblyHelper.KEY] = new EntryAssemblyHelper();
            GlobalContext.Properties[SteamAppContextHelper.KEY] = new SteamAppContextHelper();

            log.Info("Application startup.");

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

            var args = AppUriHelper.ParseArguments(startupArgs.Args);

            var options = parser.ParseArguments<SelectOptions, ManageOptions>(args);

            

            var result = options.MapResult(
                                           (SelectOptions o) => HandleSelect(o),
                                           (ManageOptions o) => HandleManage(o),
                                           err => HandleError(options, err)
                                          );

            log.Debug($"Argument {nameof(parser)} returned {result}");

            if (result != 0)
            {
                Environment.Exit(result);
            }
        }
        catch (Exception e)
        {
            var message = $"An error occurred on application startup. {e.Message}";

            log.Error(message, e);

            MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(SAMExitCode.UnhandledException);
        }
    }
    
    // ReSharper disable once InconsistentNaming
    private void DisplayHelp(IEnumerable<Error> err)
    {
        var errors = err.ToList();

        if (errors.IsVersion() || errors.IsHelp())
        {
            Console.WriteLine();
        }
    }

    private int HandleSelect(SelectOptions options)
    {
        log.Info($"Starting processing {nameof(SelectOptions)}...");

        // ensures that this is the only select process running
        if (!IsSingleInstance())
        {
            var currentProcess = Process.GetCurrentProcess();
            var processName = currentProcess.ProcessName;
            
            log.Fatal($"Another {processName} process is already running.");

            // TODO: need to finish testing this, also needs to handle the non-empty 'select' argument being passed
            //var runningProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
            //var mainProcess = runningProcesses.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.StartInfo.Arguments));
            //mainProcess?.SetActive();

            return SAMExitCode.AlreadyRunning;
        }

        SplashScreenHelper.Show("Starting up...");

        // create the default Client instance
        SteamClientManager.Init(0);

        SteamLibraryManager.Default.Init();

        var settings = HomeSettings.Load();

        if (options.TileView)
        {
            log.Info($"{nameof(options.TileView)} argument detected. Setting {nameof(HomeSettings)} {nameof(HomeSettings.View)} to {LibraryView.Tile:G}");

            settings.View = LibraryView.Tile;
        }
        else if (options.GridView)
        {
            log.Info($"{nameof(options.GridView)} argument detected. Setting {nameof(HomeSettings)} {nameof(HomeSettings.View)} to {LibraryView.DataGrid:G}");

            settings.View = LibraryView.DataGrid;
        }

        MainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(settings)
        };

        MainWindow.Show();

        ShutdownMode = ShutdownMode.OnMainWindowClose;

        return 0;
    }

    private int HandleManage(ManageOptions options)
    {
        try
        {
            log.Info($"Starting processing {nameof(ManageOptions)}...");

            SplashScreenHelper.Show("Starting up...");

            var appId = options.AppId;

            SteamClientManager.Init(appId);

            if (!SteamClientManager.Default.OwnsGame(appId))
            {
                throw new SAMInitializationException($"The current Steam account does not have a license for app '{appId}'.");
            }

            var appInfo = new SteamApp(appId);
            var gameVm = new SteamGameViewModel(appInfo);
            gameVm.RefreshStats();

            if (options.UnlockAll)
            {
                gameVm.UnlockAllAchievements();

                gameVm.Save(false);

                return 0;
            }

            SplashScreenHelper.Show(appInfo.Name);

            var mainWindowVm = new MainWindowViewModel(gameVm)
            {
                SubTitle = appInfo.Name
            };

            MainWindow = new MainWindow
            {
                DataContext = mainWindowVm
            };

            MainWindow.Show();

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            return 0;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to manage app {options?.AppId}. {e.Message}", e);

            return -1;
        }
        finally
        {
            SplashScreenHelper.Close();
        }
    }

    private int HandleError(ParserResult<object> options, IEnumerable<Error> errors)
    {
        // assume that this was launched from command line with invalid arguments
        // and attach to the parent process' console since WPF apps don't have a
        // console otherwise
        var attached = PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);

        var helpText = HelpText.AutoBuild(options);
        var message = helpText.ToString();

        var err = errors.ToList();
        var showHelp = err.IsHelp() || err.IsVersion();

        if (attached)
        {
            var handle = showHelp
                ? PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE)
                : PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_ERROR_HANDLE);

            // TODO: this output formatting needs to be fixed
            PInvoke.WriteConsole(handle, "\n", 1, out _);
            PInvoke.WriteConsole(handle, message, (uint) message.Length, out _);
            PInvoke.WriteConsole(handle, "\n", 1, out _);
        }
        else
        {
            var icon = showHelp ? MessageBoxImage.Information : MessageBoxImage.Error;
            MessageBox.Show(message, "Usage", MessageBoxButton.OK, icon);
        }

        return SAMExitCode.InvalidArguments;
    }

    private bool IsSingleInstance()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var mutexName = assembly.GetName().Name;

        // this is disposed when the process that created it exits
        _managerMutex = new Mutex(true, mutexName, out var createdNew);
        if (!createdNew)
        {
            return false;
        }

        Exit += CloseMutexHandler;
        return true;
    }

    private void CloseMutexHandler(object sender, ExitEventArgs e)
    {
        _managerMutex?.Dispose();
        _managerMutex = null;
    }

    protected override void OnExit(ExitEventArgs args)
    {
        base.OnExit(args);

        // TODO: with there being only one executable need a new way to differentiate the process types
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

    public void OnProtocolActivated(IActivatedEventArgs activatedEventArgs)
    {
        log.Info($"Received {activatedEventArgs.Kind:G} {nameof(IActivatedEventArgs)}");

        if (activatedEventArgs.Kind == ActivationKind.Protocol)
        {
            var protocolArgs = activatedEventArgs as IProtocolActivatedEventArgs;
            
            Debug.Assert(protocolArgs != null);

            var uri = protocolArgs.Uri;

            var args = AppUriHelper.ParseArguments();

            var options = parser.ParseArguments<SelectOptions, ManageOptions>(args);

            MainWindow?.Activate();
            return;
        }
    }
}
