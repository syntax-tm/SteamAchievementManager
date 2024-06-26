﻿namespace SAM.API;

public static class ClientExtensions
{

    public static bool OwnsGame(this Client client, uint id)
    {
        return client.SteamApps008.IsSubscribedApp(id);
    }

    public static string GetAppName(this Client client, uint id)
    {
        return client.SteamApps001.GetAppName(id);
    }

    public static string GetAppIcon(this Client client, uint id)
    {
        return client.SteamApps001.GetAppIcon(id);
    }

    public static string GetAppLogo(this Client client, uint id)
    {
        return client.SteamApps001.GetAppLogo(id);
    }
}
