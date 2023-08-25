using System.ComponentModel;

namespace SAM.Core;

[DefaultValue(Default)]
public enum CacheKeyType
{
    [Description("")]
    Default = 0,
    [Description("settings")]
    Settings,
    [Description("apps")]
    App,
    [Description("themes")]
    Theme,
    [Description("data")]
    Data,
    [Description("docs")]
    Docs,
    [Description("backups")]
    Backup
}
