using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ControlzEx.Theming;
using FontAwesome.WPF;
using MahApps.Metro.Controls;

namespace SAM.WPF.Core.SplashScreen
{
    public static class SplashScreenHelper
    {

        private static bool _isInitialized;
        private static SplashScreenViewModel SplashScreenVm { get; set; }
        private static MetroWindow _splashWindow;
        private static Thread _splashWindowThread;
        
        public static void Init()
        {
            if (_isInitialized) throw new ConfigurationErrorsException();
            
            SplashScreenVm = SplashScreenViewModel.Create();

            _splashWindowThread = new Thread(ThreadStartingPoint);
            _splashWindowThread.SetApartmentState(ApartmentState.STA);
            _splashWindowThread.IsBackground = true;
            
            _isInitialized = true;
        }

        public static void SetStatus(string status = null)
        {
            SplashScreenVm.Status = status;
        }

        public static void Show(string status = null)
        {
            if (!_isInitialized)
            {
                Init();
            }

            SplashScreenVm.Status = status;
            
            _splashWindowThread.Start();
        }

        public static void Close()
        {
            _splashWindow.Dispatcher.BeginInvoke(() =>
            {
                Thread.Sleep(new TimeSpan(0, 0, 0, 1, 500));

                _splashWindow.Close();
            });
        }

        private static void ThreadStartingPoint()
        {
            _splashWindow = new MetroWindow();
            _splashWindow.DataContext = SplashScreenVm;

            _splashWindow.Title = "Steam Achievement Manager";
            _splashWindow.WindowStyle = WindowStyle.None;
            _splashWindow.AllowsTransparency = true;
            _splashWindow.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.Steam, Brushes.White);
            _splashWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _splashWindow.SizeToContent = SizeToContent.WidthAndHeight;
            _splashWindow.ShowIconOnTitleBar = false;
            _splashWindow.ShowTitleBar = false;
            _splashWindow.ShowSystemMenu = false;
            _splashWindow.UseNoneWindowStyle = true;
            _splashWindow.GlowBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            _splashWindow.NonActiveGlowBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
            _splashWindow.BorderThickness = new Thickness(1);
            
            var splashView = new SplashScreenView();
            _splashWindow.Content = splashView;

            _splashWindow.Show();

            Dispatcher.Run();
        }

    }
}
