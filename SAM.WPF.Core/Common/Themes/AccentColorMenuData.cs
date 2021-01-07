using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;

namespace SAM.WPF.Core.Themes
{
    [DebuggerDisplay("{Name}")]
    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }

        public AccentColorMenuData()
        {
            this.ChangeAccentCommand = new SimpleCommand(o => true, this.DoChangeTheme);
        }

        public ICommand ChangeAccentCommand { get; }

        protected virtual void DoChangeTheme(object sender)
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, this.Name);
        }
    }
}
