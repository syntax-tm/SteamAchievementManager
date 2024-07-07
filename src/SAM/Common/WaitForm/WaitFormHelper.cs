using System.Threading;
using System.Windows.Threading;
using SAM.Core;
using Wpf.Ui.Controls;

namespace SAM.WaitForm;

public static class WaitFormHelper
{
    private static Thread _thread;
    private static volatile bool _isInitialized;
    private static volatile FluentWindow _waitFormView;
    private static volatile WaitFormViewModel _waitFormVm;
        
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
        _waitFormVm.Status = status;
    }

    private static void ThreadStartingPoint(object arg = null)
    {
        _waitFormVm = WaitFormViewModel.Create();
        _waitFormVm.Status = arg?.ToString();

        _waitFormView = new WaitFormView();
        _waitFormView.DataContext = _waitFormVm;

        _waitFormView.Show();

        Dispatcher.Run();
    }

    public static void Show(string status = null)
    {
        if (!_isInitialized)
        {
            Init(status);

            return;
        }

        _waitFormVm.Status = status;
    }

    public static void Close()
    {
        _waitFormView.Dispatcher.BeginInvoke(() =>
        {
            _waitFormView.Close();

            _waitFormView.Dispatcher.InvokeShutdown();
        }, DispatcherPriority.Background);
    }
}
