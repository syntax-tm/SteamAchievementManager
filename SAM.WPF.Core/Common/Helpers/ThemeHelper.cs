using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using ControlzEx.Theming;
using SAM.WPF.Core.Themes;

namespace SAM.WPF.Core
{
    public static class ThemeHelper
    {

        private static List<AccentColorMenuData> _accentColors;
        private static List<AppThemeMenuData> _appThemeMenuData;

        public static List<AccentColorMenuData> AccentColors
        {
            get
            {
                if (_accentColors != null) return _accentColors;

                _accentColors = ThemeManager.Current.Themes
                    .GroupBy(x => x.ColorScheme)
                    .OrderBy(a => a.Key)
                    .Select(a => new AccentColorMenuData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                    .ToList();

                return _accentColors;
            }
        }

        public static List<AppThemeMenuData> AppThemeMenuData
        {
            get
            {
                if (_appThemeMenuData != null) return _appThemeMenuData;

                _appThemeMenuData = ThemeManager.Current.Themes
                    .GroupBy(x => x.BaseColorScheme)
                    .Select(x => x.First())
                    .Select(a => new AppThemeMenuData
                        {
                            Name = a.BaseColorScheme, 
                            BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, 
                            ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush
                        })
                    .ToList();

                return _appThemeMenuData;
            }
        }
        
        public static void SetTheme()
        {
            var generatedTheme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", Colors.White);

            Debug.Assert(generatedTheme != null);
            
            ThemeManager.Current.AddTheme(generatedTheme);

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }

    }
}
