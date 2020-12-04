using System.Diagnostics;

namespace SAM.WPF.Core.Cache
{
    [DebuggerDisplay("{GetFullPath()}")]
    public abstract class CacheKeyBase : ICacheKey
    {
        public abstract string Key { get; }
        public abstract string Path { get; }
        
        public virtual string GetFullPath()
        {
            return System.IO.Path.Combine(Path, Key);
        }
    }
}
