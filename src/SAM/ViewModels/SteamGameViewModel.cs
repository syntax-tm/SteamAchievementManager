// Added necessary using directives for new features
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using JetBrains.Annotations;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Managers;
using SAM.Stats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SAM.ViewModels
{
    #region Steam Web API Data Contracts
    // Helper classes for deserializing the JSON response from the Steam API.
    public class ApiAchievement
    {
        [JsonPropertyName("apiname")]
        public string ApiName { get; set; }

        [JsonPropertyName("achieved")]
        public int Achieved { get; set; }

        [JsonPropertyName("unlocktime")]
        public long UnlockTime { get; set; }
    }

    public class PlayerStatsPayload
    {
        [JsonPropertyName("steamID")]
        public string SteamID { get; set; }

        [JsonPropertyName("gameName")]
        public string GameName { get; set; }

        [JsonPropertyName("achievements")]
        public List<ApiAchievement> Achievements { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    public class SteamApiResponse
    {
        [JsonPropertyName("playerstats")]
        public PlayerStatsPayload PlayerStats { get; set; }
    }
    #endregion

    [GenerateViewModel(ImplementISupportServices = true)]
    public partial class SteamGameViewModel
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(SteamGameViewModel));

        // A single, static HttpClient instance is used for performance and resource management.
        private static readonly HttpClient httpClient = new();
        private CancellationTokenSource _autoUnlockCts;

        public virtual ICurrentWindowService CurrentWindow => GetService<ICurrentWindowService>();

        private bool _loading = true;
        private readonly object syncLock = new();
        private CollectionViewSource _achievementsViewSource;

        [UsedImplicitly]
        private readonly ObservableHandler<SteamStatsManager> statsHandler;

        [UsedImplicitly]
        private ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement> _achievementsPropertyHandler;

        private readonly SteamStatsManager _statsManager;

        [GenerateProperty] private string searchText;
        [GenerateProperty] private bool allowUnlockAll;
        [GenerateProperty] private bool allowEdit;
        [GenerateProperty] private bool isModified;
        [GenerateProperty] private bool showHidden;
        [GenerateProperty] private AchievementFilter selectedAchievementFilter;
        [GenerateProperty] private SteamApp steamApp;
        [GenerateProperty] private SteamAchievement selectedAchievement;
        [GenerateProperty] private ObservableCollection<SteamStatisticBase> statistics;
        [GenerateProperty] private ObservableCollection<SteamAchievement> achievements;
        [GenerateProperty] private ICollectionView achievementsView;

        #region Auto-Unlock Properties
        [GenerateProperty] private int selectedTabIndex;
        [GenerateProperty] private string currentUnlockStatus;
        [GenerateProperty] private string nextUnlockCountdown;
        [GenerateProperty] private ObservableCollection<string> autoUnlockLog;

        // Properties to bind to the new UI text boxes
        [GenerateProperty] private string webAPIKey;
        [GenerateProperty] private string steamID;
        #endregion

        public SteamGameViewModel()
        {
            // Constructor for the designer.
        }

        public SteamGameViewModel(SteamApp steamApp)
        {
            SteamApp = steamApp;
            _statsManager = new(SteamClientManager.Default);
            statsHandler = new ObservableHandler<SteamStatsManager>(_statsManager)
                           .AddAndInvoke(m => m.Achievements, ManagerAchievementsChanged)
                           .AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged)
                           .AddAndInvoke(m => m.IsModified, OnManagerIsModifiedChanged);

            AutoUnlockLog = new();
            BindingOperations.EnableCollectionSynchronization(AutoUnlockLog, syncLock);
            CurrentUnlockStatus = "Idle. Enter API Key and SteamID, then press Start.";
            NextUnlockCountdown = "N/A";
        }

        #region Core Data Methods (Save, Reset, etc.)

        public int SaveAchievements()
        {
            var saved = 0;
            try
            {
                var modified = Achievements?.Where(a => a.IsModified).ToList();
                if (modified == null || !modified.Any())
                {
                    log.Info("User achievements have not been modified. Skipping save.");
                    return 0;
                }

                var stats = SteamClientManager.Default.SteamUserStats;

                foreach (var achievement in modified)
                {
                    if (!stats.SetAchievement(achievement.Id, achievement.IsAchieved)) throw new SAMException($"Failed to update achievement {achievement.Id}.");
                    if (!stats.StoreStats()) throw new SAMException($"Failed to store stats after updating achievement {achievement.Id}.");

                    achievement.CommitChanges();
                    saved++;
                }

                return saved;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to save achievements. {e.Message}";
                log.Error(message, e);
                MessageBox.Show(message, "Error Updating Achievements", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public int SaveStats()
        {
            var saved = 0;
            try
            {
                var modified = Statistics?.Where(a => a.IsModified).ToList();
                if (modified == null || !modified.Any())
                {
                    log.Info("User stats have not been modified. Skipping save.");
                    return 0;
                }

                var stats = SteamClientManager.Default.SteamUserStats;

                foreach (var stat in modified)
                {
                    var result = stat switch
                    {
                        IntegerSteamStatistic intStat => stats.SetStatValue(stat.Id, intStat.Value),
                        AverageRateSteamStatistic avgRateStat => stats.UpdateAvgRateStat(stat.Id, avgRateStat.AvgRateNumerator, avgRateStat.AvgRateDenominator),
                        FloatSteamStatistic floatStat => stats.SetStatValue(stat.Id, floatStat.Value),
                        _ => throw new InvalidOperationException($"Unknown stat type {stat.StatType} is modified and cannot be saved.")
                    };

                    if (!result) throw new SAMException($"Failed to update {stat.StatType} stat {stat.Id}.");

                    stat.CommitChanges();
                    saved++;
                }

                if (!stats.StoreStats()) throw new SAMException("Failed to store stats after updating one or more statistics.");

                return saved;
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to save stats. {e.Message}";
                log.Error(message, e);
                MessageBox.Show(message, "Error Updating Stats", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        private bool SaveSingleAchievement(SteamAchievement achievement)
        {
            try
            {
                var stats = SteamClientManager.Default.SteamUserStats;

                if (!stats.SetAchievement(achievement.Id, achievement.IsAchieved))
                {
                    var message = $"Steamworks returned false when setting achievement '{achievement.Name}' ({achievement.Id}).";
                    log.Error(message);
                    AutoUnlockLog.Add($"ERROR: {message}");
                    return false;
                }

                if (!stats.StoreStats())
                {
                    var message = $"Steamworks returned false when storing stats after setting achievement '{achievement.Name}' ({achievement.Id}).";
                    log.Error(message);
                    AutoUnlockLog.Add($"ERROR: {message}");
                    return false;
                }

                achievement.CommitChanges();
                log.Info($"Successfully unlocked and stored achievement: {achievement.Name} ({achievement.Id})");
                return true;
            }
            catch (Exception e)
            {
                var message = $"An exception occurred attempting to save achievement '{achievement.Name}'. {e.Message}";
                log.Error(message, e);
                AutoUnlockLog.Add($"ERROR: {message}");
                return false;
            }
        }

        [GenerateCommand]
        public void Save(bool displayResult = true)
        {
            var achievementsSaved = SaveAchievements();
            if (achievementsSaved == -1) return;

            var statsSaved = SaveStats();
            if (statsSaved == -1) return;

            var message = new StringBuilder();
            var achievementMessage = achievementsSaved switch { 0 => string.Empty, 1 => $"{achievementsSaved} achievement", _ => $"{achievementsSaved} achievements" };
            var statsMessage = statsSaved switch { 0 => string.Empty, 1 => $"{statsSaved} stat", _ => $"{statsSaved} stats" };

            message.Append("Successfully saved ");
            message.Append(achievementMessage);
            if (achievementsSaved > 0 && statsSaved > 0) message.Append(" and ");
            message.Append(statsMessage);
            message.Append(".");

            if (displayResult) MessageBox.Show(message.ToString(), "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);

            log.Info(message);
        }

        public void RefreshStats() => _statsManager.RefreshStats();

        // FIX: Replaced .ForEach() with a standard foreach loop.
        public void ResetAchievements()
        {
            if (Achievements == null) return;
            foreach (var a in Achievements) a.Reset();
        }

        public void ResetStats()
        {
            if (Statistics == null) return;
            foreach (var s in Statistics) s.Reset();
        }

        [GenerateCommand]
        public void Reset()
        {
            ResetAchievements();
            ResetStats();
        }

        [GenerateCommand]
        public void LockAllAchievements()
        {
            if (Achievements == null) return;
            foreach (var a in Achievements) a.Lock();
        }

        [GenerateCommand]
        public void UnlockAllAchievements()
        {
            if (Achievements == null) return;
            foreach (var a in Achievements) a.Unlock();
        }

        #endregion

        #region New Auto-Unlock Implementation (replaces file-based unlocker)

        [GenerateCommand]
        public async void StartAutoUnlock()
        {
            if (_autoUnlockCts != null && !_autoUnlockCts.IsCancellationRequested)
            {
                AutoUnlockLog.Add("An unlock process is already running.");
                return;
            }

            if (string.IsNullOrWhiteSpace(WebAPIKey) || string.IsNullOrWhiteSpace(SteamID))
            {
                MessageBox.Show("The WebAPI Key and SteamID fields cannot be empty.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Switch to the log tab (assuming it's the 3rd tab, index 2)
            SelectedTabIndex = 2;
            AutoUnlockLog.Clear();
            _autoUnlockCts = new CancellationTokenSource();
            var token = _autoUnlockCts.Token;

            try
            {
                CurrentUnlockStatus = "Fetching achievement data from Steam...";
                NextUnlockCountdown = "N/A";
                // Assuming your SteamApp object has a public 'Id' property for the AppID
                AutoUnlockLog.Add($"Requesting data for SteamID: {SteamID} and AppID: {SteamApp.Id}");

                var url = $"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={WebAPIKey}&steamid={SteamID}&appid={SteamApp.Id}";
                var response = await httpClient.GetAsync(url, token);

                if (!response.IsSuccessStatusCode) throw new SAMException($"Steam API request failed. Status: {response.StatusCode}.");

                var jsonStream = await response.Content.ReadAsStreamAsync(token);
                var apiData = await JsonSerializer.DeserializeAsync<SteamApiResponse>(jsonStream, cancellationToken: token);

                if (apiData?.PlayerStats?.Success != true || apiData.PlayerStats.Achievements == null) throw new SAMException("Steam API response was unsuccessful or did not contain achievement data.");

                AutoUnlockLog.Add($"Successfully fetched data for game: {apiData.PlayerStats.GameName}");

                var sourceAchievements = apiData.PlayerStats.Achievements
                                                .Where(a => a.Achieved == 1 && a.UnlockTime > 0)
                                                .OrderBy(a => a.UnlockTime)
                                                .ToList();

                if (!sourceAchievements.Any())
                {
                    CurrentUnlockStatus = "Process finished: No unlocked achievements found to mimic.";
                    AutoUnlockLog.Add("The specified user has no unlocked achievements for this game.");
                    return;
                }

                var unlockQueue = new List<(SteamAchievement achievement, TimeSpan delay)>();
                long lastUnlockTimestamp = 0;

                foreach (var sourceAch in sourceAchievements)
                {
                    var localAch = Achievements.FirstOrDefault(a => a.Id.Equals(sourceAch.ApiName, StringComparison.OrdinalIgnoreCase));
                    if (localAch == null)
                    {
                        AutoUnlockLog.Add($"Warning: Source achievement '{sourceAch.ApiName}' not found locally. Skipping.");
                        continue;
                    }
                    if (localAch.IsAchieved)
                    {
                        AutoUnlockLog.Add($"Skipping '{localAch.Name}' as it is already unlocked.");
                        continue;
                    }

                    var delay = (lastUnlockTimestamp > 0) ? TimeSpan.FromSeconds(sourceAch.UnlockTime - lastUnlockTimestamp) : TimeSpan.Zero;
                    unlockQueue.Add((localAch, delay < TimeSpan.Zero ? TimeSpan.Zero : delay));
                    lastUnlockTimestamp = sourceAch.UnlockTime;
                }

                if (!unlockQueue.Any())
                {
                    CurrentUnlockStatus = "Process complete. All source achievements are already unlocked locally.";
                    AutoUnlockLog.Add("No new achievements to unlock.");
                    return;
                }

                await ProcessUnlockQueue(unlockQueue, token);
            }
            catch (OperationCanceledException)
            {
                CurrentUnlockStatus = "Process stopped by user.";
                NextUnlockCountdown = "Idle.";
                AutoUnlockLog.Add("Auto-unlock process was stopped.");
                log.Info("Auto-unlock process was cancelled by the user.");
            }
            catch (Exception ex)
            {
                var message = $"A critical error occurred during the auto unlock process: {ex.Message}";
                log.Error(message, ex);
                AutoUnlockLog.Add($"FATAL ERROR: {message}");
                CurrentUnlockStatus = "A critical error occurred.";
                NextUnlockCountdown = "Process halted.";
                MessageBox.Show(message, "Auto Unlock Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _autoUnlockCts?.Dispose();
                _autoUnlockCts = null;
            }
        }

        private async Task ProcessUnlockQueue(List<(SteamAchievement achievement, TimeSpan delay)> queue, CancellationToken token)
        {
            AutoUnlockLog.Add($"Beginning unlock process for {queue.Count} achievements.");

            foreach (var (achievement, delay) in queue)
            {
                token.ThrowIfCancellationRequested();
                CurrentUnlockStatus = $"Waiting to unlock: {achievement.Name}";
                AutoUnlockLog.Add($"Queueing '{achievement.Name}' with a delay of {delay:g}.");

                var countdown = delay;
                while (countdown.TotalSeconds > 0)
                {
                    token.ThrowIfCancellationRequested();
                    NextUnlockCountdown = $"Time until next unlock: {countdown:hh\\:mm\\:ss}";
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                    countdown = countdown.Subtract(TimeSpan.FromSeconds(1));
                }

                token.ThrowIfCancellationRequested();
                NextUnlockCountdown = "Unlocking now...";
                CurrentUnlockStatus = $"Unlocking & Saving: {achievement.Name}";
                achievement.Unlock();

                if (!SaveSingleAchievement(achievement))
                {
                    AutoUnlockLog.Add($"FAILED to save achievement: {achievement.Name}. The process will be stopped.");
                    CurrentUnlockStatus = $"Error saving {achievement.Name}. See log.";
                    NextUnlockCountdown = "Process halted on error.";
                    MessageBox.Show($"Failed to save achievement: {achievement.Name}. The process will stop. Check the log for details.", "Auto Unlock Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            AutoUnlockLog.Add("Auto-unlock process completed successfully.");
            CurrentUnlockStatus = "Process complete.";
            NextUnlockCountdown = "Idle.";
            MessageBox.Show("The auto-unlock process has completed successfully.", "Auto Unlock Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [GenerateCommand]
        public void StopAutoUnlock()
        {
            if (_autoUnlockCts == null || _autoUnlockCts.IsCancellationRequested)
            {
                AutoUnlockLog.Add("No active process to stop.");
                return;
            }
            _autoUnlockCts.Cancel();
            log.Info("Stop command issued for auto-unlock process.");
        }

        #endregion

        #region UI and Data Handling

        protected void Refresh()
        {
            if (Achievements == null) return;

            _loading = true;
            _achievementsViewSource = new() { Source = Achievements };
            using (_achievementsViewSource.DeferRefresh())
            {
                _achievementsViewSource.Filter += AchievementFilter;
                _achievementsViewSource.SortDescriptions.Clear();
                _achievementsViewSource.SortDescriptions.Add(new(nameof(SteamAchievement.IsModified), ListSortDirection.Descending));
                _achievementsViewSource.SortDescriptions.Add(new(nameof(SteamAchievement.IsAchieved), ListSortDirection.Ascending));
                _achievementsViewSource.SortDescriptions.Add(new(nameof(SteamAchievement.Name), ListSortDirection.Ascending));
                _achievementsViewSource.LiveFilteringProperties.Clear();
                _achievementsViewSource.LiveFilteringProperties.Add(nameof(SteamAchievement.IsModified));
                _achievementsViewSource.LiveFilteringProperties.Add(nameof(SteamAchievement.IsAchieved));
                _achievementsViewSource.IsLiveFilteringRequested = true;
                _achievementsViewSource.IsLiveSortingRequested = true;
            }
            AchievementsView = _achievementsViewSource.View;
            AchievementsView?.Refresh();
            _loading = false;
        }

        protected void OnManagerIsModifiedChanged()
        {
            IsModified = _statsManager.IsModified;
            if (Achievements == null) return;
            AllowUnlockAll = Achievements.Any(a => !a.IsAchieved);
        }

        private void ManagerAchievementsChanged(SteamStatsManager obj)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Achievements = new(obj.Achievements);
                BindingOperations.EnableCollectionSynchronization(Achievements, syncLock);
            });
        }

        private void OnAchievementModifiedHandler(ObservableCollection<SteamAchievement> arg1, SteamAchievement arg2) => Refresh();

        protected void OnSearchTextChanged()
        {
            if (_loading) return;
            AchievementsView?.Refresh();
        }

        private void OnAchievementsChanged()
        {
            if (Achievements == null) return;
            AllowUnlockAll = Achievements.Any(a => !a.IsAchieved);
            _achievementsPropertyHandler = new ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement>(Achievements)
                .Add(a => a.IsModified, OnAchievementModifiedHandler);
            Refresh();
        }

        private void AchievementFilter(object sender, FilterEventArgs args)
        {
            if (args.Item is not SteamAchievement achievement)
            {
                args.Accepted = false;
                return;
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                if (!achievement.Name.ContainsIgnoreCase(SearchText) && !achievement.Description.ContainsIgnoreCase(SearchText))
                {
                    args.Accepted = false;
                    return;
                }
            }

            args.Accepted = SelectedAchievementFilter switch
            {
                Core.AchievementFilter.Locked => !achievement.IsAchieved,
                Core.AchievementFilter.Unlocked => achievement.IsAchieved,
                Core.AchievementFilter.Modified => achievement.IsModified,
                Core.AchievementFilter.Unmodified => !achievement.IsModified,
                _ => true
            };
        }

        // FIX: Replaced .ForEach() with a standard foreach loop.
        private void OnShowHiddenChanged()
        {
            if (Achievements == null) return;
            foreach (var a in Achievements) a.RefreshDescription(ShowHidden);
        }

        private void OnSelectedAchievementFilterChanged() => AchievementsView?.Refresh();
        private void ManagerStatisticsChanged(SteamStatsManager obj) => Statistics = new(obj.Statistics);

        #endregion
    }
}
