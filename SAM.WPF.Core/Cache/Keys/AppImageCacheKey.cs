namespace SAM.WPF.Core
{
    public class AppImageCacheKey : CacheKeyBase
    {
        public uint AppId { get; }
        public string ImageFileName { get; }
        
        public AppImageCacheKey(uint appId, string imageFileName)
            : base(imageFileName, $@"apps\{appId}\")
        {
            AppId = appId;
            ImageFileName = imageFileName;
        }
    }
}
