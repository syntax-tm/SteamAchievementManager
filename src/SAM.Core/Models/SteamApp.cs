#pragma warning disable CA1305
#pragma warning disable IDE1006

using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.API;
using SAM.Core.Extensions;
using SAM.Core.Settings;
using SAM.Core.Storage;

namespace SAM.Core;

[GenerateViewModel]
[DebuggerDisplay("{Name} ({Id})")]
public partial class SteamApp : ViewModelBase
{
	protected readonly ILog Log = LogManager.GetLogger(nameof(SteamApp));

	private Process _managerProcess;

	[GenerateProperty] private uint id;
	[GenerateProperty] private string name;
	[GenerateProperty] private GameInfoType gameInfoType;
	public bool IsJunk => GameInfoType == GameInfoType.Junk;
	public bool IsDemo => GameInfoType == GameInfoType.Demo;
	public bool IsNormal => GameInfoType == GameInfoType.Normal;
	public bool IsTool => GameInfoType == GameInfoType.Tool;
	public bool IsMod => GameInfoType == GameInfoType.Mod;
	[GenerateProperty] private bool isLoading;
	[GenerateProperty] private bool loaded;
	[GenerateProperty] private string publisher;
	[GenerateProperty] private string developer;
	[GenerateProperty] private SteamStoreApp storeInfo;
	[GenerateProperty] private Image icon;
	[GenerateProperty] private Image header;
	[GenerateProperty] private Image capsule;
	[GenerateProperty] private string group;
	[GenerateProperty] private bool isHidden;
	[GenerateProperty] private bool isFavorite;
	[GenerateProperty] private bool isMenuOpen;

	public SteamApp (uint id, GameInfoType type)
	{
		Id = id;
		GameInfoType = type;

		Name = SteamClientManager.Default.GetAppName(Id);

		if (string.IsNullOrEmpty(Name))
		{
			return;
		}

		Group = char.IsDigit(Name [0])
			? "#"
			: (Name? [..1]);
	}

	public SteamApp (SupportedApp supportedApp)
		: this(supportedApp.Id, supportedApp.GameInfoType)
	{
	}

	[GenerateCommand]
	public void ManageApp ()
	{
		// TODO: Add a visual indication that the manager is running (handle Exited event)
		if (_managerProcess != null && _managerProcess.SetActive())
		{
			return;
		}

		_managerProcess = SAMHelper.OpenManager(Id);
	}

	[GenerateCommand]
	public void LaunchApp ()
	{
		BrowserHelper.StartApp(Id);
	}

	[GenerateCommand]
	public void InstallApp ()
	{
		BrowserHelper.InstallApp(Id);
	}

	[GenerateCommand]
	public void ViewAchievements ()
	{
		BrowserHelper.ViewAchievements(Id);
	}

	[GenerateCommand]
	public void ViewSteamWorkshop ()
	{
		BrowserHelper.ViewSteamWorkshop(Id);
	}

	[GenerateCommand]
	public void ViewOnSteamDB ()
	{
		BrowserHelper.ViewOnSteamDB(Id);
	}

	[GenerateCommand]
	public void ViewOnSteam ()
	{
		BrowserHelper.ViewOnSteamStore(Id);
	}

	[GenerateCommand]
	public void ViewOnSteamCardExchange ()
	{
		BrowserHelper.ViewOnSteamCardExchange(Id);
	}

	[GenerateCommand]
	public void ViewOnPCGW ()
	{
		BrowserHelper.ViewOnPCGW(Id);
	}

	[GenerateCommand]
	public void CopySteamID ()
	{
		TextCopy.ClipboardService.SetText(Id.ToString());
	}

	[GenerateCommand]
	public void ToggleVisibility ()
	{
		IsHidden = !IsHidden;

		SaveSettings();
	}

	[GenerateCommand]
	public void ToggleFavorite ()
	{
		IsFavorite = !IsFavorite;

		SaveSettings();
	}

