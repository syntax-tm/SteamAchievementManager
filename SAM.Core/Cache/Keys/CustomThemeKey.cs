namespace SAM.Core
{
    public class CustomThemeKey : CacheKeyBase
    {
        public CustomThemeKey(string key)
            : base(key, "Themes")
        {
        }
    }
}
