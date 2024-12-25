namespace SAM.Core.Storage;

public static class CacheKeys
{
    private const uint DEFAULT_GAMES_XML_EXPIRATION = 2;
        
    public static ICacheKey AppList => new CacheKey(@"app_list.json", CacheKeyType.Data);
    public static ICacheKey CheckedAppList => new CacheKey(@"checked_apps.json", CacheKeyType.Default);
    public static ICacheKey UserTheme => new CacheKey(@"default", CacheKeyType.Theme);
    public static ICacheKey UserLibrary => new CacheKey(@"user_apps.json", CacheKeyType.Default);
    public static ICacheKey UserSettings => new CacheKey(@"user", CacheKeyType.Settings);
    public static ICacheKey Games => new CacheKey(@"games.xml", CacheKeyType.Data, GetGamesXmlExpiration());

    public static ICacheKey CreateAppCacheKey(uint appid)
    {
        var key = new CacheKey($"{appid}.json", appid, CacheKeyType.App);
        return key;
    }
        
    public static ICacheKey CreateAppSettingsCacheKey(uint appid)
    {
        var key = new CacheKey($"{appid}_settings.json", appid, CacheKeyType.App);
        return key;
    }

    public static ICacheKey CreateAppImageCacheKey(uint appid, string imageFileName)
    {
        var key = new CacheKey(imageFileName, appid, CacheKeyType.App, CacheKeySubType.Image);
        return key;
    }

    private static uint GetGamesXmlExpiration()
    {
        // TODO: add cache expiration override to all expiring cached items
        if (NetworkHelper.IsOnline())
        {
            return DEFAULT_GAMES_XML_EXPIRATION;
        }
        return ushort.MaxValue;
    }

}
