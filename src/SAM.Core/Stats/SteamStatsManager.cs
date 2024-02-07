using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using DevExpress.Mvvm;
using log4net;
using SAM.API;
using SAM.API.Stats;
using SAM.API.Types;
using SAM.Core.Extensions;
using Timer = System.Timers.Timer;
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverQueried.Local

namespace SAM.Core.Stats
{
    public class SteamStatsManager : BindableBase
    {

        private readonly ILog log = LogManager.GetLogger(nameof(SteamStatsManager));

        private static Client _client => SteamClientManager.Default;
        private readonly UserStatsReceived _userStatsReceivedCallback;
        private readonly Timer _callbackTimer;
        private AutoResetEvent _resetEvent;

        private List<ObservableHandler<SteamAchievement>> _achievementHandlers;
        private List<ObservableHandler<SteamStatisticBase>> _statHandlers;

        public uint AppId { get; }
        public bool IsLoading 
        {
            get => GetProperty(() => IsLoading);
            set => SetProperty(() => IsLoading, value);
        }
        public bool Loaded 
        {
            get => GetProperty(() => Loaded);
            set => SetProperty(() => Loaded, value);
        }
        public bool IsModified 
        {
            get => GetProperty(() => IsModified);
            set => SetProperty(() => IsModified, value);
        }
        public bool IsAchievementsModified 
        {
            get => GetProperty(() => IsAchievementsModified);
            set => SetProperty(() => IsAchievementsModified, value, OnItemChanged);
        }
        public bool IsStatsModified 
        {
            get => GetProperty(() => IsStatsModified);
            set => SetProperty(() => IsStatsModified, value, OnItemChanged);
        }
        public List<SteamStatisticBase> Statistics
        {
            get => GetProperty(() => Statistics);
            set => SetProperty(() => Statistics, value);
        }
        public List<SteamAchievement> Achievements 
        {
            get => GetProperty(() => Achievements);
            set => SetProperty(() => Achievements, value);
        }
        public List<StatInfoBase> StatDefinitions 
        {
            get => GetProperty(() => StatDefinitions);
            set => SetProperty(() => StatDefinitions, value);
        }
        public List<AchievementInfo> AchievementDefinitions 
        {
            get => GetProperty(() => AchievementDefinitions);
            set => SetProperty(() => AchievementDefinitions, value);
        }

        public SteamStatsManager()
        {
            AppId = SteamClientManager.AppId;

            Statistics = [];
            Achievements = [];
            StatDefinitions = [];
            AchievementDefinitions = [];

            _userStatsReceivedCallback = _client.CreateAndRegisterCallback<UserStatsReceived>();
            _userStatsReceivedCallback.OnRun += OnUserStatsReceived;

            _callbackTimer = new ();
            _callbackTimer.Elapsed += CallbackTimerOnElapsed;
            _callbackTimer.Interval = 100;
            _callbackTimer.Enabled = true;
        }

        private void CallbackTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _callbackTimer.Enabled = false;
			_client.RunCallbacks(false);
            _callbackTimer.Enabled = true;
        }

