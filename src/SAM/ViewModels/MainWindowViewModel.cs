using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.Native;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using SAM.Core;
using SAM.Core.Storage;
using SAM.Core.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly ILog log = LogManager.GetLogger(typeof(MainWindowViewModel));

    [GenerateProperty] private SteamUser _user;
    [GenerateProperty] private HomeViewModel _homeVm;

    public MainWindowViewModel()
    {
        User = new ();
        HomeVm = new ();
    }
    
    [GenerateCommand]
    public void ResetAllSettings()
    {
        try
        {
            const string PROMPT = @"Are you sure you want to reset your app settings?";
            var result = MessageBox.Show(PROMPT, @"Confirm Reset", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

            if (result != MessageBoxResult.OK)
            {
                log.Info($"User responded '{result:G}' to reset confirmation. Cancelling...");

                return;
            }

            var currentCachePath = CacheManager.StorageManager.ApplicationStoragePath;
            var appsPath = Path.Join(currentCachePath, "apps");

            var appSettingsFiles = Directory.GetFiles(appsPath, @"*_settings.json", SearchOption.AllDirectories);

            foreach (var file in appSettingsFiles)
            {
                var fileName = Path.GetFileName(file);

                File.Delete(file);

                log.Info($"Deleted app settings file '{fileName}'.");
            }

            var settingsPath = Path.Join(currentCachePath, @"settings");

            if (Directory.Exists(settingsPath))
            {
                Directory.Delete(settingsPath, true);

                log.Info($"Deleted user settings directory '{settingsPath}'.");
            }

            HomeVm?.UnHideAll();

            HomeVm?.Library?.Items.Where(a => a.IsFavorite).ForEach(a => a.IsFavorite = false);

            log.Info("User settings reset was successful.");
        }
        catch (Exception ex)
        {
            var message = $"An error occurred attempting to reset user settings. {ex.Message}";

            log.Error(message, ex);
        }
    }

    [GenerateCommand]
    public void ViewChangelogOnGitHub()
    {
        const string URL = @"https://github.com/syntax-tm/SteamAchievementManager/blob/main/CHANGELOG.md";

        BrowserHelper.OpenUrl(URL);
    }

    [GenerateCommand]
    public void ViewIssuesOnGitHub()
    {
        const string URL = @"https://github.com/syntax-tm/SteamAchievementManager/issues";

        BrowserHelper.OpenUrl(URL);
    }
    
    [GenerateCommand]
    public void ViewOnGitHub()
    {
        const string URL = @"https://github.com/syntax-tm/SteamAchievementManager";

        BrowserHelper.OpenUrl(URL);
    }

    [GenerateCommand]
    public void ViewLogs()
    {
        const string LOG_DIR_NAME = @"logs";

        try
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var currentPath = Path.GetDirectoryName(assemblyLocation);
            var logPath = Path.Join(currentPath, LOG_DIR_NAME);

            if (!Directory.Exists(logPath))
            {
                throw new DirectoryNotFoundException("Application log directory does not exist.");
            }

            var psi = new ProcessStartInfo(logPath) { UseShellExecute = true, Verb = "open" };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            var message = $"An error occurred attempting to open the log directory. {ex.Message}";

            log.Error(message, ex);
        }
    }

    [GenerateCommand]
    public void ExportApps()
    {
        const string DEFAULT_TITLE = @"Library Export";
        const string DEFAULT_FILENAME = @"apps.json";
        const string DEFAULT_EXT = @"json";
        const string DEFAULT_FILTER = "Json Files (*.json)|*.json|All Files (*.*)|*.*";
            
        try
        {
            var fd = new SaveFileDialog
            {
                Title = DEFAULT_TITLE,
                FileName = DEFAULT_FILENAME,
                DefaultExt = DEFAULT_EXT,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AddExtension = true,
                OverwritePrompt = true,
                CheckPathExists = true,
                Filter = DEFAULT_FILTER
            };

            var result = fd.ShowDialog();

            if (!result.HasValue || !result.Value) return;

            var apps = HomeVm?.Library?.Items;
            var ids = apps?.Select(a => new { a.Id, a.Name, a.IsHidden, a.IsFavorite, a.IsMod, a.IsJunk, a.IsDemo }).ToList();
            var json = JsonConvert.SerializeObject(ids, Formatting.Indented);

            File.WriteAllText(fd.FileName, json, Encoding.UTF8);

            log.Info($"Successfully exported app list to '{fd.FileName}'.");
        }
        catch (Exception ex)
        {
            var message = $"An error occurred attempting to export the Steam library. {ex.Message}";

            log.Error(message, ex);
        }
    }
}
