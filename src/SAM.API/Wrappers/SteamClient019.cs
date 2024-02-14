using System;
using System.Runtime.InteropServices;
using SAM.API.Types;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SAM.API.Wrappers;

public class SteamClient019 : NativeWrapper<ISteamClient019>
{
	#region GetSteamUser012

	public SteamUser012 GetSteamUser012 (int user, int pipe)
	{
		return GetISteamUser<SteamUser012>(user, pipe, "SteamUser012");
	}

	#endregion

	#region GetSteamUser017

	public SteamUser017 GetSteamUser017 (int user, int pipe)
	{
		return GetISteamUser<SteamUser017>(user, pipe, nameof(SteamUser017));
	}

	#endregion

	#region GetSteamUserStats007

	public SteamUserStats007 GetSteamUserStats006 (int user, int pipe)
	{
		return GetISteamUserStats<SteamUserStats007>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION007");
	}

	#endregion

	#region GetSteamUserStats011

	public SteamUserStats011 GetSteamUserStats011 (int user, int pipe)
	{
		return GetISteamUserStats<SteamUserStats011>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION011");
	}

	#endregion

	#region GetSteamUtils004

	public SteamUtils005 GetSteamUtils004 (int pipe)
	{
		return GetISteamUtils<SteamUtils005>(pipe, "SteamUtils005");
	}

	#endregion

	#region GetSteamApps001

	public SteamApps001 GetSteamApps001 (int user, int pipe)
	{
		return GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001");
	}

	#endregion

	#region GetSteamApps008

	public SteamApps008 GetSteamApps008 (int user, int pipe)
	{
		return GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008");
	}

	#endregion

	#region CreateSteamPipe

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate int NativeCreateSteamPipe (nint self);

	public int CreateSteamPipe ()
	{
		return Call<int, NativeCreateSteamPipe>(Functions.CreateSteamPipe, ObjectAddress);
	}

	#endregion

	#region ReleaseSteamPipe

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool NativeReleaseSteamPipe (nint self, int pipe);

	public bool ReleaseSteamPipe (int pipe)
	{
		return Call<bool, NativeReleaseSteamPipe>(Functions.ReleaseSteamPipe, ObjectAddress, pipe);
	}

	#endregion

	#region ReleaseSteamPipe

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool NativeShutdownIfAllPipesClosed (nint self);

	public bool ShutdownIfAllPipesClosed ()
	{
		return Call<bool, NativeShutdownIfAllPipesClosed>(Functions.ShutdownIfAllPipesClosed, ObjectAddress);
	}

	#endregion

	#region CreateLocalUser

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate int NativeCreateLocalUser (nint self, ref int pipe, AccountType type);

	public int CreateLocalUser (ref int pipe, AccountType type)
	{
		var call = GetFunction<NativeCreateLocalUser>(Functions.CreateLocalUser);
		return call(ObjectAddress, ref pipe, type);
	}

	#endregion

	#region ConnectToGlobalUser

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate int NativeConnectToGlobalUser (nint self, int pipe);

	public int ConnectToGlobalUser (int pipe)
	{
		return Call<int, NativeConnectToGlobalUser>(
			Functions.ConnectToGlobalUser,
			ObjectAddress,
			pipe);
	}

	#endregion

	#region ReleaseUser

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void NativeReleaseUser (nint self, int pipe, int user);

	public void ReleaseUser (int pipe, int user)
	{
		Call<NativeReleaseUser>(Functions.ReleaseUser, ObjectAddress, pipe, user);
	}

	#endregion

	#region SetLocalIPBinding

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void NativeSetLocalIPBinding (nint self, uint host, ushort port);

	public void SetLocalIPBinding (uint host, ushort port)
	{
		Call<NativeSetLocalIPBinding>(Functions.SetLocalIPBinding, ObjectAddress, host, port);
	}

	#endregion

	#region GetISteamUser

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate nint NativeGetISteamUser (nint self, int user, int pipe, nint version);

	private TClass GetISteamUser<TClass> (int user, int pipe, string version)
		where TClass : INativeWrapper, new()
	{
		using var nativeVersion = NativeStrings.StringToStringHandle(version);

		var address = Call<nint, NativeGetISteamUser>(
			Functions.GetISteamUser,
			ObjectAddress,
			user,
			pipe,
			nativeVersion.Handle);
		var result = new TClass();
		result.SetupFunctions(address);
		return result;
	}

	#endregion

	#region GetISteamUserStats

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate nint NativeGetISteamUserStats (nint self, int user, int pipe, nint version);

	private TClass GetISteamUserStats<TClass> (int user, int pipe, string version)
		where TClass : INativeWrapper, new()
	{
		using var nativeVersion = NativeStrings.StringToStringHandle(version);

		var address = Call<nint, NativeGetISteamUserStats>(
			Functions.GetISteamUserStats,
			ObjectAddress,
			user,
			pipe,
			nativeVersion.Handle);
		var result = new TClass();
		result.SetupFunctions(address);
		return result;
	}

	#endregion

	#region GetISteamUtils

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate nint NativeGetISteamUtils (nint self, int pipe, nint version);

	public TClass GetISteamUtils<TClass> (int pipe, string version)
		where TClass : INativeWrapper, new()
	{
		using var nativeVersion = NativeStrings.StringToStringHandle(version);

		var address = Call<nint, NativeGetISteamUtils>(
			Functions.GetISteamUtils,
			ObjectAddress,
			pipe,
			nativeVersion.Handle);
		var result = new TClass();
		result.SetupFunctions(address);
		return result;
	}

	#endregion

	#region GetISteamApps

	private delegate nint NativeGetISteamApps (int user, int pipe, nint version);

	private TClass GetISteamApps<TClass> (int user, int pipe, string version)
		where TClass : INativeWrapper, new()
	{
		using var nativeVersion = NativeStrings.StringToStringHandle(version);

		var address = Call<nint, NativeGetISteamApps>(
			Functions.GetISteamApps,
			user,
			pipe,
			nativeVersion.Handle);
		var result = new TClass();
		result.SetupFunctions(address);
		return result;
	}

	#endregion
}
