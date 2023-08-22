using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using log4net;
using SAM.Core.Extensions;
using SAM.Core.Settings;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Services;

namespace SAM.Core
{
    public static class ThemeHelper
    {

        private const string APP_THEME_REGISTRY_PATH = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string APP_THEME_KEY = @"AppsUseLightTheme";

        private static readonly ILog log = LogManager.GetLogger(nameof(ThemeHelper));

        private static readonly ThemeService _themeService = new ();
        
        private static ThemeType? _systemAppTheme;

        public static ThemeType SystemAppTheme
        {
            get
            {
                if (_systemAppTheme != null) return _systemAppTheme.Value;
                _systemAppTheme = _themeService.GetSystemTheme();
                return _systemAppTheme ?? default;
            }
        }

        public static void SetTheme()
        {
            _themeService.SetTheme(ThemeType.Dark);
            _themeService.SetAccent((Color)ColorConverter.ConvertFromString("#111111"));

            //_themeService.SetSystemAccent();
        }
    }
}
