using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[SuppressMessage("ReSharper", "UnassignedField.Global", Justification = "Steam API interface")]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Steam API interface")]
#pragma warning disable IDE1006
public class ISteamUserStats007
    {
        public nint RequestCurrentStats;
        public nint GetStatFloat;
        public nint GetStatInteger;
        public nint SetStatFloat;
	    public nint SetStatInteger;
	    public nint UpdateAvgRateStat;
        public nint GetAchievement;
        public nint SetAchievement;
        public nint ClearAchievement;
        public nint GetAchievementAndUnlockTime;
        public nint StoreStats;
        public nint GetAchievementIcon;
        public nint GetAchievementDisplayAttribute;
        public nint IndicateAchievementProgress;
        public nint RequestUserStats;
        public nint GetUserStatFloat;
        public nint GetUserStatInt;
        public nint GetUserAchievement;
        public nint GetUserAchievementAndUnlockTime;
        public nint ResetAllStats;
        public nint FindOrCreateLeaderboard;
        public nint FindLeaderboard;
        public nint GetLeaderboardName;
        public nint GetLeaderboardEntryCount;
        public nint GetLeaderboardSortMethod;
        public nint GetLeaderboardDisplayType;
        public nint DownloadLeaderboardEntries;
        public nint GetDownloadedLeaderboardEntry;
        public nint UploadLeaderboardScore;
        public nint GetNumberOfCurrentPlayers;
    }
