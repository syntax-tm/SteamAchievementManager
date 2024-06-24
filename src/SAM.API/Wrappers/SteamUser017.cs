using System.Runtime.InteropServices;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SAM.API.Wrappers;

public class SteamUser017 : NativeWrapper<ISteamUser017>
{
#region IsLoggedIn

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeLoggedOn(nint self);

    public bool IsLoggedIn()
    {
            return Call<bool, NativeLoggedOn>(_functions.BLoggedOn, _objectAddress);
        }

#endregion

#region GetSteamID

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeGetSteamId(nint self, out ulong steamId);

    public ulong GetSteamId()
    {
            var call = GetFunction<NativeGetSteamId>(_functions.GetSteamID);
            call(_objectAddress, out var steamId);
            return steamId;
        }

#endregion

#region GetPlayerSteamLevel
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I4)]
    private delegate int NativeGetPlayerSteamLevel(nint self);

    public int GetPlayerSteamLevel()
    {
            return Call<int, NativeGetPlayerSteamLevel>(_functions.GetPlayerSteamLevel, _objectAddress);
        }
#endregion
}
