namespace SAM.Core.Logging;

public class SteamAppContextHelper
{
    public const string KEY = @"SteamAppId";

    // TODO: either remove this class if it's not going to be logged or actually return the app id
    public override string ToString()
    {
        return "0";
    }
}
