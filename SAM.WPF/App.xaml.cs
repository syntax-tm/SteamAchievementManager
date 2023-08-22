using System;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using log4net;
using SAM.WPF.Core.SplashScreen;

namespace SAM.WPF
{
    public partial class App
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(App));

        private void App_OnStartup(object sender, StartupEventArgs args)
        {
            try
            {
                SplashScreenHelper.Init();
                SplashScreenHelper.Show();

                MainWindow = new MainWindow();
                MainWindow.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.Steam, Brushes.Black);
                MainWindow.Show();

                SplashScreenHelper.Close();
            }
            catch (Exception e)
            {
                log.Error($"An error occurred on application startup. {e.Message}", e);

                throw;
            }
        }
    }
}
