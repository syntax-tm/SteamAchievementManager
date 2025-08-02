using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.Native;
using JetBrains.Annotations;
using log4net;
using Microsoft.Win32;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Managers;
using SAM.Stats;

namespace SAM.ViewModels;

[GenerateViewModel(ImplementISupportServices = true)]
public partial class SteamGameViewModel
{
    protected readonly ILog log = LogManager.GetLogger(nameof(SteamGameViewModel));

    public virtual ICurrentWindowService CurrentWindow => GetService<ICurrentWindowService>();

    private bool _loading = true;
    private readonly object syncLock = new();
    private CollectionViewSource _achievementsViewSource;

    [UsedImplicitly]
    private readonly ObservableHandler<SteamStatsManager> statsHandler;

    [UsedImplicitly]
    private ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement> _achievementsPropertyHandler;

    // ReSharper disable once InconsistentNaming
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

    // Properties for Auto-Unlock Tab
    [GenerateProperty] private int selectedTabIndex;
    [GenerateProperty] private string currentUnlockStatus;
    [GenerateProperty] private string nextUnlockCountdown;
    [GenerateProperty] private ObservableCollection<string> autoUnlockLog;

    [GenerateProperty] private ICollectionView achievementsView;

    public SteamGameViewModel()
    {

    }

    public SteamGameViewModel(SteamApp steamApp)
    {
        SteamApp = steamApp;

        _statsManager = new(SteamClientManager.Default);

        statsHandler = new ObservableHandler<SteamStatsManager>(_statsManager)
                       .AddAndInvoke(m => m.Achievements, ManagerAchievementsChanged)
                       .AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged)
                       .AddAndInvoke(m => m.IsModified, OnManagerIsModifiedChanged);

