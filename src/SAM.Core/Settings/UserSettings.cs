using DevExpress.Mvvm;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SAM.Core.Storage;

namespace SAM.Core.Settings;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UserSettings : BindableBase
{
	private static UserSettings _instance;
	private static readonly object syncLock = new();

	public ManagerSettings ManagerSettings
	{
		get => GetProperty(() => ManagerSettings);
		set => SetProperty(() => ManagerSettings, value);
	}

	public LibrarySettings LibrarySettings
	{
		get => GetProperty(() => LibrarySettings);
		set => SetProperty(() => LibrarySettings, value);
	}

	public static UserSettings Default
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			lock (syncLock)
			{
				_instance = new();
			}

			return _instance;
		}
	}

	protected UserSettings ()
	{
		ManagerSettings = new();

		Load();
	}

	private void Load ()
	{
		if (CacheManager.TryPopulateObject(CacheKeys.UserSettings, this))
		{
			return;
		}

		CacheManager.CacheObject(CacheKeys.UserSettings, this);
	}

	public void Save ()
	{
		CacheManager.CacheObject(CacheKeys.UserSettings, this);
	}
}
