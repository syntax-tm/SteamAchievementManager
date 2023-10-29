using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.LibraryLoader;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace SAM.API
{
    public static class Steam
    {
        private static string _installPath;
        
        private static nint _handle = nint.Zero;
        private static NativeCreateInterface _callCreateInterface;
        private static NativeSteamGetCallback _callSteamBGetCallback;
        private static NativeSteamFreeLastCallback _callSteamFreeLastCallback;

        private static string SteamClientDll => Environment.Is64BitProcess ? @"steamclient64.dll" : @"steamclient.dll";

        private static Delegate GetExportDelegate<TDelegate>(nint module, string name)
        {
            var hModule = new SafeProcessHandle(module, false);
            var address = PInvoke.GetProcAddress(hModule, name);
            return address == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        private static TDelegate GetExportFunction<TDelegate>(nint module, string name)
            where TDelegate : class
        {
            return GetExportDelegate<TDelegate>(module, name) as TDelegate;
        }

        public static string GetInstallPath()
        {
            if (!string.IsNullOrEmpty(_installPath)) return _installPath;

            // TODO: consider switching this to HKCU:\SOFTWARE\Valve\Steam\ActiveProcess\SteamClientDll
            using var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var clsid32 = view32.OpenSubKey(@"Software\Valve\Steam", false);

            _installPath = (string) clsid32?.GetValue(@"InstallPath");
            return _installPath;

            //return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        }

        public static TClass CreateInterface<TClass>(string version)
            where TClass : INativeWrapper, new()
        {
            var address = _callCreateInterface(version, nint.Zero);

            if (address == nint.Zero) return default;

            var rez = new TClass();
            rez.SetupFunctions(address);
            return rez;
        }

        public static bool GetCallback(int pipe, out CallbackMessage message, out int call)
        {
            return _callSteamBGetCallback(pipe, out message, out call);
        }

        public static bool FreeLastCallback(int pipe)
        {
            return _callSteamFreeLastCallback(pipe);
        }

        public static bool Load()
        {
            if (_handle != nint.Zero)
            {
                PInvoke.FreeLibrary(new (_handle));
                _handle = nint.Zero;
            }

            var path = GetInstallPath();
            if (path == null) return false;

            PInvoke.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));
            path = Path.Combine(path, SteamClientDll);

            var module = PInvoke.LoadLibraryEx(path, LOAD_LIBRARY_FLAGS.LOAD_WITH_ALTERED_SEARCH_PATH);
            if (module == default) return false;

            var hModule = module.DangerousGetHandle();

            _callCreateInterface = GetExportFunction<NativeCreateInterface>(hModule, "CreateInterface");
            if (_callCreateInterface == null) return false;

            _callSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(hModule, "Steam_BGetCallback");
            if (_callSteamBGetCallback == null) return false;

            _callSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(hModule, "Steam_FreeLastCallback");
            if (_callSteamFreeLastCallback == null) return false;

            _handle = hModule;
            return true;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate nint NativeCreateInterface(string version, nint returnCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamGetCallback(int pipe, out CallbackMessage message, out int call);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamFreeLastCallback(int pipe);
    }
}
