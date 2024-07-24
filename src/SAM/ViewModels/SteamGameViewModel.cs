using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.Native;
using JetBrains.Annotations;
using log4net;
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
    private readonly object syncLock = new ();
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

    [GenerateProperty] private ICollectionView achievementsView;

    public SteamGameViewModel()
    {

    }

    public SteamGameViewModel(SteamApp steamApp)
    {
        SteamApp = steamApp;

        _statsManager = new (SteamClientManager.Default);

        statsHandler = new ObservableHandler<SteamStatsManager>(_statsManager)
                       .AddAndInvoke(m => m.Achievements, ManagerAchievementsChanged)
                       .AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged)
                       .AddAndInvoke(m => m.IsModified, OnManagerIsModifiedChanged);
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

                log.Info($"Successfully saved achievement {achievement.Id}.");

                achievement.CommitChanges();

                saved++;
            }

            stats.StoreStats();

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
                    IntegerSteamStatistic intStat         => stats.SetStatValue(stat.Id, intStat.Value),
                    AverageRateSteamStatistic avgRateStat => stats.UpdateAvgRateStat(stat.Id, avgRateStat.AvgRateNumerator, avgRateStat.AvgRateDenominator),
                    FloatSteamStatistic floatStat         => stats.SetStatValue(stat.Id, floatStat.Value),
                    _                                     => throw new InvalidOperationException($"Unknown stat type {stat.StatType} is modified and cannot be saved.")
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

            stats.StoreStats();

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
    public void Save()
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

        var achievementMessage = achievementsSaved switch
        {
            0 => string.Empty,
            1 => $"{achievementsSaved} achievement",
            _ => $"{achievementsSaved} achievements"
        };

        var statsMessage = statsSaved switch
        {
            0 => string.Empty,
            1 => $"{statsSaved} stat",
            _ => $"{statsSaved} stats"
        };

        message.Append("Successfully saved ");
        message.Append(achievementMessage);

        if (achievementsSaved > 0 && statsSaved > 0)
        {
            message.Append(" and ");
        }

        message.Append(statsMessage);
        message.Append(".");

        MessageBox.Show(message.ToString(), "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
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

        _achievementsViewSource = new ()
        {
            Source = Achievements
        };

        using (_achievementsViewSource.DeferRefresh())
        {
            _achievementsViewSource.Filter += AchievementFilter;

            _achievementsViewSource.SortDescriptions.Clear();
            _achievementsViewSource.SortDescriptions.Add(new (nameof(SteamAchievement.IsModified), ListSortDirection.Descending));
            _achievementsViewSource.SortDescriptions.Add(new (nameof(SteamAchievement.IsAchieved), ListSortDirection.Ascending));
            _achievementsViewSource.SortDescriptions.Add(new (nameof(SteamAchievement.Name), ListSortDirection.Ascending));

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
            Achievements = new (obj.Achievements);

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

        // if we have search text that was entered
        if (!string.IsNullOrEmpty(SearchText))
        {
            // if it's not a match on the name or description then filter it out
            if (!achievement.Name.ContainsIgnoreCase(SearchText)
                && !achievement.Description.ContainsIgnoreCase(SearchText))
            {
                args.Accepted = false;
                return;
            }
        }

        var accepted = SelectedAchievementFilter switch
        {
            Core.AchievementFilter.Locked     => !achievement.IsAchieved,
            Core.AchievementFilter.Unlocked   => achievement.IsAchieved,
            Core.AchievementFilter.Modified   => achievement.IsModified,
            Core.AchievementFilter.Unmodified => !achievement.IsModified,
            Core.AchievementFilter.All        => true,
            _                                 => true
        };

        args.Accepted = accepted;
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
        Statistics = new (obj.Statistics);
    }
}
