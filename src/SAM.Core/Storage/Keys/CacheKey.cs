using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using SAM.Core.Extensions;

namespace SAM.Core.Storage
{
    // TODO: Add configurable cache expiration
    [DebuggerDisplay("{GetFullPath()}")]
    public class CacheKey : ICacheKey
    {
        private const string DEFAULT_EXTENSION = ".json";

        protected readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.ReflectedType ?? typeof(CacheKey));

        private string _fullPath;

        public string Key { get; protected set; }
        public string FilePath { get; protected set; }
        /// <summary>
        /// The maximum number of days the cached item is valid for. By <see langword="default"/> (<see langword="null"/>) the cached item does not expire.
        /// </summary>
        public uint? DaysValid { get; internal set; }
        public bool HasExpiration => DaysValid.HasValue;

        protected CacheKey()
        {

        }

        public CacheKey(object key, CacheKeyType type)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            SetKey(key);
            
            if (type == CacheKeyType.App)
            {
                throw new NotSupportedException(@$"{CacheKeyType.App} cache keys require an additional id parameter.");
            }

            FilePath = type.GetDescription();
        }
        
        public CacheKey(object key, CacheKeyType type, uint daysValid)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            SetKey(key);
            
            if (type == CacheKeyType.App)
            {
                throw new NotSupportedException(@$"{CacheKeyType.App} cache keys require an additional id parameter.");
            }

            DaysValid = daysValid;

            FilePath = type.GetDescription();
        }
        
        public CacheKey(string fileName, object id, CacheKeyType type = CacheKeyType.Default, CacheKeySubType subType = CacheKeySubType.None)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            SetKey(fileName);

            if (type != CacheKeyType.App)
            {
                throw new NotSupportedException(@$"Only {CacheKeyType.App} cache keys support {nameof(id)}.");
            }
            
            var path = new List<object>
            {
                type.GetDescription(),
                id
            };

            // add subtype to path if set (currently only for app images)
            if (subType != CacheKeySubType.None)
            {
                path.Add(subType.GetDescription());
            }

            FilePath = string.Join(Path.DirectorySeparatorChar, path);
        }

        public virtual string GetFullPath()
        {
            if (_fullPath != null) return _fullPath;

            return _fullPath = Path.Combine(FilePath, Key);
        }

        public static bool IsExpired(CacheKey key, string fileName)
        {
            var fi = new FileInfo(fileName);

            return IsExpired(key, fi);
        }

        public static bool IsExpired(CacheKey key, FileInfo fi)
        {
            // if there's no expiration, it's never expired
            if (!key.HasExpiration) return false;

            var cacheDate = fi.CreationTime;
            var cacheLimit = key.DaysValid!.Value;
            var expirationDate =cacheDate.AddDays(cacheLimit);

            return DateTime.Now < expirationDate;
        }

        protected void SetKey(object key)
        {
            var fileName = key.ToString();

            var hasExtension = Path.HasExtension(fileName);
            if (!hasExtension)
            {
                fileName = Path.ChangeExtension(fileName, DEFAULT_EXTENSION);
            }

            Key = fileName;
        }

    }
}
