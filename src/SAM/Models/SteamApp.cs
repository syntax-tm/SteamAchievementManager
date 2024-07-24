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
using SAM.Managers;

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
    [GenerateProperty] private Uri _header;
    [GenerateProperty] private bool _isAnimatedHeader;
    [GenerateProperty] private Uri _capsule;
    [GenerateProperty] private bool _isAnimatedCapsule;
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

            var loadSteamDataTask = Task.Run(LoadSteamData);

            var dirty = false;
            var settings = await LoadSettingsAsync();
            
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
                Header = settings.Header;
                IsAnimatedHeader = settings.IsAnimatedHeader;
                Capsule = settings.Capsule;
                IsAnimatedCapsule = settings.IsAnimatedCapsule;
            }
            else
            {
                await LoadImagesAsync();

                dirty = true;
            }

            await loadSteamDataTask;

            if (!dirty) return;

            await SaveSettingsAsync();
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
        IsAnimatedHeader = false;
    }

    [GenerateCommand]
    public async Task LoadImagesAsync()
    {
        try
        {
            // try and load the user's grid image for the app first
            if (SteamImageManager.TryGetCachedAppImage(Id, SteamImageType.GridLandscape, out var response))
            {
                Header = response.Uri;
                IsAnimatedHeader = response.IsAnimated;
            }
            // if they don't have a grid image then try and use the default header
            else if (SteamImageManager.TryGetCachedAppImage(Id, SteamImageType.Header, out response))
            {
                Header = response.Uri;
                IsAnimatedHeader = response.IsAnimated;
            }
            // if there's no default header either, then see if there's a logo we can use
            else if (SteamImageManager.TryGetCachedAppImage(Id, SteamImageType.Logo, out response))
            {
                log.Info($"Using {nameof(SteamImageType.Logo)} for app id {Id}.");
                
                Header = response.Uri;
                IsAnimatedHeader = response.IsAnimated;
            }

            // try and load the user's portrait grid image for the app
            if (SteamImageManager.TryGetCachedAppImage(Id, SteamImageType.GridPortrait, out response))
            {
                Capsule = response.Uri;
                IsAnimatedCapsule = response.IsAnimated;
            }
            // if they don't have a portrait grid, try and use the library 600x900 image
            else if (SteamImageManager.TryGetCachedAppImage(Id, SteamImageType.Library, out response))
            {
                Capsule = response.Uri;
                IsAnimatedCapsule = response.IsAnimated;
            }

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

    private async Task<SteamAppSettings> LoadSettingsAsync()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);

        try
        {
            var cachedSettings = await CacheManager.GetObjectAsync<SteamAppSettings>(key);
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            var defaultSettings = SteamAppSettings.Create(Id, Name);
            return defaultSettings;
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load the settings for {Id}. {e.Message}", e);
            throw;
        }
    }

    private async Task SaveSettingsAsync()
    {
        var key = CacheKeys.CreateAppSettingsCacheKey(Id);
        var settings = new SteamAppSettings(this);

        await CacheManager.CacheObjectAsync(key, settings);

        log.Debug($"Saved {nameof(SteamAppSettings)} {settings}.");
    }
    
    protected async Task OnIsHiddenChanged()
    {
        if (IsLoading) return;
        
        RefreshGroup();

        await SaveSettingsAsync();
    }

    protected async Task OnIsFavoriteChanged()
    {
        if (IsLoading) return;
        
        RefreshGroup();
        
        await SaveSettingsAsync();
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
