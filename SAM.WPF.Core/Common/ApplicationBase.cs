using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Core.Themes;

namespace SAM.WPF.Core
{
    public abstract class ApplicationBase : Application
    {

        protected readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(ApplicationBase));
        
        protected ApplicationBase()
        {
            log.Info("Application startup.");

            Startup += App_OnStartup;
            Exit += App_OnExit;
        }

        protected virtual void App_OnStartup(object sender, StartupEventArgs args)
        {
            try
            {
                SplashScreenHelper.Show("Starting up...");

                SAMHelper.VerifySteamProcess();

                //  handle any WPF dispatcher exceptions
                Current.DispatcherUnhandledException += OnDispatcherException;

                //  handle any AppDomain exceptions
                var current = AppDomain.CurrentDomain;
                current.UnhandledException += OnAppDomainException;

                //  handle any TaskScheduler exceptions
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
                
                IsolatedStorageManager.Init();

                SplashScreenHelper.SetStatus("Applying theme...");

                ThemeHelper.SetTheme(this);
            }
            catch (SAMInitializationException ie)
            {
                log.Fatal(ie);

                MessageBox.Show(ie.Message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(SAMExitCode.SteamNotRunning);
            }
            catch (Exception e)
            {
                var message = $"An error occurred on application startup. {e.Message}";

                log.Fatal(message, e);

                MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(SAMExitCode.UnhandledException);
            }
        }
        
        protected virtual void App_OnExit(object sender, ExitEventArgs args)
        {

        }

        protected virtual void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
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

        protected virtual void OnAppDomainException(object sender, UnhandledExceptionEventArgs args)
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

        protected virtual void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs args)
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
