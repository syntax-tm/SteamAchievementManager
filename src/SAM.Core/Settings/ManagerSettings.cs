using DevExpress.Mvvm;

namespace SAM.Core.Settings
{
	public class ManagerSettings : BindableBase
	{
		/// <summary>
		/// Whether or not to show the description for achievements marked "hidden" that are
		/// not unlocked.
		/// </summary>
		public bool ShowHidden
		{
			get => GetProperty(() => ShowHidden);
			set => SetProperty(() => ShowHidden, value);
		}

		public bool GroupAchievements
		{
			get => GetProperty(() => GroupAchievements);
			set => SetProperty(() => GroupAchievements, value);
		}
	}
}
