using MahApps.Metro.IconPacks;
using Wpf.Ui.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace SAM.Core
{
    public static class SplashScreenHelper
    {
        private static bool _isInitialized;
        private static UiWindow _splashWindow;
        private static SplashScreenViewModel _splashScreenVm;
        
        public static void Init()
        {
            if (_isInitialized) throw new SAMException();
            
            _splashScreenVm = SplashScreenViewModel.Create();

            _splashWindow = new SplashScreenView();
            _splashWindow.DataContext = _splashScreenVm;
            
            var icon = PackIconHelper.GetImageSource(PackIconMaterialKind.Steam, Brushes.White);
            _splashWindow.Icon = icon;
            
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

            if (_splashWindow.IsVisible) return;

            _splashWindow.Show();
        }

        public static void Close()
        {
            _splashWindow.Close();
        }
    }
}
