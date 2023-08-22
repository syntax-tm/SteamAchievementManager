namespace SAM.WPF.Core.Cache
{
    public class UserLibraryCacheKey : CacheKeyBase
    {
        public override string Key { get; } = "userLibrary.json";
        public override string Path { get; } = string.Empty;
    }
}
