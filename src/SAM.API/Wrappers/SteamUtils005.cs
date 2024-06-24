using System;
using System.Runtime.InteropServices;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SAM.API.Wrappers;

public class SteamUtils005 : NativeWrapper<ISteamUtils005>
{
#region GetConnectedUniverse

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetConnectedUniverse(nint self);

    public int GetConnectedUniverse()
    {
        return Call<int, NativeGetConnectedUniverse>(_functions.GetConnectedUniverse, _objectAddress);
    }

#endregion

#region GetIPCountry

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate nint NativeGetIPCountry(nint self);

    public string GetIPCountry()
    {
        var result = Call<nint, NativeGetIPCountry>(_functions.GetIPCountry, _objectAddress);
        return NativeStrings.PointerToString(result);
    }

#endregion

#region GetImageSize

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageSize(nint self, int index, out int width, out int height);

    public bool GetImageSize(int index, out int width, out int height)
    {
        var call = GetFunction<NativeGetImageSize>(_functions.GetImageSize);
        return call(_objectAddress, index, out width, out height);
    }

#endregion

#region GetImageRGBA

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageRGBA(nint self, int index, byte[] buffer, int length);

    public bool GetImageRGBA(int index, byte[] data)
    {
        if (data == null) throw new ArgumentNullException("data");

        var call = GetFunction<NativeGetImageRGBA>(_functions.GetImageRGBA);
        return call(_objectAddress, index, data, data.Length);
    }

#endregion

#region GetAppID

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate uint NativeGetAppId(nint self);

    public uint GetAppId()
    {
        return Call<uint, NativeGetAppId>(_functions.GetAppID, _objectAddress);
    }

#endregion
}
