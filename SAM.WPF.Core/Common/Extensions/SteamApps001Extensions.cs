using SAM.API.Wrappers;

namespace SAM.WPF.Core.Extensions
{
    public static class SteamApps001Extensions
    {

        public static string GetAppName(this SteamApps001 clientApps, uint id)
        {
            return clientApps.GetAppData(id, "name");
        }

        public static string GetAppIcon(this SteamApps001 clientApps, uint id)
        {
            return clientApps.GetAppData(id, "icon");
        }

    }
}
