﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SAM.API.Types;

namespace SAM.API
{
    public static class Steam
    {
        private static string _installPath;

        private static IntPtr _Handle = IntPtr.Zero;
        private static NativeCreateInterface _CallCreateInterface;
        private static NativeSteamGetCallback _CallSteamBGetCallback;
        private static NativeSteamFreeLastCallback _CallSteamFreeLastCallback;

        private static Delegate GetExportDelegate<TDelegate>(IntPtr module, string name)
        {
            var address = Native.GetProcAddress(module, name);
            return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        private static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name)
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

            _installPath = (string) clsid32.GetValue(@"InstallPath");
            return _installPath;

            //return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        }

        public static TClass CreateInterface<TClass>(string version)
            where TClass : INativeWrapper, new()
        {
            var address = _CallCreateInterface(version, IntPtr.Zero);

            if (address == IntPtr.Zero) return default;

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
            if (_Handle != IntPtr.Zero)
            {
                Native.FreeLibrary(_Handle);
                _Handle = IntPtr.Zero;
            }

            var path = GetInstallPath();
            if (path == null) return false;

            Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));

#if BUILD_X86
            path = Path.Combine(path, "steamclient.dll");
#elif BUILD_X64
            path = Path.Combine(path, "steamclient64.dll");
#elif BUILD_ANYCPU
            // for AnyCPU we need to check and see if we're running as a 32-bit
            // or 64-bit process since it depends on the processor architecture
            var is64Bit = Environment.Is64BitOperatingSystem && Environment.Is64BitProcess;
            var clientDll = is64Bit
                ? "steamclient64.dll"
                : "steamclient.dll";

            path = Path.Combine(path, clientDll);
#else
#error Unknown project platform. Target either x86 for 32-bit or x64 for 64-bit.
#endif

            var module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LoadWithAlteredSearchPath);
            if (module == IntPtr.Zero) return false;

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
            internal static extern IntPtr GetProcAddress(IntPtr module, string name);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory(string path);
            
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeLibrary(IntPtr hModule);

            internal const uint LoadWithAlteredSearchPath = 8;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate IntPtr NativeCreateInterface(string version, IntPtr returnCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamGetCallback(int pipe, out CallbackMessage message, out int call);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamFreeLastCallback(int pipe);
    }
}
