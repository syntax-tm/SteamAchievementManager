namespace SAM.WPF.Core
{
    public class AppCacheKey : CacheKeyBase
    {
        public uint AppId { get; }

        public AppCacheKey(uint appId)
            : base($"{appId}", $@"apps\{appId}\")
        {
            AppId = appId;
        }
    }
}