        public void RefreshStats()
        {
            _resetEvent ??= new(false);

            Statistics.Clear();
            Achievements.Clear();
            
            Loaded = false;
            IsLoading = true;

            if (_client.SteamUserStats.RequestCurrentStats())
            {
                _resetEvent?.WaitOne();

                return;
            }

            MessageBox.Show("Failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnUserStatsReceived(UserStatsResponse param)
        {
            try
            {
                if (!param.IsSuccess)
                {
                    IsLoading = false;
                    return;
                }

                if (!LoadSchema())
                {
                    IsLoading = false;
                    return;
                }

                GetAchievements();
                GetStatistics();
            }
            catch (Exception e)
            {
                IsLoading = false;

                var message = $"An error occurred handling stats retrieval. {e.Message}";
                log.Error(message, e);

                MessageBox.Show(message, "Steam Stats Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loaded = true;
                IsLoading = false;

                _resetEvent.Set();
            }
        }

        public bool LoadSchema()
        {
            string path;

            try
            {
                path = Steam.GetInstallPath();
                path = Path.Combine(path, @"appcache");
                path = Path.Combine(path, @"stats");
                path = Path.Combine(path, $@"UserGameStatsSchema_{AppId}.bin");

                if (!File.Exists(path)) return false;
            }
            catch
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);

            if (kv == null) return false;

            var currentLanguage = _client.SteamApps008.GetCurrentGameLanguage();

            AchievementDefinitions.Clear();
            StatDefinitions.Clear();

            var stats = kv[AppId.ToString()][@"stats"];

            if (!stats.Valid || stats.Children == null) return false;

            foreach (var stat in stats.Children.Where(s => s.Valid))
            {
                var rawType = stat[@"type_int"].Valid
                            ? stat[@"type_int"].AsInteger()
                            : stat[@"type"].AsInteger();
                var type = (UserStatType) rawType;
                switch (type)
                {
                    case UserStatType.Invalid:
                    {
                        break;
                    }
                    case UserStatType.Integer:
                    {
                        StatDefinitions.Add(new IntegerStatInfo(stat, currentLanguage));
                        break;
                    }
                    case UserStatType.AverageRate:
                    case UserStatType.Float:
                    {
                        StatDefinitions.Add(new FloatStatInfo(stat, currentLanguage));
                        break;
                    }
                    //case UserStatType.AverageRate:
                    //{
                    //    StatDefinitions.Add(new FloatStatInfo(stat, currentLanguage));
                    //    break;
                    //}
                    case UserStatType.Achievements:
                    case UserStatType.GroupAchievements:
                    {
                        if (stat.Children != null)
                        {
                            foreach (var bits in stat.Children.Where(b => b.Name.EqualsIgnoreCase(@"bits")))
                            {
                                if (!bits.Valid || bits.Children == null)
                                {
                                    continue;
                                }

                                foreach (var bit in bits.Children)
                                {
                                    var achievement = UserStatsFactory.Create(bit, currentLanguage);
                                    AchievementDefinitions.Add(achievement);
                                }
                            }
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(rawType), @$"Invalid stat type '{rawType}'.");
                    }
                }
            }

            return true;
        }

        private void GetAchievements()
        {
            var achievements = new List<SteamAchievement>();

            foreach (var def in AchievementDefinitions)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                if (!_client.SteamUserStats.GetAchievementState(def.Id, out var isAchieved)) continue;
                
                def.IsAchieved = isAchieved;
                
                var achievement = new SteamAchievement(AppId, def);

                achievements.Add(achievement);
            }

            Achievements = new (achievements);
            
            var handlers = Achievements.Select(achievement => new ObservableHandler<SteamAchievement>(achievement).Add(a => a.IsModified, OnAchievementChanged));

            _achievementHandlers = [.. handlers];
        }

        private void GetStatistics()
        {
            var stats = new List<SteamStatisticBase>();
            var validDefinitions = StatDefinitions.Where(statDef => !string.IsNullOrEmpty(statDef?.Id));

            foreach (var statDef in validDefinitions)
            {
                var stat = SteamStatisticFactory.CreateStat(_client, statDef);
                stats.Add(stat);
            }

            Statistics = new (stats);
            
            var handlers = Statistics.Select(stat => new ObservableHandler<SteamStatisticBase>(stat).Add(s => s.IsModified, OnStatChanged));

            _statHandlers = [.. handlers];
        }
        
        private void OnItemChanged()
        {
            IsModified = IsAchievementsModified || IsStatsModified;
        }

        private void OnAchievementChanged()
        {
            IsAchievementsModified = Achievements.Any(a => a.IsModified);
        }

        private void OnStatChanged()
        {
            IsStatsModified = Statistics.Any(s => s.IsModified);
        }
    }
}
