using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.API;
using SAM.Core.Extensions;
using SAM.Core.Messages;
using SAM.Core.Settings;
using SAM.Core.Storage;

namespace SAM.Core;

[GenerateViewModel]
[DebuggerDisplay("{Name} ({Id})")]
public partial class SteamApp : ViewModelBase
{
    protected readonly ILog log = LogManager.GetLogger(nameof(SteamApp));

    private Process _managerProcess;

    [GenerateProperty] private uint _id;
    [GenerateProperty] private string _name;
    [GenerateProperty] private GameInfoType _gameInfoType;
    public bool IsJunk => GameInfoType == GameInfoType.Junk;
    public bool IsDemo => GameInfoType == GameInfoType.Demo;
    public bool IsNormal => GameInfoType == GameInfoType.Normal;
    public bool IsTool => GameInfoType == GameInfoType.Tool;
    public bool IsMod => GameInfoType == GameInfoType.Mod;
    [GenerateProperty] private bool _isLoading;
    [GenerateProperty] private bool _loaded;
    [GenerateProperty] private string _publisher;
    [GenerateProperty] private string _developer;
    [GenerateProperty] private SteamStoreApp _storeInfo;
    [GenerateProperty] private Image _icon;
    [GenerateProperty] private Image _header;
    [GenerateProperty] private Image _capsule;
    [GenerateProperty] private Image _logo;
    [GenerateProperty] private string _group;
    [GenerateProperty] private bool _isHidden;
    [GenerateProperty] private bool _isFavorite;
    [GenerateProperty] private bool _isMenuOpen;
    public bool StoreInfoLoaded => StoreInfo != null;

    public SteamApp(uint id, GameInfoType type = GameInfoType.Normal)
    {
        Id = id;
        GameInfoType = type;
            
        Name = SteamClientManager.Default.GetAppName(Id);

        if (string.IsNullOrEmpty(Name)) return;

        Group = char.IsDigit(Name[0])
            ? "#"
            : (Name?.Substring(0, 1));
    }
        
    public SteamApp(SupportedApp supportedApp) 
        : this(supportedApp.Id, supportedApp.GameInfoType)
    {
    }

    [GenerateCommand]
    public void ManageApp()
    {
        // TODO: Add a visual indication that the manager is running (handle Exited event)
        if (_managerProcess != null && _managerProcess.SetActive()) return;
        _managerProcess = SAMHelper.OpenManager(Id);
    }
        
    [GenerateCommand]
    public void LaunchApp()
    {
        BrowserHelper.StartApp(Id);
    }
        
    [GenerateCommand]
    public void InstallApp()
    {
        BrowserHelper.InstallApp(Id);
    }
        
    [GenerateCommand]
    public void ViewAchievements()
    {
        BrowserHelper.ViewAchievements(Id);
    }
        
    [GenerateCommand]
    public void ViewSteamWorkshop()
    {
        BrowserHelper.ViewSteamWorkshop(Id);
    }
        
    [GenerateCommand]
    public void ViewOnSteamDB()
    {
        BrowserHelper.ViewOnSteamDB(Id);
    }
        
    [GenerateCommand]
    public void ViewOnSteam()
    {
        BrowserHelper.ViewOnSteamStore(Id);
    }

    [GenerateCommand]
    public void ViewOnSteamGridDB()
    {
        BrowserHelper.ViewOnSteamGridDB(Id);
    }
        
    [GenerateCommand]
    public void ViewOnSteamCardExchange()
    {
        BrowserHelper.ViewOnSteamCardExchange(Id);
    }
        
    [GenerateCommand]
    public void ViewOnPCGW()
    {
        BrowserHelper.ViewOnPCGW(Id);
    }
        
    [GenerateCommand]
    public void CopySteamID()
    {
        TextCopy.ClipboardService.SetText(Id.ToString());
    }
        
    [GenerateCommand]
    public void ToggleVisibility()
    {
        IsHidden = !IsHidden;

        SaveSettings();

        Messenger.Default.SendRequest(EntityType.Library, RequestType.Refresh);
    }
        
    [GenerateCommand]
    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
        SaveSettings();
        
        Messenger.Default.SendRequest(EntityType.Library, RequestType.Refresh);
    }

    public async Task Load()
    {
        if (Loaded) return;

        try
        {
            IsLoading = true;

            // TODO: SteamApp shouldn't need to configure its cache directory structure
            CacheManager.StorageManager.CreateDirectory($@"apps\{Id}");

            await Task.WhenAll([
                Task.Run(LoadImagesAsync),
                // load user preferences (hidden, favorite, etc) for app
                Task.Run(LoadSettings)
            ]).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load app info for '{Name}' ({Id}). {e.Message}", e);
        }
        finally
        {
            Loaded = true;
            IsLoading = false;
        }
    }

    public async Task LoadImagesAsync()
    {
        try
        {
            // try and load the user's grid image for the app first
            if (SteamClientManager.TryGetCachedAppImage(Id, SteamImageType.GridLandscape, out var gridHeader))
            {
                Header = gridHeader;

                return;
            }

            // if they don't have a grid image then try and use the default header
            if (SteamClientManager.TryGetCachedAppImage(Id, SteamImageType.Header, out var appHeader))
            {
                Header = appHeader;

                return;
            }
            
            // if there's no default header either, then see if there's a logo we can use
            if (SteamClientManager.TryGetCachedAppImage(Id, SteamImageType.Logo, out var appLogo))
            {
                log.Info($"Using {nameof(SteamImageType.Logo)} for app id {Id}.");

                Header = Logo = appLogo;

                return;
            }

            // if we don't have any cached header or logo image to use, then request the store info
            // refresh so that we can download a header
            SteamworksManager.LoadStoreInfo(this);

            await Task.CompletedTask.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var message = $"An error occurred loading images for {Name} ({Id}). {e.Message}";
            log.Error(message, e);
        }
    }

    private void LoadSettings()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);

        if (!CacheManager.TryGetObject<SteamAppSettings>(key, out var settings))
        {
            return;
        }

        IsFavorite = settings.IsFavorite;
        IsHidden = settings.IsHidden;

        log.Debug($"Loaded {nameof(SteamAppSettings)} {settings}.");
    }

    private void SaveSettings()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);
        var settings = new SteamAppSettings
        {
            AppId = Id,
            IsFavorite = IsFavorite,
            IsHidden = IsHidden
        };

        CacheManager.CacheObject(key, settings);
            
        log.Debug($"Saving {nameof(SteamAppSettings)} {settings}.");
    }

    protected void OnStoreInfoChanged()
    {
        // we didn't have a header OR a logo in the cache, so try downloading the header first
        if (!string.IsNullOrEmpty(StoreInfo?.HeaderImage))
        {
            var storeHeader = SteamCdnHelper.DownloadImage(Id, SteamImageType.Header);

            Header = storeHeader;

            return;
        }

        // this should run when Header is null regardless of whether
        // the StoreInfo.HeaderImage is null
        var appLogoUrl = SteamClientManager.Default.GetAppLogo(Id);
        if (!string.IsNullOrEmpty(appLogoUrl))
        {
            Header = Logo = SteamCdnHelper.DownloadImage(Id, SteamImageType.Logo, appLogoUrl);
        }

        // TODO: re-add this back when the other library views are available, until then nothing uses the icon
        // TODO: Change to be lazy loaded when needed
        //var iconName = SteamClientManager.Default.GetAppIcon(Id);
        //
        //if (!string.IsNullOrEmpty(iconName))
        //{
        //    Icon = SteamCdnHelper.DownloadImage(Id, SteamImageType.Icon, iconName);
        //}
    }
}
