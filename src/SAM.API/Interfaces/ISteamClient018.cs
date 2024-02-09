using System;
using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamClient018
{
	public nint CreateSteamPipe;
	public nint ReleaseSteamPipe;
	public nint ConnectToGlobalUser;
	public nint CreateLocalUser;
	public nint ReleaseUser;
	public nint GetISteamUser;
	public nint GetISteamGameServer;
	public nint SetLocalIPBinding;
	public nint GetISteamFriends;
	public nint GetISteamUtils;
	public nint GetISteamMatchmaking;
	public nint GetISteamMatchmakingServers;
	public nint GetISteamGenericInterface;
	public nint GetISteamUserStats;
	public nint GetISteamGameServerStats;
	public nint GetISteamApps;
	public nint GetISteamNetworking;
	public nint GetISteamRemoteStorage;
	public nint GetISteamScreenshots;
	public nint GetISteamGameSearch;
	public nint RunFrame;
	public nint GetIPCCallCount;
	public nint SetWarningMessageHook;
	public nint ShutdownIfAllPipesClosed;
	public nint GetISteamHTTP;
	public nint DEPRECATED_GetISteamUnifiedMessages;
	public nint GetISteamController;
	public nint GetISteamUGC;
	public nint GetISteamAppList;
	public nint GetISteamMusic;
	public nint GetISteamMusicRemote;
	public nint GetISteamHTMLSurface;
	public nint DEPRECATED_Set_SteamAPI_CPostAPIResultInProcess;
	public nint DEPRECATED_Remove_SteamAPI_CPostAPIResultInProcess;
	public nint Set_SteamAPI_CCheckCallbackRegisteredInProcess;
	public nint GetISteamInventory;
	public nint GetISteamVideo;
	public nint GetISteamParentalSettings;
	public nint GetISteamInput;
	public nint GetISteamParties;
}
