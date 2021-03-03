using System.Diagnostics;
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
        public string Path { get; protected set; }

        protected CacheKeyBase()
        {

        }

        protected CacheKeyBase(string key, string path = "")
        {
            SetKey(key);

            Path = path;
        }

        protected void SetKey(string key)
        {
            var fileName = key;
            
            var hasExtension = System.IO.Path.HasExtension(key);
            if (!hasExtension)
            {
                log.Debug($"No extension for {nameof(key)} '{key}', defaulting to '{DEFAULT_EXTENSION}'.");

                fileName = System.IO.Path.ChangeExtension(fileName, DEFAULT_EXTENSION);
            }

            Key = fileName;
        }

        public virtual string GetFullPath()
        {
            if (_fullPath != null) return _fullPath;
            
            return _fullPath = System.IO.Path.Combine(Path, Key);
        }
    }
}
