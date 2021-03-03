using System.ComponentModel;

namespace SAM.WPF.Core.Themes
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
