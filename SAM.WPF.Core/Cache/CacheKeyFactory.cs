using System.Collections.Generic;

namespace SAM.WPF.Core.Cache
{
    public static class CacheKeyFactory
    {

        public static ICacheKey CreateAppListCacheKey()
        {
            var key = new AppListCacheKey();

            IsolatedStorageManager.CreateDirectory(key.Path);

            return key;
        }

        public static ICacheKey CreateAppCacheKey(uint appid)
        {
            var key = new AppCacheKey(appid);

            IsolatedStorageManager.CreateDirectory(key.Path);

            return key;
        }

        public static ICacheKey CreateAppImageCacheKey(uint appid, string imageFileName)
        {
            var key = new AppImageCacheKey(appid, imageFileName);
            
            IsolatedStorageManager.CreateDirectory(key.Path);

            return key;
        }

        public static ICacheKey CreateUserLibraryCacheKey()
        {
            var key = new UserLibraryCacheKey();

            IsolatedStorageManager.CreateDirectory(key.Path);

            return key;
        }

        public static ICacheKey CreateCheckedAppsCacheKey()
        {
            var key = new CheckedAppListCacheKey();

            IsolatedStorageManager.CreateDirectory(key.Path);

            return key;
        }

    }
}
