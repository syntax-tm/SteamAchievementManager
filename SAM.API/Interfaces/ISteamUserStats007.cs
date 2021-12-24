using System;
using System.Runtime.InteropServices;

namespace SAM.API
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ISteamUserStats007
    {
        public IntPtr ClearAchievement;
        public IntPtr DownloadLeaderboardEntries;
        public IntPtr FindLeaderboard;
        public IntPtr FindOrCreateLeaderboard;
        public IntPtr GetAchievement;
        public IntPtr GetAchievementAndUnlockTime;
        public IntPtr GetAchievementDisplayAttribute;
        public IntPtr GetAchievementIcon;
        public IntPtr GetDownloadedLeaderboardEntry;
        public IntPtr GetLeaderboardDisplayType;
        public IntPtr GetLeaderboardEntryCount;
        public IntPtr GetLeaderboardName;
        public IntPtr GetLeaderboardSortMethod;
        public IntPtr GetNumberOfCurrentPlayers;
        public IntPtr GetStatFloat;
        public IntPtr GetStatInteger;
        public IntPtr GetUserAchievement;
        public IntPtr GetUserAchievementAndUnlockTime;
        public IntPtr GetUserStatFloat;
        public IntPtr GetUserStatInt;
        public IntPtr IndicateAchievementProgress;
        public IntPtr RequestCurrentStats;
        public IntPtr RequestUserStats;
        public IntPtr ResetAllStats;
        public IntPtr SetAchievement;
        public IntPtr SetStatFloat;
        public IntPtr SetStatInteger;
        public IntPtr StoreStats;
        public IntPtr UpdateAvgRateStat;
        public IntPtr UploadLeaderboardScore;
    }
}
