using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using FontAwesome.WPF;
using log4net;
using SAM.WPF.Core.SplashScreen;
using SAM.WPF.Core.Themes;

namespace SAM.WPF
{
    public partial class App
    {
        protected readonly ILog log = LogManager.GetLogger(nameof(App));

        private void App_OnStartup(object sender, StartupEventArgs args)
        {
            try
            {
                var accentColors = ThemeManager.Current.Themes
                    .GroupBy(x => x.ColorScheme)
                    .OrderBy(a => a.Key)
                    .Select(a => new AccentColorMenuData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                    .ToList();

                var appThemes = ThemeManager.Current.Themes
                    .GroupBy(x => x.BaseColorScheme)
                    .Select(x => x.First())
                    .Select(a => new AppThemeMenuData { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
                    .ToList();

                ThemeManager.Current.AddTheme(RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", Colors.White));

                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
                ThemeManager.Current.SyncTheme();
                //ThemeManager.Current.ChangeThemeColorScheme(this, "Slate");
                
                SplashScreenHelper.Init();
                SplashScreenHelper.Show();
                
                //Thread.Sleep(5000);

                MainWindow = new MainWindow();
                MainWindow.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.Steam, Brushes.White);
                MainWindow.Show();

                SplashScreenHelper.Close();
            }
            catch (Exception e)
            {
                log.Error($"An error occurred on application startup. {e.Message}", e);

                throw;
            }
        }
    }
}
