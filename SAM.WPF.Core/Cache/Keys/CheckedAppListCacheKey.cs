namespace SAM.WPF.Core.Cache
{
    public class CheckedAppListCacheKey : CacheKeyBase
    {
        public override string Key { get; } = "checked_apps.json";
        public override string Path { get; } = string.Empty;
    }
}
