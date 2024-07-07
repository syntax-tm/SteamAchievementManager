using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.API;
using SAM.Core;
using SAM.Core.Extensions;
using SAM.Core.Interfaces;
using SAM.Settings;
using SAM.Core.Storage;

namespace SAM;

[DebuggerDisplay("{Name} ({Id})")]
[GenerateViewModel]
public partial class SteamApp : BindableBase, ISteamApp
{
    protected readonly ILog log = LogManager.GetLogger(nameof(SteamApp));

    private Process _managerProcess;

    [GenerateProperty] private uint _id;
    [GenerateProperty] private string _name;
    [GenerateProperty] private GameInfoType _gameInfoType;
    [GenerateProperty] private bool _isLoading;
    [GenerateProperty] private bool _loaded;
    [GenerateProperty] private string _publisher;
    [GenerateProperty] private string _developer;
    [GenerateProperty] private string _franchise;
    [GenerateProperty] private ISteamStoreApp _storeInfo;
    [GenerateProperty] private Uri _icon;
    [GenerateProperty] private Uri _header;
    [GenerateProperty] private Uri _capsule;
    [GenerateProperty] private Uri _logo;
    [GenerateProperty] private bool _headerLoaded;
    [GenerateProperty] private bool _isHidden;
    [GenerateProperty] private bool _isFavorite;
    [GenerateProperty] private bool _isMenuOpen;
    [GenerateProperty] private bool _isManaging;
    [GenerateProperty] private bool _isInstalled;
    [GenerateProperty] private string _installDirectory;
    [GenerateProperty] private string _group;
    [GenerateProperty] private ulong _groupSortIndex;

    public bool IsJunk => GameInfoType == GameInfoType.Junk;
    public bool IsDemo => GameInfoType == GameInfoType.Demo;
    public bool IsNormal => GameInfoType == GameInfoType.Normal;
    public bool IsTool => GameInfoType == GameInfoType.Tool;
    public bool IsMod => GameInfoType == GameInfoType.Mod;
    public bool StoreInfoLoaded => StoreInfo != null;

    public SteamApp(uint id, GameInfoType type = GameInfoType.Normal)
    {
        Id = id;
        GameInfoType = type;

        Name = SteamClientManager.Default.GetAppName(Id);
    }

    public SteamApp(SupportedApp supportedApp)
        : this(supportedApp.Id, supportedApp.GameInfoType)
    {
    }

    [GenerateCommand]
    [SupportedOSPlatform("windows5.0")]
    public void ManageApp()
    {
        // TODO: Add a visual indication that the manager is running (handle Exited event)
        if (_managerProcess != null && _managerProcess.SetActive()) return;
        _managerProcess = SAMHelper.OpenManager(Id);
        _managerProcess.EnableRaisingEvents = true;
        _managerProcess.Exited += OnManagerProcessExited;
        IsManaging = true;
    }
    
    [GenerateCommand]
    public void EndManagerProcess()
    {
        if (_managerProcess == null) return;
        if (_managerProcess.HasExited) return;

        _managerProcess.Kill();
    }

    private void OnManagerProcessExited(object sender, EventArgs args)
    {
        _managerProcess.Exited -= OnManagerProcessExited;
        _managerProcess.Dispose();
        _managerProcess = null;

        IsManaging = false;
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
    }

