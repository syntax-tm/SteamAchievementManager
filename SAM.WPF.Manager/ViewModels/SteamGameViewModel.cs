using System;
using System.Collections.Generic;
using System.Threading;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using SAM.WPF.Core;
using SAM.WPF.Core.Stats;

namespace SAM.WPF.Manager.ViewModels
{
    public class SteamGameViewModel
    {
        public virtual ICurrentWindowService CurrentWindow { get { return null; } }

        private ObservableHandler<SteamStatsManager> _statsHandler;

        private readonly SteamStatsManager _statsManager;

        public virtual bool AllowEdit { get; set; }
        public virtual bool IsModified { get; set; }
        public virtual AchievementFilter SelectedAchievementFilter { get; set; }

        public virtual SteamApp SteamApp { get; set; }
        
        public virtual List<SteamStatistic> Statistics { get; set; }
        public virtual List<SteamAchievement> Achievements { get; set; }

        protected SteamGameViewModel()
        {

        }
        
        protected SteamGameViewModel(SteamApp steamApp)
        {
            SteamApp = steamApp;
            
            _statsManager = new SteamStatsManager();

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

            SpinWait.SpinUntil(() => _statsManager.Loaded, new TimeSpan(0, 0, 30));
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
            Achievements = new List<SteamAchievement>(obj.Achievements);
        }
        
        private void ManagerStatisticsChanged(SteamStatsManager obj)
        {
            Statistics = new List<SteamStatistic>(obj.Statistics);
        }
    }
}
