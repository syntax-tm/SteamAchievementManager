using System;
using System.Runtime.InteropServices;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SAM.API.Wrappers;

public class SteamApps008 : NativeWrapper<ISteamApps008>
{
#region IsAppInstalled

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsAppInstalled(nint self, uint gameId);

    public bool IsAppInstalled(uint gameId)
    {
        return Call<bool, NativeIsAppInstalled>(_functions.IsAppInstalled, _objectAddress, gameId);
    }

#endregion

#region IsSubscribedFromFamilySharing

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedFromFamilySharing(nint self);

    public bool IsSubscribedFromFamilySharing()
    {
        return Call<bool, NativeIsSubscribedFromFamilySharing>(_functions.IsSubscribedFromFamilySharing, _objectAddress);
    }

#endregion

#region IsSubscribedFromFreeWeekend

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedFromFreeWeekend(nint self);

    public bool IsSubscribedFromFreeWeekend()
    {
        return Call<bool, NativeIsSubscribedFromFreeWeekend>(_functions.IsSubscribedFromFreeWeekend, _objectAddress);
    }

#endregion

#region MarkContentCorrupt

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeMarkContentCorrupt(nint self, bool missingFilesOnly);

    public bool MarkContentCorrupt(bool missingFilesOnly)
    {
        return Call<bool, NativeMarkContentCorrupt>(_functions.MarkContentCorrupt, _objectAddress, missingFilesOnly);
    }

#endregion

#region IsVACBanned

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsVACBanned(nint self);

    public bool IsVACBanned()
    {
        return Call<bool, NativeIsVACBanned>(_functions.IsVACBanned, _objectAddress);
    }

#endregion

#region IsSubscribed

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedApp(nint self, uint gameId);

    public bool IsSubscribedApp(uint gameId)
    {
        return Call<bool, NativeIsSubscribedApp>(_functions.IsSubscribedApp, _objectAddress, gameId);
    }

#endregion

#region GetCurrentGameLanguage

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate nint NativeGetCurrentGameLanguage(nint self);

    public string GetCurrentGameLanguage()
    {
        var languagePointer = Call<nint, NativeGetCurrentGameLanguage>(_functions.GetCurrentGameLanguage, _objectAddress);
        return NativeStrings.PointerToString(languagePointer);
    }

#endregion
}
