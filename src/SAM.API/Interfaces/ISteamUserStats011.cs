using System;
using System.Runtime.InteropServices;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace SAM.API;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ISteamUserStats011
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
    //
    public nint RequestGlobalAchievementPercentages;
    public nint GetMostAchievedAchievementInfo;
    public nint GetNextMostAchievedAchievementInfo;
    public nint GetAchievementAchievedPercent;
    public nint RequestGlobalStats;
    public nint GetGlobalStatFloat;
    public nint GetGlobalStatInteger;
    public nint GetGlobalStatHistoryFloat;
    public nint GetGlobalStatHistoryInteger;
}
