﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;
using SAM.Core;

namespace SAM
{
    public partial class App
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(App));

        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);

            WalkDictionary(Resources);

            try
            {
                log.Info("Application startup.");
                
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

                ThemeHelper.SetTheme(this);

                // create the default Client instance
                SteamClientManager.Init(0);

                SteamLibraryManager.Init();
                
                MainWindow = new MainWindow();
                MainWindow.Show();

                SplashScreenHelper.Close();
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

            try
            {
                log.Info(@"Application exiting. Ending any running manager processes...");

                SAMHelper.CloseAllManagers();
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred attempting to exit the SAM Managers. {e.Message}", e);
            }
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