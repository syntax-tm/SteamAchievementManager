using CommandLine;
using CommandLine.Text;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Managers;
using SAM.SplashScreen;
using SAM.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.ApplicationModel.Activation;
using Windows.Win32;
using Windows.Win32.System.Console;
using PipeStream = SAM.Serialization.PipeStream;

namespace SAM;

public partial class App
{
    private const string PIPE_SERVER_NAME = @".";

    protected readonly ILog log = LogManager.GetLogger(nameof(App));

    private readonly Parser parser;

    public uint AppId { get; set; }
    public ApplicationMode Mode { get; set; }
    public Mutex Mutex { get; set; }
    public string Key { get; set; }

    public App()
    {
        var helpWriter = new StringWriter();
        parser = new Parser(with =>
        {
            with.CaseSensitive = false;
            //ignore case for enum values
            with.CaseInsensitiveEnumValues = true;
            with.HelpWriter = helpWriter;
        });
    }

    protected override void OnStartup(StartupEventArgs startupArgs)
    {
        base.OnStartup(startupArgs);

        try
        {
            log.Info("Application startup.");

            // handle any WPF dispatcher exceptions
            Current.DispatcherUnhandledException += OnDispatcherException;

            // handle any AppDomain exceptions
            var current = AppDomain.CurrentDomain;
            current.UnhandledException += OnAppDomainException;

            // handle any TaskScheduler exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }
        catch (Exception e)
        {
            var message = $"An error occurred on application startup. {e.Message}";

            log.Error(message, e);

            MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(SAMExitCode.UnhandledException);
        }
    }

    public void ProcessUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        try
        {
            if (!AppUriHelper.TryParseUri(uri.ToString(), out var args))
            {
                throw new SAMInitializationException($"Invalid {nameof(Uri)} '{uri}'.");
            }

            var parsed = parser.ParseArguments<SelectOptions, ManageOptions>(args);
            var option = parsed.Value as IOptionUri;

            log.Debug($"Argument {nameof(Parser)} returned {option?.Mode} options");

            var result = parsed.MapResult(
                                           (SelectOptions o) => HandleSelect(o),
                                           (ManageOptions o) => HandleManage(o),
                                           err => HandleError(parsed, err)
                                          );

            log.Debug($"{nameof(option)} returned code {result}");

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
    private static void DisplayHelp(IEnumerable<Error> err)
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

            if (options.LockAll)
            {
                gameVm.LockAllAchievements();

                gameVm.Save(false);

                return 0;
            }

            SplashScreenHelper.Show(appInfo.Name);

            var mainWindowVm = new MainWindowViewModel(gameVm)
            {
                SubTitle = appInfo.Name,
                Mode = Mode
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

    private static int HandleError(ParserResult<object> options, IEnumerable<Error> errors)
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

    public void SendPipeMessage(ApplicationMessage message)
    {
        try
        {
            // TODO: make server name a constant
            using var pipeClient = new NamedPipeClientStream(PIPE_SERVER_NAME, Key, PipeDirection.Out);

            // TODO: make timeout a constant
            pipeClient.Connect(3000);

            var pipeStream = new PipeStream(pipeClient);
            pipeStream.SendMessage(message);
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to sent a message using the named pipe '{Key}'. {e.Message}", e);
            throw;
        }
    }

    public bool CreateMutex(uint? appId = null)
    {
        if (appId <= 0) throw new ArgumentException($"Invalid {nameof(appId)} ('{appId}').");

        var isManager = appId is > 0;
        var mode = isManager ? ApplicationMode.Manager : ApplicationMode.Default;
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name;

        Key = $"{assemblyName}_{mode:G}";

        if (appId is > 0)
        {
            Key += $"_{appId}";
        }

        Mutex = new Mutex(true, Key, out var createdNew);

        //Exit += CloseMutexHandler;

        return createdNew;
    }

    //private void CloseMutexHandler(object sender, ExitEventArgs e)
    //{
    //    Mutex?.Dispose();
    //    Mutex = null;
    //}

    protected override void OnExit(ExitEventArgs args)
    {
        base.OnExit(args);

        Mutex?.Dispose();
        Mutex = null;

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
            var exception = args?.Exception;
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

        if (activatedEventArgs.Kind == ActivationKind.CommandLineLaunch)
        {
            var cmdArgs = activatedEventArgs as ICommandLineActivatedEventArgs;
            var operation = cmdArgs?.Operation;
            var operationArgs = operation?.Arguments;
            string[] args = null;

            if (!string.IsNullOrEmpty(operationArgs))
            {
                args = operationArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            Init(args);

            return;
        }

        if (activatedEventArgs.Kind == ActivationKind.Launch)
        {
            var launchArgs = activatedEventArgs as ILaunchActivatedEventArgs;
            string[] args = null;

            if (!string.IsNullOrEmpty(launchArgs?.Arguments))
            {
                args = launchArgs.Arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            Init(args);

            return;
        }
        
        if (activatedEventArgs.Kind == ActivationKind.Protocol)
        {
            var protocolArgs = activatedEventArgs as IProtocolActivatedEventArgs;
            
            Debug.Assert(protocolArgs != null);

            var uri = protocolArgs.Uri;

            Init(uri);

            return;
        }

        // default if activated kind is not listed above
        log.Info($"{nameof(App)} activated kind '{activatedEventArgs.Kind:G}' is not explicitly handled. Using defaults...");

        string[] defaultArgs = [ string.Empty ];

        Init(defaultArgs);
    }

    private void Init(string[] args)
    {
        args ??= [];

        var isSingleInstance = false;
        var options = parser.ParseArguments<SelectOptions, ManageOptions>(args);

        if (options.Value is SelectOptions selectOptions)
        {
            Mode = ApplicationMode.Default;

            isSingleInstance = CreateMutex();
        }
        else if (options.Value is ManageOptions manageOptions)
        {
            Mode = ApplicationMode.Manager;

            isSingleInstance = CreateMutex(manageOptions.AppId);
        }

        // if this is not the only instance then 
        if (!isSingleInstance)
        {
            SendPipeMessage(ApplicationMessage.Activate);

            Environment.Exit(SAMExitCode.AlreadyRunning);
        }

        // if this is the only instance, start listening for messages on the named pipe
        var pipeThread = new Thread(CreateNamedPipeServer) { IsBackground = true };
        pipeThread.Start();

        var result = options.MapResult(
                                       (SelectOptions o) => HandleSelect(o),
                                       (ManageOptions o) => HandleManage(o),
                                       err => HandleError(options, err)
                                      );
    }

    private void Init(Uri uri)
    {
        var isSingleInstance = false;
        var args = AppUriHelper.ParseUri(uri);
        var options = parser.ParseArguments<SelectOptions, ManageOptions>(args);

        if (options.Value is SelectOptions selectOptions)
        {
            Mode = ApplicationMode.Default;

            isSingleInstance = CreateMutex();
        }
        else if (options.Value is ManageOptions manageOptions)
        {
            Mode = ApplicationMode.Manager;

            isSingleInstance = CreateMutex(manageOptions.AppId);
        }

        // if this is not the only instance then 
        if (!isSingleInstance)
        {
            SendPipeMessage(ApplicationMessage.Activate);

            Environment.Exit(SAMExitCode.AlreadyRunning);
        }

        // if this is the only instance, start listening for messages on the named pipe
        var pipeThread = new Thread(CreateNamedPipeServer) { IsBackground = true };
        pipeThread.Start();

        var result = options.MapResult(
                                       (SelectOptions o) => HandleSelect(o),
                                       (ManageOptions o) => HandleManage(o),
                                       err => HandleError(options, err)
                                      );
    }

    public void ProcessMessage(ApplicationMessage message)
    {
        switch (message)
        {
            case ApplicationMessage.Activate:
                MainWindow?.Activate();
                break;
            case ApplicationMessage.Exit:
                Environment.Exit(SAMExitCode.ExitRequested);
                break;
            default:
                log.Warn($"Received unknown {nameof(ApplicationMessage)} {message:G}. Ignoring...");
                break;
        }
    }

    //[SuppressMessage("ReSharper", "FunctionNeverReturns")]
    private void CreateNamedPipeServer()
    {
        using var pipeServer = new NamedPipeServerStream(Key, PipeDirection.In, 1, PipeTransmissionMode.Message);

        while (true)
        {
            pipeServer.WaitForConnection();

            do
            {
                var reader = new PipeStream(pipeServer);
                var message = reader.ReadMessage();

                Dispatcher.BeginInvoke(new Action(() => ProcessMessage(message)));
            }
            while (!pipeServer.IsMessageComplete);

            pipeServer.Disconnect();
        }
    }
}
