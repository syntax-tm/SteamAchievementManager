using System;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers
{
    public class SteamApps008 : NativeWrapper<ISteamApps008>
    {
#region IsSubscribed

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeIsSubscribedApp(IntPtr self, uint gameId);

        public bool IsSubscribedApp(uint gameId)
        {
            return Call<bool, NativeIsSubscribedApp>(Functions.IsSubscribedApp, ObjectAddress, gameId);
        }

#endregion

#region GetCurrentGameLanguage

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetCurrentGameLanguage(IntPtr self);

        public string GetCurrentGameLanguage()
        {
            var languagePointer = Call<IntPtr, NativeGetCurrentGameLanguage>(
                Functions.GetCurrentGameLanguage,
                ObjectAddress);
            return NativeStrings.PointerToString(languagePointer);
        }

#endregion
    }
}
