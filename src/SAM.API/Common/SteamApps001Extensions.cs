using SAM.API.Wrappers;

namespace SAM.API;

public static class SteamApps001Extensions
{

	public static string GetAppName (this SteamApps001 clientApps, uint id)
	{
		return clientApps.GetAppData(id, @"name");
	}

	public static string GetAppIcon (this SteamApps001 clientApps, uint id)
	{
		return clientApps.GetAppData(id, @"icon");
	}

	public static string GetAppLogo (this SteamApps001 clientApps, uint id)
	{
		return clientApps.GetAppData(id, @"logo");
	}

}
