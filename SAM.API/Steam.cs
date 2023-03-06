using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SAM.API
{
    public static class Steam
    {
        private static string _installPath;

#if BUILD_X86
        private const string STEAM_CLIENT_DLL = @"steamclient.dll";
#elif BUILD_X64
        private const string STEAM_CLIENT_DLL = @"steamclient64.dll";
#elif BUILD_ANYCPU
        private const string STEAM_CLIENT_DLL = @"steamclient.dll";
#else
#error Unsupported platform. Target either x86 for 32-bit or x64 for 64-bit.
#endif

        private static nint _Handle = nint.Zero;
        private static NativeCreateInterface _CallCreateInterface;
        private static NativeSteamGetCallback _CallSteamBGetCallback;
        private static NativeSteamFreeLastCallback _CallSteamFreeLastCallback;

        private static Delegate GetExportDelegate<TDelegate>(nint module, string name)
        {
            var address = Native.GetProcAddress(module, name);
            return address == nint.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        private static TDelegate GetExportFunction<TDelegate>(nint module, string name)
            where TDelegate : class
        {
            return GetExportDelegate<TDelegate>(module, name) as TDelegate;

            //return (TDelegate)((object)GetExportDelegate<TDelegate>(module, name));
        }

        public static string GetInstallPath()
        {
            if (!string.IsNullOrEmpty(_installPath)) return _installPath;

            using var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var clsid32 = view32.OpenSubKey(@"Software\Valve\Steam", false);

            _installPath = (string) clsid32?.GetValue(@"InstallPath");
            return _installPath;

            //return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        }

        public static TClass CreateInterface<TClass>(string version)
            where TClass : INativeWrapper, new()
        {
            var address = _CallCreateInterface(version, nint.Zero);

            if (address == nint.Zero) return default;

            var rez = new TClass();
            rez.SetupFunctions(address);
            return rez;
        }

        public static bool GetCallback(int pipe, out CallbackMessage message, out int call)
        {
            return _CallSteamBGetCallback(pipe, out message, out call);
        }

        public static bool FreeLastCallback(int pipe)
        {
            return _CallSteamFreeLastCallback(pipe);
        }

        public static bool Load()
        {
            if (_Handle != nint.Zero)
            {
                Native.FreeLibrary(_Handle);
                _Handle = nint.Zero;
            }

            var path = GetInstallPath();
            if (path == null) return false;

            Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));
            path = Path.Combine(path, STEAM_CLIENT_DLL);

            var module = Native.LoadLibraryEx(path, nint.Zero, Native.LoadWithAlteredSearchPath);
            if (module == nint.Zero) return false;

            _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
            if (_CallCreateInterface == null) return false;

            _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
            if (_CallSteamBGetCallback == null) return false;

            _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_CallSteamFreeLastCallback == null) return false;

            _Handle = module;
            return true;
        }

        private struct Native
        {
            [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern nint GetProcAddress(nint module, string name);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern nint LoadLibraryEx(string path, nint file, uint flags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory(string path);
            
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeLibrary(nint hModule);

            internal const uint LoadWithAlteredSearchPath = 8;
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
