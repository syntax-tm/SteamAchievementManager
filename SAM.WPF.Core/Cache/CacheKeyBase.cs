using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;

namespace SAM.WPF.Core
{
    [DebuggerDisplay("{GetFullPath()}")]
    public abstract class CacheKeyBase : ICacheKey
    {
        private const string DEFAULT_EXTENSION = ".json";

        protected readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.ReflectedType ?? typeof(CacheKeyBase));

        private string _fullPath;

        public string Key { get; protected set; }
        public string FilePath { get; protected set; }

        protected CacheKeyBase()
        {

        }

        protected CacheKeyBase(string key, string path = "")
        {
            SetKey(key);

            FilePath = path;
        }

        protected void SetKey(string key)
        {
            var fileName = key;
            
            var hasExtension = Path.HasExtension(key);
            if (!hasExtension)
            {
                fileName = Path.ChangeExtension(fileName, DEFAULT_EXTENSION);
            }

            Key = fileName;
        }

        public virtual string GetFullPath()
        {
            if (_fullPath != null) return _fullPath;
            
            return _fullPath = Path.Combine(FilePath, Key);
        }
    }
}
