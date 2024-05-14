using System.Threading;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace SAM.Core
{
    public static class SplashScreenHelper
    {
        private static Thread _thread;
        private static volatile bool _isInitialized;
        private static volatile FluentWindow _splashWindow;
        private static volatile SplashScreenViewModel _splashScreenVm;
        
        public static void Init(string status = null)
        {
            if (_isInitialized) throw new SAMException();

            var pts = new ParameterizedThreadStart(ThreadStartingPoint);

            _thread = new (pts);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = true;
            _thread.Start(status);

            _isInitialized = true;
        }

        public static void SetStatus(string status = null)
        {
            _splashScreenVm.Status = status;
        }

        private static void ThreadStartingPoint(object arg = null)
        {
            _splashScreenVm = SplashScreenViewModel.Create();
            _splashScreenVm.Status = arg?.ToString();

            _splashWindow = new SplashScreenView();
            _splashWindow.DataContext = _splashScreenVm;

            _splashWindow.Show();

            Dispatcher.Run();
        }

        public static void Show(string status = null)
        {
            if (!_isInitialized)
            {
                Init(status);

                return;
            }

            _splashScreenVm.Status = status;
        }

        public static void Close()
        {
            _splashWindow.Dispatcher.BeginInvoke(() =>
            {
                _splashWindow.Close();
            }, DispatcherPriority.Background);
        }
    }
}
