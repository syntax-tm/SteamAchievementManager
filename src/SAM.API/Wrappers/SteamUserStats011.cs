using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SAM.API.Wrappers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SteamUserStats011 : NativeWrapper<ISteamUserStats011>
{
#region RequestCurrentStats

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeRequestCurrentStats(nint self);

    public bool RequestCurrentStats()
    {
        return Call<bool, NativeRequestCurrentStats>(_functions.RequestCurrentStats, _objectAddress);
    }

#endregion

#region GetStatValue (int)

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatInt(nint self, nint name, out int data);

    public bool GetStatValue(string name, out int value)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        var call = GetFunction<NativeGetStatInt>(_functions.GetStatInteger);
        return call(_objectAddress, nativeName.Handle, out value);
    }

#endregion

#region GetStatValue (float)

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatFloat(nint self, nint name, out float data);

    public bool GetStatValue(string name, out float value)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        var call = GetFunction<NativeGetStatFloat>(_functions.GetStatFloat);
        return call(_objectAddress, nativeName.Handle, out value);
    }

#endregion

#region SetStatValue (int)

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatInt(nint self, nint name, int data);

    public bool SetStatValue(string name, int value)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        return Call<bool, NativeSetStatInt>(
                                            _functions.SetStatInteger,
                                            _objectAddress,
                                            nativeName.Handle,
                                            value);
    }

#endregion

#region SetStatValue (float)

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatFloat(nint self, nint name, float data);

    public bool SetStatValue(string name, float value)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        return Call<bool, NativeSetStatFloat>(_functions.SetStatFloat, _objectAddress,
                                              nativeName.Handle, value);
    }

#endregion

#region UpdateAvgRateStat

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeUpdateAvgRateStat(nint self, nint name, float flCountThisSession, double dSessionLength);

    public bool UpdateAvgRateStat(string name, float flCountThisSession, double dSessionLength)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        return Call<bool, NativeUpdateAvgRateStat>(_functions.UpdateAvgRateStat, _objectAddress,
                                                   nativeName.Handle, flCountThisSession, dSessionLength);
    }

#endregion

#region GetAchievement

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievement(
        nint self,
        nint name,
        [MarshalAs(UnmanagedType.I1)] out bool isAchieved);

    public bool GetAchievementState(string name, out bool isAchieved)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        var call = GetFunction<NativeGetAchievement>(_functions.GetAchievement);
        return call(_objectAddress, nativeName.Handle, out isAchieved);
    }

#endregion

#region SetAchievementState

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetAchievement(nint self, nint name);

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeClearAchievement(nint self, nint name);

    public bool SetAchievement(string name, bool state)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        return state == false
            ? Call<bool, NativeClearAchievement>(_functions.ClearAchievement,
                                                 _objectAddress,
                                                 nativeName.Handle)
            : Call<bool, NativeSetAchievement>(_functions.SetAchievement,
                                               _objectAddress,
                                               nativeName.Handle);
    }

#endregion

#region StoreStats

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeStoreStats(nint self);

    public bool StoreStats()
    {
        return Call<bool, NativeStoreStats>(_functions.StoreStats, _objectAddress);
    }

#endregion

#region GetAchievementIcon

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetAchievementIcon(nint self, nint name);

    public int GetAchievementIcon(string name)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);

        return Call<int, NativeGetAchievementIcon>(_functions.GetAchievementIcon,
                                                   _objectAddress,
                                                   nativeName.Handle);
    }

#endregion

#region GetAchievementDisplayAttribute

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate nint NativeGetAchievementDisplayAttribute(nint self, nint name, nint key);

    public string GetAchievementDisplayAttribute(string name, string key)
    {
        using var nativeName = NativeStrings.StringToStringHandle(name);
        using var nativeKey = NativeStrings.StringToStringHandle(key);

        var result = Call<nint, NativeGetAchievementDisplayAttribute>(_functions.GetAchievementDisplayAttribute, 
                                                                      _objectAddress,
                                                                      nativeName.Handle,
                                                                      nativeKey.Handle);
        return NativeStrings.PointerToString(result);
    }

#endregion

#region ResetAllStats

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeResetAllStats(nint self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);

    public bool ResetAllStats(bool achievementsToo)
    {
        return Call<bool, NativeResetAllStats>(
                                               _functions.ResetAllStats,
                                               _objectAddress,
                                               achievementsToo);
    }

#endregion
}
