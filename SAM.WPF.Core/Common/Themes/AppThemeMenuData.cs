using System.Diagnostics;
using System.Windows;
using ControlzEx.Theming;

namespace SAM.WPF.Core.Themes
{
    [DebuggerDisplay("{Name}")]
    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, this.Name);
        }
    }
}
