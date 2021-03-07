using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using log4net;
using SAM.WPF.Core.Extensions;
using SAM.WPF.Core.Settings;

namespace SAM.WPF.Core.Themes
{
    public static class ThemeHelper
    {

        private const string APP_THEME_REGISTRY_PATH = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string APP_THEME_KEY = @"AppsUseLightTheme";

        private static readonly ILog log = LogManager.GetLogger(nameof(ThemeHelper));

        private static List<AccentColorMenuData> _accentColors;
        private static List<AppThemeMenuData> _appThemeMenuData;
        private static SystemAppTheme? _systemAppTheme;

        public static SystemAppTheme SystemAppTheme
        {
            get
            {
                if (_systemAppTheme != null) return _systemAppTheme.Value;
                _systemAppTheme = GetSystemTheme();
                return _systemAppTheme ?? default;
            }
        }
        public static Theme CurrentTheme { get; set; }

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
            var baseTheme = SystemAppTheme.GetDescription();
            var generatedTheme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme(baseTheme, ThemeSettings.Default.Accent);

            Debug.Assert(generatedTheme != null);
            
            ThemeManager.Current.AddTheme(generatedTheme);

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }
        
        private static SystemAppTheme GetSystemTheme()
        {
            try
            {
                var themeValue = (int) Microsoft.Win32.Registry.GetValue(APP_THEME_REGISTRY_PATH, APP_THEME_KEY, null);

                if (Enum.IsDefined(typeof(SystemAppTheme), themeValue))
                {
                    return (SystemAppTheme) themeValue;
                }
            }
            catch (Exception e)
            {
                log.Error($"An error occurred checking the system app theme setting. {e.Message}", e);
            }

            return default;
        }


    }
}
