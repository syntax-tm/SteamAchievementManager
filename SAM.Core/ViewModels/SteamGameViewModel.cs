using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using JetBrains.Annotations;
using log4net;
using SAM.Core.Stats;

namespace SAM.Core.ViewModels
{
    public class SteamGameViewModel
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(SteamGameViewModel));

        public virtual ICurrentWindowService CurrentWindow { get { return null; } }

        [UsedImplicitly]
        private ObservableHandler<SteamStatsManager> _statsHandler;
        
        [UsedImplicitly]
        private ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement> _achievementsPropertyHandler;
        
        private readonly SteamStatsManager _statsManager;

        public virtual string SearchText { get; set; }
        public virtual bool AllowUnlockAll { get; set; } = true;
        public virtual bool AllowEdit { get; set; }
        public virtual bool IsModified { get; set; }
        public virtual bool ShowHidden { get; set; }
        public virtual AchievementFilter SelectedAchievementFilter { get; set; }

        public virtual SteamApp SteamApp { get; set; }
        
        public virtual SteamAchievement SelectedAchievement { get; set; }

        public virtual ObservableCollection<SteamStatisticBase> Statistics { get; set; }
        public virtual ObservableCollection<SteamAchievement> Achievements { get; set; }

        public virtual CollectionView AchievementsView { get; set; }

        protected SteamGameViewModel()
        {

        }
        
        protected SteamGameViewModel(SteamApp steamApp)
        {
            SteamApp = steamApp;
            
            _statsManager = new ();

            _statsHandler = new ObservableHandler<SteamStatsManager>(_statsManager)
                .AddAndInvoke(m => m.Achievements, ManagerAchievementsChanged)
                .AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged)
                .AddAndInvoke(m => m.IsModified, OnManagerIsModifiedChanged);
        }

        public static SteamGameViewModel Create()
        {
            return ViewModelSource.Create(() => new SteamGameViewModel());
        }

        public static SteamGameViewModel Create(SteamApp steamApp)
        {
            return ViewModelSource.Create(() => new SteamGameViewModel(steamApp));
        }

        public void SaveAchievements()
        {
            try
            {
                var modified = Achievements.Where(a => a.IsModified).ToList();
                if (!modified.Any())
                {
                    log.Info("User achievements have not been modified. Skipping save.");
                    return;
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
                }

                stats.StoreStats();
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to save achievements. {e.Message}";

                log.Error(message, e);

                MessageBox.Show(message, "Error Updating Achievements", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveStats()
        {
            try
            {
                var modified = Statistics.Where(a => a.IsModified).ToList();
                if (!modified.Any())
                {
                    log.Info("User stats have not been modified. Skipping save.");
                    return;
                }

                var stats = SteamClientManager.Default.SteamUserStats;

                foreach (var stat in modified)
                {
                    bool result;
                    if (stat.IsInteger)
                    {
                        result = stats.SetStatValue(stat.Id, (int) stat.GetValue());
                    }
                    else if (stat.IsFloat)
                    {
                        result = stats.SetStatValue(stat.Id, (float) stat.GetValue());
                    }
                    else if (stat.IsAverageRate)
                    {
                        var avgRate = (FloatSteamStatistic) stat;
                        result = stats.UpdateAvgRateStat(stat.Id, avgRate.AvgRateNumerator, avgRate.AvgRateDenominator);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unknown stat type {stat.StatType} is modified and cannot be saved.");
                    }

                    if (!result)
                    {
                        var message = $"Failed to update {stat.StatType} stat {stat.Id}.";

                        throw new SAMException(message);
                    }

                    log.Info($"Successfully saved {stat.StatType} stat {stat.Id}.");

                    stat.CommitChanges();
                }

                stats.StoreStats();
            }
            catch (Exception e)
            {
                var message = $"An error occurred attempting to save stats. {e.Message}";

                log.Error(message, e);

                MessageBox.Show(message, "Error Updating Stats", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Save()
        {
            SaveAchievements();
            SaveStats();
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
        
        public void Reset()
        {
            ResetAchievements();
            ResetStats();
        }

        public void LockAllAchievements()
        {
            Achievements.ForEach(a => a.Lock());
        }

        public void UnlockAllAchievements()
        {
            Achievements.ForEach(a => a.Unlock());
        }

        protected void Refresh()
        {
            if (!Achievements.Any()) return;

            AllowUnlockAll = Achievements.Any(a => !a.IsAchieved);
        }

        protected void OnManagerIsModifiedChanged()
        {
            IsModified = _statsManager.IsModified;

            Refresh();
        }

        private void ManagerAchievementsChanged(SteamStatsManager obj)
        {
            Achievements = new (obj.Achievements);

            AchievementsView = (CollectionView) CollectionViewSource.GetDefaultView(Achievements);
            AchievementsView.Filter = AchievementFilter;

            _achievementsPropertyHandler = new ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement>(Achievements)
                .Add(a => a.IsModified, OnAchievementModifiedHandler);
        }

        private void OnAchievementModifiedHandler(ObservableCollection<SteamAchievement> arg1, SteamAchievement arg2)
        {
            Refresh();
        }

        private bool AchievementFilter(object obj)
        {
            if (obj is not SteamAchievement achievement)
            {
                throw new InvalidOperationException($"{nameof(obj)} must be of type {nameof(SteamAchievement)}.");
            }

            return true;
        }

        private void ManagerStatisticsChanged(SteamStatsManager obj)
        {
            Statistics = new (obj.Statistics);
        }
    }
}
