using System.Configuration;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;

namespace SAM.WPF.Core.SplashScreen
{
    public static class SplashScreenHelper
    {

        private static bool _isInitialized;
        private static SplashScreenViewModel SplashScreenVm { get; set; }
        private static Window _splashWindow;
        
        public static void Init()
        {
            if (_isInitialized) throw new ConfigurationErrorsException();
            
            SplashScreenVm = SplashScreenViewModel.Create();
            
            _splashWindow = new Window();
            _splashWindow.DataContext = SplashScreenVm;

            _splashWindow.Title = "Steam Achievement Manager";
            _splashWindow.WindowStyle = WindowStyle.None;
            _splashWindow.AllowsTransparency = true;
            _splashWindow.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.Steam, Brushes.Black);
            _splashWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _splashWindow.SizeToContent = SizeToContent.WidthAndHeight;
            
            var splashView = new SplashScreenView();
            _splashWindow.Content = splashView;

            _isInitialized = true;
        }

        public static void SetStatus(string status = null)
        {
            SplashScreenVm.Status = status;
        }

        public static void Show(string status = null)
        {
            SplashScreenVm.Status = status;

            _splashWindow.Show();
        }

        public static void Close()
        {
            _splashWindow.Close();
        }

    }
}
