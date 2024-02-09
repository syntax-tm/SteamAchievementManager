using System;
using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamUtils005
{
	public nint GetSecondsSinceAppActive;
	public nint GetSecondsSinceComputerActive;
	public nint GetConnectedUniverse;
	public nint GetServerRealTime;
	public nint GetIPCountry;
	public nint GetImageSize;
	public nint GetImageRGBA;
	public nint GetCSERIPPort;
	public nint GetCurrentBatteryPower;
	public nint GetAppID;
	public nint SetOverlayNotificationPosition;
	public nint IsAPICallCompleted;
	public nint GetAPICallFailureReason;
	public nint GetAPICallResult;
	public nint RunFrame;
	public nint GetIPCCallCount;
	public nint SetWarningMessageHook;
	public nint IsOverlayEnabled;
	public nint OverlayNeedsPresent;
}
