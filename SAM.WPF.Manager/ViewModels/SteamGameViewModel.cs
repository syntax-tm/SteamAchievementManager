using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                .AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged);
        }

        public static SteamGameViewModel Create()
        {
            return ViewModelSource.Create(() => new SteamGameViewModel());
        }

        public static SteamGameViewModel Create(SteamApp steamApp)
        {
            return ViewModelSource.Create(() => new SteamGameViewModel(steamApp));
        }

        public void RefreshStats()
        {
            _statsManager.RefreshStats();

            SpinWait.SpinUntil(() => _statsManager.Loaded, new TimeSpan(0, 0, 30));
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
