namespace SAM.WPF.Core.Cache
{
    public class AppCacheKey : CacheKeyBase
    {
        public uint AppId { get; }
        public override string Key => $"{AppId}.json";
        public override string Path => $@"apps\{AppId}\";

        public AppCacheKey(uint appId)
        {
            AppId = appId;
        }
    }
}
