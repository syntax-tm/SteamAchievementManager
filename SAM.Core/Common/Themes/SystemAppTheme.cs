using System.ComponentModel;

namespace SAM.Core
{
    [DefaultValue(Light)]
    public enum SystemAppTheme
    {
        [Description(nameof(Dark))]
        Dark = 0,
        [Description(nameof(Light))]
        Light = 1
    }
}
