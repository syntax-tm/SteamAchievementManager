#pragma warning disable CA1305

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
using SAM.Core.Extensions;
using SAM.Core.Stats;

namespace SAM.Core.ViewModels;

[GenerateViewModel(ImplementISupportServices = true)]
public partial class SteamGameViewModel
{
	protected readonly ILog Log = LogManager.GetLogger(nameof(SteamGameViewModel));

	public virtual ICurrentWindowService CurrentWindow => GetService<ICurrentWindowService>();

	[UsedImplicitly]
	private readonly ObservableHandler<SteamStatsManager> statsHandler;

	[UsedImplicitly]
	private ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement> _achievementsPropertyHandler;

	// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
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

	public SteamGameViewModel ()
	{

	}

	public SteamGameViewModel (SteamApp steamApp)
	{
		SteamApp = steamApp;

		_statsManager = new();

		statsHandler = new ObservableHandler<SteamStatsManager>(_statsManager)
			.AddAndInvoke(m => m.Achievements, ManagerAchievementsChanged)
			.AddAndInvoke(m => m.Statistics, ManagerStatisticsChanged)
			.AddAndInvoke(m => m.IsModified, OnManagerIsModifiedChanged);
	}

	public int SaveAchievements ()
	{
		var saved = 0;
		try
		{
			var modified = Achievements!.Where(a => a.IsModified).ToList();
			if (modified.Count == 0)
			{
				Log.Info("User achievements have not been modified. Skipping save.");
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

				Log.Info($"Successfully saved achievement {achievement.Id}.");

				achievement.CommitChanges();

				saved++;
			}

			stats.StoreStats();

			return saved;
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to save achievements. {e.Message}";

			Log.Error(message, e);

			MessageBox.Show(message, "Error Updating Achievements", MessageBoxButton.OK, MessageBoxImage.Error);

			return -1;
		}
	}

	public int SaveStats ()
	{
		var saved = 0;
		try
		{
			var modified = Statistics!.Where(a => a.IsModified).ToList();
			if (modified.Count == 0)
			{
				Log.Info("User stats have not been modified. Skipping save...");
				return 0;
			}

			var stats = SteamClientManager.Default.SteamUserStats;

			foreach (var stat in modified)
			{
				bool result;
				if (stat is IntegerSteamStatistic intStat)
				{
					result = stats.SetStatValue(stat.Id, intStat.Value);
				}
				else if (stat is AverageRateSteamStatistic avgRateStat)
				{
					result = stats.UpdateAvgRateStat(stat.Id, avgRateStat.AvgRateNumerator, avgRateStat.AvgRateDenominator);
				}
				else if (stat is FloatSteamStatistic floatStat)
				{
					result = stats.SetStatValue(stat.Id, floatStat.Value);
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

				Log.Info($"Successfully saved {stat.StatType} stat {stat.Id}.");

				stat.CommitChanges();

				saved++;
			}

			stats.StoreStats();

			return saved;
		}
		catch (Exception e)
		{
			var message = $"An error occurred attempting to save stats. {e.Message}";

			Log.Error(message, e);

			MessageBox.Show(message, "Error Updating Stats", MessageBoxButton.OK, MessageBoxImage.Error);

			return -1;
		}
	}

	[GenerateCommand]
	public void Save ()
	{
		var achievementsSaved = SaveAchievements();
		if (achievementsSaved == -1)
		{
			Log.Warn($"{nameof(SaveAchievements)} encountered an error.");

			return;
		}

		var statsSaved = SaveStats();
		if (statsSaved == -1)
		{
			Log.Warn($"{nameof(SaveStats)} encountered an error.");

			return;
		}

		var message = new StringBuilder();

		var achievementMessage = achievementsSaved switch
		{
			1 => $"{achievementsSaved} achievement",
			_ => $"{achievementsSaved} achievements"
		};

		var statsMessage = statsSaved switch
		{
			1 => $"{statsSaved} stat",
			_ => $"{statsSaved} stats"
		};

		message.Append($"Successfully saved {achievementMessage} and {statsMessage}.");

		MessageBox.Show(message.ToString(), "Achievements and Stats Saved", MessageBoxButton.OK, MessageBoxImage.Information);
	}

	public void RefreshStats ()
	{
		_statsManager.RefreshStats();
	}

	public void ResetAchievements ()
	{
		Achievements.ForEach(a => a.Reset());
	}

	public void ResetStats ()
	{
		Statistics.ForEach(s => s.Reset());
	}

	[GenerateCommand]
	public void Reset ()
	{
		ResetAchievements();
		ResetStats();
	}

	[GenerateCommand]
	public void LockAllAchievements ()
	{
		Achievements.ForEach(a => a.Lock());
	}

	[GenerateCommand]
	public void UnlockAllAchievements ()
	{
		Achievements.ForEach(a => a.Unlock());
	}

	protected void Refresh ()
	{
		if (!Achievements!.Any())
		{
			return;
		}

		AllowUnlockAll = Achievements.Any(a => !a.IsAchieved);
	}

	protected void OnManagerIsModifiedChanged ()
	{
		IsModified = _statsManager.IsModified;

		Refresh();
	}

	private void ManagerAchievementsChanged (SteamStatsManager obj)
	{
		Achievements = new(obj.Achievements);

		AchievementsView = (CollectionView) CollectionViewSource.GetDefaultView(Achievements);
		AchievementsView!.Filter = AchievementFilter;
		AchievementsView.SortDescriptions.Add(new(nameof(SteamAchievement.IsModified), ListSortDirection.Descending));
		AchievementsView.SortDescriptions.Add(new(nameof(SteamAchievement.IsAchieved), ListSortDirection.Ascending));
		AchievementsView.SortDescriptions.Add(new(nameof(SteamAchievement.Name), ListSortDirection.Ascending));

		_achievementsPropertyHandler = new ObservableCollectionPropertyHandler<ObservableCollection<SteamAchievement>, SteamAchievement>(Achievements)
			.Add(a => a.IsModified, OnAchievementModifiedHandler);
	}

	private void OnAchievementModifiedHandler (ObservableCollection<SteamAchievement> arg1, SteamAchievement arg2)
	{
		Refresh();
	}

	protected void OnSearchTextChanged ()
	{
		AchievementsView!.Refresh();
	}

	private bool AchievementFilter (object obj)
	{
		if (obj is not SteamAchievement achievement)
		{
			throw new InvalidOperationException($"{nameof(obj)} must be of type {nameof(SteamAchievement)}.");
		}

		if (string.IsNullOrEmpty(SearchText))
		{
			return true;
		}

		if (achievement.Name.ContainsIgnoreCase(SearchText)
			|| achievement.Description.ContainsIgnoreCase(SearchText))
		{
			return true;
		}

		return false;
	}

	private void OnShowHiddenChanged ()
	{
		Achievements.ForEach(a => a.RefreshDescription(ShowHidden));
	}

	private void OnAchievementsChanged ()
	{
		Refresh();
	}

	private void ManagerStatisticsChanged (SteamStatsManager obj)
	{
		Statistics = new(obj.Statistics);
	}
}
