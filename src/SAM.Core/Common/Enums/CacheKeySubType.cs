using System.ComponentModel;

namespace SAM.Core;

[DefaultValue(None)]
public enum CacheKeySubType
{
    None = 0,
    [Description("img")]
    Image
}
