using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SAM.WPF.Core.Extensions
{
    public static class ProcessExtensions
    {

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        public static bool SetActive(this Process process)
        {
            if (process == null) return false;
            if (process.HasExited) return false;
            if (!process.Responding) return false;

            var hwnd = process.MainWindowHandle;
            var hwndRef = new HandleRef(null, hwnd);
            
            ShowWindowAsync(hwndRef, (int) ShowWindowCommands.SW_RESTORE);
            SetForegroundWindow(hwnd);

            return true;
        }

    }
}
