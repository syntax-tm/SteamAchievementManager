using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using DevExpress.Mvvm;
using SAM.API;
using SAM.API.Types;
using SAM.Game;
using SAM.Game.Stats;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core.Stats
{
    public class SteamStatsManager : BindableBase
    {

        private Client _client => SteamClientManager.Default;
        private readonly SAM.API.Callbacks.UserStatsReceived _UserStatsReceivedCallback;
        private readonly Timer _callbackTimer;

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

        public List<SteamStatistic> Statistics
        {
            get => GetProperty(() => Statistics);
            set => SetProperty(() => Statistics, value);
        }
        public List<SteamAchievement> Achievements 
        {
            get => GetProperty(() => Achievements);
            set => SetProperty(() => Achievements, value);
        }
        public List<StatDefinition> StatDefinitions 
        {
            get => GetProperty(() => StatDefinitions);
            set => SetProperty(() => StatDefinitions, value);
        }
        public List<AchievementDefinition> AchievementDefinitions 
        {
            get => GetProperty(() => AchievementDefinitions);
            set => SetProperty(() => AchievementDefinitions, value);
        }

        public SteamStatsManager()
        {
            AppId = SteamClientManager.AppId;

            Statistics = new List<SteamStatistic>();
            Achievements = new List<SteamAchievement>();
            StatDefinitions = new List<StatDefinition>();
            AchievementDefinitions = new List<AchievementDefinition>();

            _UserStatsReceivedCallback = _client.CreateAndRegisterCallback<SAM.API.Callbacks.UserStatsReceived>();
            _UserStatsReceivedCallback.OnRun += OnUserStatsReceived;

            _callbackTimer = new Timer();
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
            Statistics.Clear();
            Achievements.Clear();
            
            Loaded = false;
            IsLoading = true;

            if (_client.SteamUserStats.RequestCurrentStats())
            {
                return;
            }

            MessageBox.Show("Failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnUserStatsReceived(UserStatsReceived param)
        {
            try
            {
                if (param.Result != 1)
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
                MessageBox.Show($"Error when handling stats retrieval:\r\n{e}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loaded = true;
                IsLoading = false;
            }
        }

        public bool LoadSchema()
        {
            string path;

            try
            {
                path = Steam.GetInstallPath();
                path = Path.Combine(path, "appcache");
                path = Path.Combine(path, "stats");
                path = Path.Combine(path, $"UserGameStatsSchema_{AppId}.bin");

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

            var stats = kv[AppId.ToString()]["stats"];

            if (stats.Valid == false || stats.Children == null) return false;

            foreach (var stat in stats.Children.Where(s => s.Valid))
            {
                var rawType = stat["type_int"].Valid
                                  ? stat["type_int"].AsInteger(0)
                                  : stat["type"].AsInteger(0);
                var type = (UserStatType) rawType;
                switch (type)
                {
                    case UserStatType.Invalid:
                    {
                        break;
                    }
                    case UserStatType.Integer:
                    {
                        var id = stat["name"].AsString(string.Empty);
                        var name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        StatDefinitions.Add(new IntegerStatDefinition
                        {
                            Id = stat["name"].AsString(string.Empty),
                            DisplayName = name,
                            MinValue = stat["min"].AsInteger(int.MinValue),
                            MaxValue = stat["max"].AsInteger(int.MaxValue),
                            MaxChange = stat["maxchange"].AsInteger(0),
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            DefaultValue = stat["default"].AsInteger(0),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case UserStatType.Float:
                    case UserStatType.AverageRate:
                    {
                        var id = stat["name"].AsString(string.Empty);
                        var name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                        StatDefinitions.Add(new FloatStatDefinition
                        {
                            Id = stat["name"].AsString(string.Empty),
                            DisplayName = name,
                            MinValue = stat["min"].AsFloat(float.MinValue),
                            MaxValue = stat["max"].AsFloat(float.MaxValue),
                            MaxChange = stat["maxchange"].AsFloat(0.0f),
                            IncrementOnly = stat["incrementonly"].AsBoolean(false),
                            DefaultValue = stat["default"].AsFloat(0.0f),
                            Permission = stat["permission"].AsInteger(0),
                        });
                        break;
                    }

                    case UserStatType.Achievements:
                    case UserStatType.GroupAchievements:
                    {
                        if (stat.Children != null)
                        {
                            foreach (var bits in stat.Children.Where(b => b.Name.EqualsIgnoreCase("bits")))
                            {
                                if (bits.Valid == false || bits.Children == null)
                                {
                                    continue;
                                }

                                foreach (var bit in bits.Children)
                                {
                                    var id = bit["name"].AsString("");
                                    var name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                    var desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                    AchievementDefinitions.Add(new AchievementDefinition
                                    {
                                        Id = id,
                                        Name = name,
                                        Description = desc,
                                        IconNormal = bit["display"]["icon"].AsString(""),
                                        IconLocked = bit["display"]["icon_gray"].AsString(""),
                                        IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                        Permission = bit["permission"].AsInteger(0),
                                    });
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

                var info = SteamAchievementFactory.CreateAchievementInfo(def, isAchieved);
                var achievement = new SteamAchievement(AppId, info, def);

                achievements.Add(achievement);
            }

            Achievements = new List<SteamAchievement>(achievements);
        }

        private void GetStatistics()
        {
            var stats = new List<SteamStatistic>();

            foreach (var statDef in StatDefinitions)
            {
                if (string.IsNullOrEmpty(statDef?.Id)) continue;

                var stat = SteamStatisticFactory.CreateStat(_client, statDef);
                stats.Add(stat);
            }

            Statistics = new List<SteamStatistic>(stats);
        }

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString(string.Empty);
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString(string.Empty);

                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            name = kv.AsString(string.Empty);
            return string.IsNullOrEmpty(name) ? defaultValue : name;
        }

    }
}
