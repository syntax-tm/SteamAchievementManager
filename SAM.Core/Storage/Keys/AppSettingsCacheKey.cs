namespace SAM.Core.Storage
{
    public class AppSettingsCacheKey : CacheKeyBase
    {
        public uint AppId { get; }

        public AppSettingsCacheKey(uint appId)
            : base($"{appId}_settings", $@"apps\{appId}\")
        {
            AppId = appId;
        }
    }
}
