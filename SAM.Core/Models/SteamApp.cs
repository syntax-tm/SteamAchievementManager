using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core
{
    [DebuggerDisplay("{Name} ({Id})")]
    public class SteamApp : ViewModelBase
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(SteamApp));

        private Process _managerProcess;

        public uint Id { get; }

        public virtual string Name { get; set; }
        public virtual GameInfoType GameInfoType { get; set; }
        public bool IsJunk => GameInfoType == GameInfoType.Junk;
        public bool IsDemo => GameInfoType == GameInfoType.Demo;
        public bool IsNormal => GameInfoType == GameInfoType.Normal;
        public bool IsTool => GameInfoType == GameInfoType.Tool;
        public bool IsMod => GameInfoType == GameInfoType.Mod;
        public virtual bool IsLoading { get; set; }
        public virtual bool Loaded { get; set; }
        public virtual string Publisher { get; set; }
        public virtual string Developer { get; set; }
        public virtual SteamStoreApp StoreInfo { get; set; }
        public virtual Image Icon { get; set; }
        public virtual Image Header { get; set; }
        public virtual Image Capsule { get; set; }
        public virtual string Group { get; set; }
        public virtual bool IsHidden { get; set; }
        public virtual bool IsFavorite { get; set; }

        protected SteamApp(uint id, GameInfoType type)
        {
            Id = id;
            GameInfoType = type;
            
            Load();
        }
        
        protected SteamApp(SupportedApp supportedApp) 
            : this(supportedApp.Id, supportedApp.GameInfoType)
        {
        }

        public static SteamApp Create(uint id, GameInfoType type)
        {
            return ViewModelSource.Create(() => new SteamApp(id, type));
        }
        
        public static SteamApp Create(SupportedApp supportedApp)
        {
            return ViewModelSource.Create(() => new SteamApp(supportedApp));
        }

        public void ManageApp()
        {
            if (_managerProcess != null && _managerProcess.SetActive()) return;

            _managerProcess = SAMHelper.OpenManager(Id);
        }

        public void LaunchApp()
        {
            BrowserHelper.StartApp(Id);
        }
        
        public void InstallApp()
        {
            BrowserHelper.InstallApp(Id);
        }
        
        public void ViewAchievements()
        {
            BrowserHelper.ViewAchievements(Id);
        }
        
        public void ViewSteamWorkshop()
        {
            BrowserHelper.ViewSteamWorkshop(Id);
        }

        public void ViewOnSteamDB()
        {
            BrowserHelper.ViewOnSteamDB(Id);
        }

        public void ViewOnSteam()
        {
            BrowserHelper.ViewOnSteamStore(Id);
        }

        public void ViewOnSteamCardExchange()
        {
            BrowserHelper.ViewOnSteamCardExchange(Id);
        }

        public void ViewOnPCGW()
        {
            BrowserHelper.ViewOnPCGW(Id);
        }

        public void CopySteamID()
        {
            TextCopy.ClipboardService.SetText(Id.ToString());
        }
        
        public void ToggleVisibility()
        {
            IsHidden = !IsHidden;
        }
        
        public void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
        }

        public void Load()
        {
            if (Loaded) return;

            try
            {
                IsLoading = true;

                IsolatedStorageManager.CreateDirectory($@"apps\{Id}");

                LoadClientInfo();
                LoadStoreInfo();
                LoadImages();
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
        
        public void LoadClientInfo()
        {
            Name = SteamClientManager.Default.GetAppName(Id);

            if (string.IsNullOrEmpty(Name)) return;

            Group = char.IsDigit(Name[0])
                ? "#"
                : (Name?.Substring(0, 1));
        }

        private void LoadStoreInfo()
        {
            var retryTime = TimeSpan.FromSeconds(30);

            while (StoreInfo == null)
            {
                try
                {
                    StoreInfo = SteamworksManager.GetAppInfo(Id);

                    if (StoreInfo == null) return;

                    Publisher = StoreInfo.Publishers.FirstOrDefault();
                    Developer = StoreInfo.Developers.FirstOrDefault();
                }
                catch (WebException wex) when (((HttpWebResponse) wex.Response)?.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retrySb = new StringBuilder();

                    retrySb.Append($"Request for store info on app '{Id}' returned {nameof(HttpStatusCode)} {HttpStatusCode.TooManyRequests} for {nameof(HttpStatusCode.TooManyRequests)}. ");
                    retrySb.Append($"Waiting {retryTime.TotalSeconds} second(s) and then retrying...");

                    log.Warn(retrySb);

                    Thread.Sleep(retryTime);
                }
                catch (Exception e)
                {
                    log.Error($"An error occurred attempting to load the store info for app {Id}. {e.Message}", e);
                    break;
                }
            }
        }

        private void LoadImages()
        {
            try
            {
                var iconName = SteamClientManager.Default.GetAppIcon(Id);
                if (!string.IsNullOrEmpty(iconName))
                {
                    Icon = SteamCdnHelper.DownloadImage(Id, SteamImageType.Icon, iconName);
                }

                var appLogo = SteamClientManager.Default.GetAppLogo(Id);
                if (!string.IsNullOrEmpty(appLogo))
                {
                    Header = SteamCdnHelper.DownloadImage(Id, SteamImageType.Logo, appLogo);
                }
            }
            catch (Exception e)
            {
                var message = $"An error occurred loading the images for {Name} ({Id}). {e.Message}";
                log.Error(message, e);
            }
        }
    }
}
