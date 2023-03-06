using System;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers
{
    public class SteamApps001 : NativeWrapper<ISteamApps001>
    {
#region GetAppData

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetAppData(
            nint self,
            uint appId,
            nint key,
            nint value,
            int valueLength);

        public string GetAppData(uint appId, string key)
        {
            using var nativeHandle = NativeStrings.StringToStringHandle(key);

            const int valueLength = 1024;
            var valuePointer = Marshal.AllocHGlobal(valueLength);
            var result = Call<int, NativeGetAppData>(
                                                     Functions.GetAppData,
                                                     ObjectAddress,
                                                     appId,
                                                     nativeHandle.Handle,
                                                     valuePointer,
                                                     valueLength);
            var value = result == 0 ? null : NativeStrings.PointerToString(valuePointer, valueLength);
            Marshal.FreeHGlobal(valuePointer);
            return value;
        }

#endregion
    }
}
