using System.Diagnostics;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace SAM.Core.Extensions;

public static class ProcessExtensions
{
    [SupportedOSPlatform("windows5.0")] 
    public static bool SetActive(this Process process)
    {
        if (process == null) return false;
        if (process.HasExited) return false;
        if (!process.Responding) return false;

        var hwnd = process.MainWindowHandle;
        var hwndRef = new HWND(hwnd);
            
        PInvoke.ShowWindowAsync(hwndRef, SHOW_WINDOW_CMD.SW_RESTORE);
        PInvoke.SetForegroundWindow(hwndRef);

        return true;
    }

}