	public async Task Load ()
	{
		if (Loaded)
		{
			return;
		}

		try
		{
			IsLoading = true;

			// TODO: SteamApp shouldn't need to configure its cache directory structure
			CacheManager.StorageManager.CreateDirectory($@"apps\{Id}");

			await Task.WhenAll(new []
			{
				Task.Run(LoadStoreInfo),
                // load user preferences (hidden, favorite, etc) for app
                Task.Run(LoadSettings)
			});

			await LoadImages();
		}
		catch (Exception e)
		{
			Log.Error($"An error occurred attempting to load app info for '{Name}' ({Id}). {e.Message}", e);
		}
		finally
		{
			Loaded = true;
			IsLoading = false;
		}
	}

	private void LoadStoreInfo ()
	{
		const int MAX_RETRIES = 3;
		var retryTime = TimeSpan.FromSeconds(30);
		var retries = 0;

		while (StoreInfo == null)
		{
			if (retries > MAX_RETRIES)
			{
				break;
			}

			try
			{
				StoreInfo = SteamworksManager.GetAppInfo(Id);

				if (StoreInfo == null)
				{
					return;
				}

				Publisher = StoreInfo.Publishers.FirstOrDefault();
				Developer = StoreInfo.Developers.FirstOrDefault();
			}
			catch (HttpRequestException hre) when (hre.StatusCode == HttpStatusCode.TooManyRequests)
			{
				var retrySb = new StringBuilder();

				retrySb.Append($"Request for store info on app '{Id}' returned {nameof(HttpStatusCode)} {HttpStatusCode.TooManyRequests} for {nameof(HttpStatusCode.TooManyRequests)}. ");
				retrySb.Append($"Waiting {retryTime.TotalSeconds} second(s) and then retrying...");

				Log.Warn(retrySb);

				Thread.Sleep(retryTime);
			}
			catch (Exception e)
			{
				Log.Error($"An error occurred attempting to load the store info for app {Id}. {e.Message}", e);
				break;
			}
			finally
			{
				retries++;
			}
		}
	}

	public async Task LoadImages ()
	{
		try
		{
			// TODO: Verify that the preferred HeaderImage method is consistent
			// TODO: For each type, loop through sources until one is successful
			if (!string.IsNullOrEmpty(StoreInfo?.HeaderImage))
			{
				// TODO: The Uri file name parsing should be moved to the WebManager
				// TODO: Move image cache key creation to WebManager
				var uri = new Uri(StoreInfo.HeaderImage);
				var fileName = Path.GetFileName(uri.LocalPath);
				var key = CacheKeys.CreateAppImageCacheKey(Id, fileName);

				var storeHeader = await WebManager.DownloadImageAsync(StoreInfo.HeaderImage, key);

				// this assumes that we'll get a header back that we can use
				Header = storeHeader;
			}
			else
			{
				// this should run when Header is null regardless of whether or not
				// the StoreInfo.HeaderImage is null
				var appLogo = SteamClientManager.Default.GetAppLogo(Id);
				if (!string.IsNullOrEmpty(appLogo))
				{
					Header = SteamCdnHelper.DownloadImage(Id, SteamImageType.Logo, appLogo);
				}
			}

			// TODO: Change to be lazy loaded when needed
			var iconName = SteamClientManager.Default.GetAppIcon(Id);
			if (!string.IsNullOrEmpty(iconName))
			{
				Icon = SteamCdnHelper.DownloadImage(Id, SteamImageType.Icon, iconName);
			}
		}
		catch (Exception e)
		{
			var message = $"An error occurred loading images for {Name} ({Id}). {e.Message}";
			Log.Error(message, e);
		}
	}

	private void LoadSettings ()
	{
		var key = CacheKeys.CreateAppSettingsCacheKey(Id);

		if (!CacheManager.TryGetObject<SteamAppSettings>(key, out var settings))
		{
			return;
		}

		IsFavorite = settings.IsFavorite;
		IsHidden = settings.IsHidden;

		Log.Debug($"Loaded {nameof(SteamAppSettings)} {settings}.");
	}

	private void SaveSettings ()
	{
		var key = CacheKeys.CreateAppSettingsCacheKey(Id);
		var settings = new SteamAppSettings
		{
			AppId = Id,
			IsFavorite = IsFavorite,
			IsHidden = IsHidden
		};

		CacheManager.CacheObject(key, settings);

		Log.Debug($"Saving {nameof(SteamAppSettings)} {settings}.");
	}
}
