namespace SAM.WPF.Core.Cache
{
    public class AppImageCacheKey : CacheKeyBase
    {
        public uint AppId { get; }
        public string ImageFileName { get; }
        public override string Key => ImageFileName;
        public override string Path => $@"apps\{AppId}\";

        public AppImageCacheKey(uint appId, string imageFileName)
        {
            AppId = appId;
            ImageFileName = imageFileName;
        }
    }
}
