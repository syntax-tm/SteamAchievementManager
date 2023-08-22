using System;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers
{
    public class SteamUser012 : NativeWrapper<ISteamUser012>
    {
#region IsLoggedIn

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeLoggedOn(IntPtr self);

        public bool IsLoggedIn()
        {
            return Call<bool, NativeLoggedOn>(Functions.LoggedOn, ObjectAddress);
        }

#endregion

#region GetSteamID

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeGetSteamId(IntPtr self, out ulong steamId);

        public ulong GetSteamId()
        {
            var call = GetFunction<NativeGetSteamId>(Functions.GetSteamID);
            call(ObjectAddress, out var steamId);
            return steamId;
        }

#endregion
    }
}
