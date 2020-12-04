namespace SAM.WPF.Core.Cache
{
    public class AppListCacheKey : CacheKeyBase
    {
        public override string Key { get; } = "appList.json";
        public override string Path { get; } = string.Empty;
    }
}
