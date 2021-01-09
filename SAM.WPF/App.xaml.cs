using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using FontAwesome.WPF;
using log4net;
using SAM.WPF.Core.API.Steam;
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
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                log.Info($"Application startup.");
                
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
                
                // create the default Client instance
                SteamClientManager.Init(0);

                SteamLibraryManager.Init();

                //Thread.Sleep(5000);

                var iconColor = (Color) ColorConverter.ConvertFromString("#E6E6E6");
                var iconBrush = new SolidColorBrush(iconColor);

                MainWindow = new MainWindow();
                MainWindow.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.Steam, iconBrush);
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
