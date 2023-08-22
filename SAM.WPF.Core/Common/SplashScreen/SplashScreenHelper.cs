using System;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace SAM.WPF.Core.SplashScreen
{
    public static class SplashScreenHelper
    {

        private static bool _isInitialized;
        private static Thread _splashWindowThread;
        private static MetroWindow _splashWindow;
        private static SplashScreenViewModel _splashScreenVm;
        
        public static void Init()
        {
            if (_isInitialized) throw new SAMException();
            
            _splashScreenVm = SplashScreenViewModel.Create();

            _splashWindowThread = new (ThreadStartingPoint);
            _splashWindowThread.SetApartmentState(ApartmentState.STA);
            _splashWindowThread.IsBackground = true;
            
            _isInitialized = true;
        }

        public static void SetStatus(string status = null)
        {
            _splashScreenVm.Status = status;
        }

        public static void Show(string status = null)
        {
            if (!_isInitialized)
            {
                Init();
            }

            _splashScreenVm.Status = status;
            
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
            _splashWindow.DataContext = _splashScreenVm;
            
            var icon = PackIconHelper.GetImageSource(PackIconMaterialKind.Steam, Brushes.White);
            
            _splashWindow.AllowsTransparency = false;
            _splashWindow.Icon = icon;
            _splashWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _splashWindow.SizeToContent = SizeToContent.WidthAndHeight;
            _splashWindow.ShowIconOnTitleBar = false;
            _splashWindow.ShowTitleBar = false;
            _splashWindow.ShowSystemMenu = false;
            _splashWindow.ShowSystemMenuOnRightClick = false;
            _splashWindow.ShowMinButton = true;
            _splashWindow.ShowMaxRestoreButton = false;
            _splashWindow.ShowCloseButton = true;
            _splashWindow.ShowInTaskbar = true;
            _splashWindow.GlowBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
            _splashWindow.NonActiveGlowBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
            _splashWindow.BorderThickness = new Thickness(2);
            
            _splashWindow.Content = new SplashScreenView();

            _splashWindow.Show();

            Dispatcher.Run();
        }

    }
}
