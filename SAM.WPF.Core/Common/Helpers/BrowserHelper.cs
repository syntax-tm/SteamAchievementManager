using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SAM.WPF.Core
{
    public static class BrowserHelper
    {

        private const string STEAM_STORE_URI_FORMAT = @"https://store.steampowered.com/app/{0}";
        private const string STEAMDB_URI_FORMAT = @"https://steamdb.info/app/{0}/graphs/";
        private const string CARD_EXCHANGE_URI_FORMAT = @"https://www.steamcardexchange.net/index.php?gamepage-appid-{0}";
        private const string PCGW_URI_FORMAT = @"https://www.pcgamingwiki.com/api/appid.php?appid={0}";

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
