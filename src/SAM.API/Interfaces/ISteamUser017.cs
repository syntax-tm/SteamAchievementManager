using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "Steam API interface")]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Steam API interface")]
#pragma warning disable IDE1006
public struct ISteamUser017
{
	public nint GetHSteamUser;
	public nint BLoggedOn;
	public nint GetSteamID;
	public nint InitiateGameConnection;
	public nint TerminateGameConnection;
	public nint TrackAppUsageEvent;
	public nint GetUserDataFolder;
	public nint StartVoiceRecording;
	public nint StopVoiceRecording;
	public nint GetAvailableVoice;
	public nint GetVoice;
	public nint DecompressVoice;
	public nint GetVoiceOptimalSampleRate;
	public nint GetAuthSessionTicket;
	public nint BeginAuthSession;
	public nint EndAuthSession;
	public nint CancelAuthTicket;
	public nint UserHasLicenseForApp;
	public nint BIsBehindNAT;
	public nint AdvertiseGame;
	public nint RequestEncryptedAppTicket;
	public nint GetEncryptedAppTicket;
	public nint GetGameBadgeLevel;
	public nint GetPlayerSteamLevel;
}