    [GenerateCommand]
    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }
    
    [GenerateCommand]
    public void BrowseFiles()
    {
        if (!IsInstalled) return;
        if (string.IsNullOrEmpty(InstallDirectory)) return;
        if (!Directory.Exists(InstallDirectory)) return;

        var psi = new ProcessStartInfo(InstallDirectory) { UseShellExecute = true, Verb = "open" };

        Process.Start(psi);
    }

    [GenerateCommand]
    public async Task Load()
    {
        if (Loaded) return;

        try
        {
            IsLoading = true;

            // TODO: SteamApp shouldn't need to configure its cache directory structure
            CacheManager.StorageManager.CreateDirectory($@"apps\{Id}");

            var loadSteamDataTask = Task.Run(LoadSteamData);

            var dirty = false;
            var settings = LoadSettings();
            
            IsFavorite = settings.IsFavorite;
            IsHidden = settings.IsHidden;

            if (settings.GroupSortIndex == null)
            {
                var group = AppGroupHelper.GetGroup(this);

                Group = group.Name;
                GroupSortIndex = group.SortIndex;

                dirty = true;
            }
            else
            {
                Group = settings.Group;
                GroupSortIndex = settings.GroupSortIndex.Value;
            }

            if (settings.ImagesLoaded)
            {
                Logo = settings.Logo;
                Header = settings.Header;
                Capsule = settings.Capsule;
                Icon = settings.Icon;
            }
            else
            {
                await LoadImagesAsync();

                dirty = true;
            }

            await loadSteamDataTask;

            if (!dirty) return;

            SaveSettings();
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

    public void UnloadImages()
    {
        Header = null;
        Capsule = null;
        Icon = null;
        Logo = null;
    }

    [GenerateCommand]
    public async Task LoadImagesAsync()
    {
        try
        {
            // try and load the user's grid image for the app first
            if (SteamClientManager.TryGetCachedAppImageUri(Id, SteamImageType.GridLandscape, out var gridHeader))
            {
                Header = gridHeader;

                return;
            }

            // if they don't have a grid image then try and use the default header
            if (SteamClientManager.TryGetCachedAppImageUri(Id, SteamImageType.Header, out var appHeader))
            {
                Header = appHeader;

                return;
            }

            // if there's no default header either, then see if there's a logo we can use
            if (SteamClientManager.TryGetCachedAppImageUri(Id, SteamImageType.Logo, out var appLogo))
            {
                log.Info($"Using {nameof(SteamImageType.Logo)} for app id {Id}.");

                Header = Logo = appLogo;

                return;
            }

            // we didn't have a header OR a logo in the cache, so try downloading the header first
            //if (SteamCdnHelper.DownloadImage())
            //{
            //    var storeHeaderUri = SteamCdnHelper.GetImageUri(Id, SteamImageType.Header);

            //    Header = storeHeaderUri;

            //    return;
            //}

            // if we are only using local images don't download from the store

            //var settings = new HomeSettings(true);

            //if (settings.LocalImagesOnly) return;

            // if we don't have any cached header or logo image to use, then request the store info
            // refresh so that we can download a header
            //SteamworksManager.LoadStoreInfo(this);

            await Task.CompletedTask.ConfigureAwait(false);
        }
        catch (Exception e)
        {
            var message = $"An error occurred loading images for {Name} ({Id}). {e.Message}";
            log.Error(message, e);
        }
    }

    private void LoadSteamData()
    {
        var client = SteamClientManager.Default;
        var steamApps = client.SteamApps008;

        IsInstalled = steamApps.IsAppInstalled(Id);
        InstallDirectory = steamApps.GetAppInstallDir(Id);
    }

    private SteamAppSettings LoadSettings()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);

        if (!CacheManager.TryGetObject<SteamAppSettings>(key, out var settings))
        {
            return SteamAppSettings.Create(Id, Name);
        }

        log.Debug($"Loaded {nameof(SteamAppSettings)} {settings}.");

        return settings;
    }

    private void SaveSettings()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);
        var settings = new SteamAppSettings
        {
            AppId = Id,
            IsFavorite = IsFavorite,
            IsHidden = IsHidden,
            Name = Name,
            Group = Group,
            GroupSortIndex = GroupSortIndex,
            Logo = Logo,
            Header = Header,
            Capsule = Capsule,
            Icon = Icon,
            Version = SteamAppSettings.CurrentVersion
        };

        CacheManager.CacheObject(key, settings);

        log.Debug($"Saving {nameof(SteamAppSettings)} {settings}.");
    }
    
    protected void OnIsHiddenChanged()
    {
        if (IsLoading) return;
        
        RefreshGroup();

        SaveSettings();

        //Messenger.Default.SendRequest(EntityType.Library, RequestType.Refresh);
    }

    protected void OnIsFavoriteChanged()
    {
        if (IsLoading) return;
        
        RefreshGroup();

        SaveSettings();

        //Messenger.Default.SendRequest(EntityType.Library, RequestType.Refresh);
    }

    protected void OnHeaderChanged()
    {
        HeaderLoaded = Header != null;
    }

    private void RefreshGroup()
    {
        var group = AppGroupHelper.GetGroup(this);

        Group = group.Name;
        GroupSortIndex = group.SortIndex;
    }
}
