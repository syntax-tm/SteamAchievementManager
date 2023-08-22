using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SAM.Core
{
    /// <summary>
    /// Utility for opening various Steam browser protocol links and external resources.
    /// </summary>
    /// <seealso href="https://developer.valvesoftware.com/wiki/Steam_browser_protocol">Steam Browser Protocol</seealso>
    /// <seealso href="https://steamcommunity.com/dev/registerkey">Steam Web API Key</seealso>
    public static class BrowserHelper
    {
        // TODO: Make these configurable to disable or enable
        // TODO: Add support for custom context menu items
        private const string APP_NEWS_URI_FORMAT = @"steam://appnews/{0}";
        private const string UPDATE_NEWS_URI_FORMAT = @"steam://updatenews/{0}";
        private const string FRIENDS_URI = @"steam://friends/";
        private const string INSTALL_APP_URI_FORMAT = @"steam://install/{0}";
        private const string INSTALL_ADDON_URI_FORMAT = @"steam://install/{0}";
        private const string APP_RUN_URI_FORMAT = @"steam://rungameid/{0}";
        private const string GAME_HUB_URI_FORMAT = @"steam://url/GameHub/{0}";
        private const string APP_WORKSHOP_URI_FORMAT = @"steam://url/SteamWorkshopPage/{0}";
        private const string VALIDATE_APP_URI_FORMAT = @"steam://validate/{0}";
        private const string ACHIEVEMENTS_URI_FORMAT = @"steam://url/SteamIDAchievementsPage/{0}";

        private const string STEAM_STORE_URI_FORMAT = @"https://store.steampowered.com/app/{0}";
        private const string STEAMDB_URI_FORMAT = @"https://steamdb.info/app/{0}/graphs/";
        private const string CARD_EXCHANGE_URI_FORMAT = @"https://www.steamcardexchange.net/index.php?gamepage-appid-{0}";
        private const string PCGW_URI_FORMAT = @"https://www.pcgamingwiki.com/api/appid.php?appid={0}";

        public static void ViewAppNews(uint id) => OpenSteamUrl(APP_NEWS_URI_FORMAT, id);
        public static void ViewUpdateNews(uint id) => OpenSteamUrl(UPDATE_NEWS_URI_FORMAT, id);
        public static void ViewFriends() => OpenUrl(FRIENDS_URI);
        public static void InstallApp(uint id) => OpenSteamUrl(INSTALL_APP_URI_FORMAT, id);
        public static void InstallAddOn(uint id) => OpenSteamUrl(INSTALL_ADDON_URI_FORMAT, id);
        public static void StartApp(uint id) => OpenSteamUrl(APP_RUN_URI_FORMAT, id);
        public static void ViewGameHub(uint id) => OpenSteamUrl(GAME_HUB_URI_FORMAT, id);
        public static void ViewSteamWorkshop(uint id) => OpenSteamUrl(APP_WORKSHOP_URI_FORMAT, id);
        public static void ValidateApp(uint id) => OpenSteamUrl(VALIDATE_APP_URI_FORMAT, id);
        public static void ViewAchievements(uint id) => OpenSteamUrl(ACHIEVEMENTS_URI_FORMAT, id);

        public static void ViewOnSteamStore(uint id)
        {
            var steamStorePage = string.Format(STEAM_STORE_URI_FORMAT, id);

            OpenUrl(steamStorePage);
        }

        public static void ViewOnSteamDB(uint id)
        {
            var steamDbPage = string.Format(STEAMDB_URI_FORMAT, id);

            OpenUrl(steamDbPage);
        }

        public static void ViewOnSteamCardExchange(uint id)
        {
            var cardExchangePage = string.Format(CARD_EXCHANGE_URI_FORMAT, id);

            OpenUrl(cardExchangePage);
        }

        public static void ViewOnPCGW(uint id)
        {
            var pcgwPage = string.Format(PCGW_URI_FORMAT, id);

            OpenUrl(pcgwPage);
        }

        private static void OpenSteamUrl(string format, uint appId)
        {
            var url = string.Format(format, appId);

            OpenUrl(url);
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
