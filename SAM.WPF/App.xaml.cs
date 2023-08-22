using System;
using System.Windows;
using SAM.WPF.Core;
using SAM.WPF.Core.API;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Core.Themes;

namespace SAM.WPF
{
    public partial class App
    {
        protected override void App_OnStartup(object sender, StartupEventArgs args)
        {
            try
            {
                base.App_OnStartup(sender, args);

                IsolatedStorageManager.Init();

                SplashScreenHelper.SetStatus("Applying theme...");

                ThemeHelper.SetTheme(this);

                SplashScreenHelper.SetStatus("Initializing Steam Client...");

                // create the default Client instance
                SteamClientManager.Init(0);

                SplashScreenHelper.SetStatus("Initializing library...");

                SteamLibraryManager.Init();
                
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            catch (Exception e)
            {
                var message = $"An error occurred on application startup. {e.Message}";

                log.Error(message, e);

                MessageBox.Show(message, @"SAM Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);

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

                log.Info(@"Application exiting. Ending any running manager processes...");

                SAMHelper.CloseAllManagers();
            }
            catch (Exception e)
            {
                log.Fatal($"An error occurred attempting to exit the SAM Managers. {e.Message}", e);
            }
        }
    }
}
