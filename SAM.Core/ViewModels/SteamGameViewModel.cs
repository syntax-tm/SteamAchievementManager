using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm;
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

        private readonly SteamStatsManager _statsManager;

        public virtual string SearchText { get; set; }
        public virtual bool AllowEdit { get; set; }
        public virtual bool IsModified { get; set; }
        public virtual bool ShowHidden { get; set; }
        public virtual AchievementFilter SelectedAchievementFilter { get; set; }

        public virtual SteamApp SteamApp { get; set; }
        
        public virtual SteamAchievement SelectedAchievement { get; set; }

        public virtual List<SteamStatistic> Statistics { get; set; }
        public virtual List<SteamAchievement> Achievements { get; set; }

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
                .Add(m => m.IsModified, OnManagerIsModifiedChanged);
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

        }

        public void Save()
        {
            SaveAchievements();
            SaveStats();
        }

        public void RefreshStats()
        {
            _statsManager.RefreshStats();

            //SpinWait.SpinUntil(() => _statsManager.Loaded, new TimeSpan(0, 0, 30));
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

        protected void OnManagerIsModifiedChanged()
        {
            IsModified = _statsManager.IsModified;
        }

        private void ManagerAchievementsChanged(SteamStatsManager obj)
        {
            Achievements = new (obj.Achievements);

            AchievementsView = (CollectionView) CollectionViewSource.GetDefaultView(Achievements);
            AchievementsView.Filter = AchievementFilter;
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
