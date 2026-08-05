using CommandLine;
using log4net;
using SAM.Core;
using SAM.Core.Logging;
using SAM.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace SAM;

public partial class Program
{
    private static readonly ILog log = LogManager.GetLogger(nameof(Program));

    private static App _app;

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            GlobalContext.Properties[AssemblyVersionHelper.KEY] = new AssemblyVersionHelper();
            GlobalContext.Properties[EntryAssemblyHelper.KEY] = new EntryAssemblyHelper();
            GlobalContext.Properties[SteamAppContextHelper.KEY] = new SteamAppContextHelper();

            SAMHelper.VerifySteamProcess();

            //var isSteamRunning = SAMHelper.IsSteamRunning();

            //if (!isSteamRunning)
            //{
            //    var message = $"The Steam client is not currently running. Would you like start Steam now?";
            //    var title = $"Steam Not Running";
            //    var response = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            //    if (response == MessageBoxResult.Yes)
            //    {
            //        SAMHelper.LaunchSteamClient();
            //    }
            //}

            IActivatedEventArgs activatedEventArgs = null;

            try
            {
                activatedEventArgs = AppInstance.GetActivatedEventArgs();
            }
            catch
            {
                log.Info($"Application is not running in {nameof(AppInstance)}.");
            }

            _app = new App();
            _app.InitializeComponent();

            if (activatedEventArgs != null)
            {
                log.Info($"Using {nameof(activatedEventArgs)} to configure {nameof(App)}...");

                ApplicationContext.Default.IsUwpApp = true;

                _app.OnProtocolActivated(activatedEventArgs);
            }
            else
            {
                log.Info($"Current {nameof(AppInstance)} does not exist. Using {nameof(IActivatedEventArgs)}. Falling back to {nameof(args)} to configure {nameof(App)}...");
                log.Debug($"{nameof(args)}:");

                var i = 0;

                foreach (var arg in args)
                {
                    log.Debug($"[{++i}]: {arg}");
                }

                var currentAssembly = Assembly.GetExecutingAssembly();
                var optionTypes = currentAssembly.GetImplementingTypes<IOptionUri>();

                var helpWriter = new StringWriter();
                var parser = new Parser(with =>
                {
                    with.CaseSensitive = false;
                    with.CaseInsensitiveEnumValues = true;
                    with.HelpWriter = helpWriter;
                });

                var options = parser.ParseArguments(args, optionTypes);

                if (options.Value is IOptionUri uriOption)
                {
                    var uri = uriOption.ToUri();
                    _app.ProcessUri(uri);
                }
            }

            _app.Run();
        }
        catch (Exception e)
        {
            var message = $"An error occurred on application startup. {e.Message}";

            log.Error(message, e);

            MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(SAMExitCode.UnhandledException);
        }
    }
}
