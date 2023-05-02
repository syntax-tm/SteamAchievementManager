namespace SAM.Core.Storage
{
    public class CustomThemeKey : CacheKeyBase
    {
        public CustomThemeKey(string key)
            : base(key, "Themes")
        {
        }
    }
}
