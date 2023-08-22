using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Input;
using DevExpress.Mvvm;
using log4net;
using SAM.API;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    [DebuggerDisplay("{Name} ({Id})")]
    public class SteamApp : ViewModelBase
    {

        protected readonly ILog log = LogManager.GetLogger(nameof(SteamApp));

        //private bool _statsLoaded;

        private Process _managerProcess;

        public uint Id { get; }

        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
        }
        public GameInfoType GameInfoType
        {
            get => GetProperty(() => GameInfoType);
            set => SetProperty(() => GameInfoType, value);
        }
        public virtual bool IsLoading
        {
            get => GetProperty(() => IsLoading);
            set => SetProperty(() => IsLoading, value);
        }
        public bool Loaded
        {
            get => GetProperty(() => Loaded);
            set => SetProperty(() => Loaded, value);
        }
        public string Publisher
        {
            get => GetProperty(() => Publisher);
            set => SetProperty(() => Publisher, value);
        }
        public string Developer 
        {
            get => GetProperty(() => Developer);
            set => SetProperty(() => Developer, value);
        }
        public SteamStoreApp StoreInfo
        {
            get => GetProperty(() => StoreInfo);
            set => SetProperty(() => StoreInfo, value);
        }
        public Image Icon
        {
            get => GetProperty(() => Icon);
            set => SetProperty(() => Icon, value);
        }
        public Image Header
        {
            get => GetProperty(() => Header);
            set => SetProperty(() => Header, value);
        }

        public ICommand ManageAppCommand => new DelegateCommand(ManageApp);

        public SteamApp(uint id, GameInfoType type)
        {
            Id = id;
            GameInfoType = type;

            Load();
        }
        
        public SteamApp(SupportedApp supportedApp) 
            : this(supportedApp.Id, supportedApp.GameInfoType)
        {
        }

        public void ManageApp()
        {
            if (!Loaded) return;

            if (_managerProcess == null || !_managerProcess.SetActive())
            {
                _managerProcess = SAMHelper.OpenManager(Id);
            }
        }

        public void Load()
        {
            if (Loaded) return;

            try
            {
                IsLoading = true;

                IsolatedStorageManager.CreateDirectory($@"apps\{Id}");

                //using var client = new Client();
                //client.Initialize(0);

                LoadClientInfo();
                LoadStoreInfo();
                LoadImages();
            }
            catch (Exception e)
            {
                log.Error($"An error occurred attempting to load app info for app id {Id}. {e.Message}", e);
            }
            finally
            {
                Loaded = true;
                IsLoading = false;
            }
        }
        
        public void LoadClientInfo(Client client = null)
        {
            client ??= SteamClientManager.Default;

            Name = client.GetAppName(Id);
        }

        private void LoadStoreInfo()
        {
            var retryTime = new TimeSpan(0, 1, 0);

            while (StoreInfo == null)
            {
                try
                {
                    StoreInfo = SteamworksManager.GetAppInfo(Id);

                    if (StoreInfo == null) return;

                    Publisher = StoreInfo.Publishers.FirstOrDefault();
                    Developer = StoreInfo.Developers.FirstOrDefault();
                }
                catch (WebException wex) when (((HttpWebResponse) wex.Response).StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retrySb = new StringBuilder();

                    retrySb.Append($"Request for store info on app '{Id}' returned {nameof(HttpStatusCode)} {HttpStatusCode.TooManyRequests} for {nameof(HttpStatusCode.TooManyRequests)}. ");
                    retrySb.Append($"Waiting {retryTime.TotalMinutes:0.#} minute(s) and then retrying...");

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

        private void LoadImages(Client client = null)
        {
            client ??= SteamClientManager.Default;

            var iconName = client.GetAppIcon(Id);
            if (!string.IsNullOrEmpty(iconName))
            {
                Icon = SteamCdnHelper.DownloadImage(Id, SteamImageType.Icon, iconName);
            }

            Header = SteamCdnHelper.DownloadImage(Id, SteamImageType.Header);
        }

    }
}
