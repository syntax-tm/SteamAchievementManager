using System.Collections.Generic;

namespace SAM.WPF.Core.Cache
{
    public static class CacheKeyFactory
    {

        public static ICacheKey CreateAppListCacheKey()
        {
            var key = new AppListCacheKey();
            
            return key;
        }

        public static ICacheKey CreateAppCacheKey(uint appid)
        {
            var key = new AppCacheKey(appid);
            
            return key;
        }

        public static ICacheKey CreateAppImageCacheKey(uint appid, string imageFileName)
        {
            var key = new AppImageCacheKey(appid, imageFileName);
            
            return key;
        }

        public static ICacheKey CreateUserLibraryCacheKey()
        {
            var key = new UserLibraryCacheKey();
            
            return key;
        }

        public static ICacheKey CreateCheckedAppsCacheKey()
        {
            var key = new CheckedAppListCacheKey();
            
            return key;
        }

    }
}
