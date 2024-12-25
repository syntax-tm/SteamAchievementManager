using System.Net.Http;

namespace SAM.Core;

public static class NetworkHelper
{
    private const string STEAM_STORE_URL = @"https://store.steampowered.com";

    public static bool IsOnline()
    {
        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, STEAM_STORE_URL);
            using var response = client.Send(request);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
