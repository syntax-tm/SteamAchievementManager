using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using log4net;
using SAM.API;
using SAM.WPF.Core.API.Steam;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    [DebuggerDisplay("{Name} ({Id})")]
    public class SteamApp
    {

        protected readonly ILog log = LogManager.GetLogger(nameof(SteamApp));

        //private bool _statsLoaded;

        public uint Id { get; }
        public virtual string Name { get; set; }
        public GameInfoType GameInfoType { get; set; }
        public virtual bool IsLoading { get; set; }
        public virtual bool Loaded { get; set; }
        public virtual string Publisher { get; set; }
        public virtual string Developer { get; set; }
        public virtual SteamStoreApp StoreInfo { get; set; }
        public virtual Image Icon { get; set; }
        public virtual Image Header { get; set; }
        //public virtual SteamStatsManager StatsManager { get; set; }

        protected SteamApp(SupportedApp supportedApp)
        {
            Id = supportedApp.Id;
            GameInfoType = supportedApp.GameInfoType;
        }

        protected SteamApp(uint id, GameInfoType type)
        {
            Id = id;
            GameInfoType = type;

            Task.Run(Load).ConfigureAwait(false);
        }

        public static SteamApp Create(SupportedApp supportedApp)
        {
            return ViewModelSource.Create(() => new SteamApp(supportedApp));
        }

        public static SteamApp Create(uint id, GameInfoType type)
        {
            return ViewModelSource.Create(() => new SteamApp(id, type));
        }

        public void Load()
        {
            if (Loaded) return;

            try
            {
                IsLoading = true;

                using var client = new Client();
                client.Initialize(0);

                LoadDetails(client);
                LoadStoreInfo();
                LoadImages(client);
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
            finally
            {
                Loaded = true;
                IsLoading = false;
            }
        }

        //public void LoadStats()
        //{
        //    if (_statsLoaded) return;

        //    try
        //    {
        //        StatsManager = new SteamStatsManager(Id);
        //        StatsManager.RefreshStats();

        //        _statsLoaded = true;
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error($"An error occurred loading the stats for app '{Id}'. {e.Message}", e);

        //        throw;
        //    }
        //}
        
        private void LoadDetails(Client steamClient)
        {
            Name = steamClient.GetAppName(Id);
        }

        private void LoadStoreInfo()
        {
            StoreInfo = SteamworksManager.GetAppInfo(Id);

            if (StoreInfo == null) return;

            Publisher = StoreInfo.Publishers.FirstOrDefault();
            Developer = StoreInfo.Developers.FirstOrDefault();
        }

        private void LoadImages(Client steamClient)
        {
            var iconName = steamClient.GetAppIcon(Id);
            if (!string.IsNullOrEmpty(iconName))
            {
                Icon = SteamCdnHelper.DownloadImage(Id, SteamImageType.Icon, iconName);
            }

            Header = SteamCdnHelper.DownloadImage(Id, SteamImageType.Header);
        }

    }
}