        // Initialize Auto-Unlock properties
        AutoUnlockLog = new();
        BindingOperations.EnableCollectionSynchronization(AutoUnlockLog, syncLock);
        CurrentUnlockStatus = "Idle. Start the process to see live status.";
        NextUnlockCountdown = "N/A";
    }

    public int SaveAchievements()
    {
        var saved = 0;
        try
        {
            var modified = Achievements!.Where(a => a.IsModified).ToList();
            if (!modified.Any())
            {
                log.Info("User achievements have not been modified. Skipping save.");
                return 0;
            }

            var stats = SteamClientManager.Default.SteamUserStats;

            foreach (var achievement in modified)
            {
                var result = stats.SetAchievement(achievement.Id, achievement.IsAchieved);
                if (!result)
                {
                    var message = $"Failed to update achievement {achievement.Id}.";

                    throw new SAMException(message);
                }

                log.Info($"Successfully set achievement {achievement.Id}. Now storing stats...");

                // Store stats after each achievement change to ensure it's written
                if (!stats.StoreStats())
                {
                    throw new SAMException($"Failed to store stats after updating achievement {achievement.Id}.");
                }

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
            var modified = Statistics!.Where(a => a.IsModified).ToList();
            if (!modified.Any())
            {
                log.Info("User stats have not been modified. Skipping save...");
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

                if (!result)
                {
                    var message = $"Failed to update {stat.StatType} stat {stat.Id}.";
                    throw new SAMException(message);
                }

                log.Info($"Successfully saved {stat.StatType} stat {stat.Id}.");
                stat.CommitChanges();
                saved++;
            }

            if (!stats.StoreStats())
            {
                throw new SAMException("Failed to store stats after updating one or more statistics.");
            }

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

    [GenerateCommand]
    public async void AutoUnlock()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Select an Unlock Schedule File"
        };

        if (openFileDialog.ShowDialog() != true) return;

        SelectedTabIndex = 2;

        AutoUnlockLog.Clear();
        CurrentUnlockStatus = "Preparing to start...";
        NextUnlockCountdown = "Reading file...";

        try
        {
            AutoUnlockLog.Add($"Reading schedule file: {openFileDialog.FileName}");

            using (var reader = new System.IO.StreamReader(openFileDialog.FileName))
            {
                await reader.ReadLineAsync(); // Skip header line

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split('|');
                    if (parts.Length != 2)
                    {
                        AutoUnlockLog.Add($"Skipping malformed line: {line}");
                        continue;
                    }

                    var name = parts[0].Trim();
                    if (!TimeSpan.TryParse(parts[1].Trim(), out var delay))
                    {
                        AutoUnlockLog.Add($"Could not parse delay for '{name}'. Skipping.");
                        continue;
                    }

                    var achievement = Achievements.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (achievement == null)
                    {
                        AutoUnlockLog.Add($"Achievement '{name}' not found. Skipping.");
                        continue;
                    }

                    if (achievement.IsAchieved)
                    {
                        AutoUnlockLog.Add($"Achievement '{name}' is already unlocked. Skipping.");
                        continue;
                    }

                    AutoUnlockLog.Add($"Queueing '{name}' for unlock with a delay of {delay}.");
                    CurrentUnlockStatus = $"Waiting to unlock: {name}";

                    var countdown = delay;
                    while (countdown.TotalSeconds > 0)
                    {
                        NextUnlockCountdown = $"Time until next unlock: {countdown:hh\\:mm\\:ss}";
                        await Task.Delay(1000);
                        countdown = countdown.Subtract(TimeSpan.FromSeconds(1));
                    }

                    NextUnlockCountdown = "Unlocking now...";
                    CurrentUnlockStatus = $"Unlocking & Saving: {name}";

                    achievement.Unlock();

                    // Save the single achievement immediately
                    if (SaveSingleAchievement(achievement))
                    {
                        AutoUnlockLog.Add($"Successfully unlocked and saved: {name}");
                    }
                    else
                    {
                        AutoUnlockLog.Add($"FAILED to save achievement: {name}. It is unlocked in the UI but not on Steam. See error log for details.");
                        CurrentUnlockStatus = $"Error saving {name}. See log.";
                        NextUnlockCountdown = "Process halted on error.";
                        MessageBox.Show($"Failed to save achievement: {name}. The process will stop. Check the log for details.", "Auto Unlock Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Stop the process on failure
                    }
                }
            }

            AutoUnlockLog.Add("Auto-unlock process completed successfully.");
            CurrentUnlockStatus = "Process complete.";
            NextUnlockCountdown = "Idle.";

            MessageBox.Show("Auto unlock process completed.", "Auto Unlock Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            var message = $"A critical error occurred during the auto unlock process: {ex.Message}";
            log.Error(message, ex);
            AutoUnlockLog.Add($"FATAL ERROR: {message}");
            CurrentUnlockStatus = "A critical error occurred. Check log.";
            NextUnlockCountdown = "Process halted.";
            MessageBox.Show(message, "Auto Unlock Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // NEW: Helper method to save a single achievement and its stats immediately.
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
        if (achievementsSaved == -1)
        {
            log.Warn($"{nameof(SaveAchievements)} encountered an error.");
            return;
        }

        var statsSaved = SaveStats();
        if (statsSaved == -1)
        {
            log.Warn($"{nameof(SaveStats)} encountered an error.");
            return;
        }

        var message = new StringBuilder();
        var achievementMessage = achievementsSaved switch { 0 => string.Empty, 1 => $"{achievementsSaved} achievement", _ => $"{achievementsSaved} achievements" };
        var statsMessage = statsSaved switch { 0 => string.Empty, 1 => $"{statsSaved} stat", _ => $"{statsSaved} stats" };

        message.Append("Successfully saved ");
        message.Append(achievementMessage);
        if (achievementsSaved > 0 && statsSaved > 0) message.Append(" and ");
        message.Append(statsMessage);
        message.Append(".");

        if (displayResult)
        {
            MessageBox.Show(message.ToString(), "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        log.Info(message);
    }

    public void RefreshStats()
    {
        _statsManager.RefreshStats();
    }

    public void ResetAchievements()
    {
        Achievements.ForEach(a => a.Reset());
    }

    public void ResetStats()
    {
        Statistics.ForEach(s => s.Reset());
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
        Achievements.ForEach(a => a.Lock());
    }

    [GenerateCommand]
    public void UnlockAllAchievements()
    {
        Achievements.ForEach(a => a.Unlock());
    }

    protected void Refresh()
    {
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
            _achievementsViewSource.IsLiveGroupingRequested = false;
        }
        AchievementsView = _achievementsViewSource.View;
        AchievementsView?.Refresh();
        _loading = false;
    }

    protected void OnManagerIsModifiedChanged()
    {
        IsModified = _statsManager.IsModified;
        if (Achievements == null) return;
        AllowUnlockAll = Achievements!.Any(a => !a.IsAchieved);
    }

    private void ManagerAchievementsChanged(SteamStatsManager obj)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Achievements = new(obj.Achievements);
            BindingOperations.EnableCollectionSynchronization(Achievements, syncLock);
        });
    }

    private void OnAchievementModifiedHandler(ObservableCollection<SteamAchievement> arg1, SteamAchievement arg2)
    {
        Refresh();
    }

    protected void OnSearchTextChanged()
    {
        if (_loading) return;
        AchievementsView?.Refresh();
    }

    private void OnAchievementsChanged()
    {
        AllowUnlockAll = Achievements!.Any(a => !a.IsAchieved);
        _achievementsPropertyHandler = new ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement>(Achievements)
            .Add(a => a.IsModified, OnAchievementModifiedHandler);
        Refresh();
    }

    private void AchievementFilter(object sender, FilterEventArgs args)
    {
        var obj = args.Item;
        if (obj is not SteamAchievement achievement)
        {
            throw new InvalidOperationException($"{nameof(obj)} must be of type {nameof(SteamAchievement)}.");
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
            Core.AchievementFilter.All => true,
            _ => true
        };
    }

    private void OnShowHiddenChanged()
    {
        Achievements.ForEach(a => a.RefreshDescription(ShowHidden));
    }

    private void OnSelectedAchievementFilterChanged()
    {
        AchievementsView?.Refresh();
    }

    private void ManagerStatisticsChanged(SteamStatsManager obj)
    {
        Statistics = new(obj.Statistics);
    }
}
