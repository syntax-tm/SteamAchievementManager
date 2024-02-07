using System;
using System.Runtime.InteropServices;

namespace SAM.API
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ISteamUser012
	{
		public nint GetHSteamUser;
		public nint LoggedOn;
		public nint GetSteamID;
		public nint InitiateGameConnection;
		public nint TerminateGameConnection;
		public nint TrackAppUsageEvent;
		public nint GetUserDataFolder;
		public nint StartVoiceRecording;
		public nint StopVoiceRecording;
		public nint GetCompressedVoice;
		public nint DecompressVoice;
		public nint GetAuthSessionTicket;
		public nint BeginAuthSession;
		public nint EndAuthSession;
		public nint CancelAuthTicket;
		public nint UserHasLicenseForApp;
	}
}
