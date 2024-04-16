namespace SAM.Core.Logging;

public class SteamAppContextHelper
{
    public const string KEY = @"SteamAppId";

    public override string ToString()
    {
        return SteamClientManager.AppId.ToString();
    }
}
