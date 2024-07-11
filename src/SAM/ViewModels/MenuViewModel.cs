using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.Native;
using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using SAM.Controls;
using SAM.Core;
using SAM.Core.Storage;
using SAM.Managers;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class MenuViewModel
{
    private const string GITHUB_CHANGELOG_URL = @"https://github.com/syntax-tm/SteamAchievementManager/blob/main/CHANGELOG.md";
    private const string GITHUB_ISSUES_URL = @"https://github.com/syntax-tm/SteamAchievementManager/issues";
    private const string GITHUB_URL = @"https://github.com/syntax-tm/SteamAchievementManager";

    private readonly ILog log = LogManager.GetLogger(typeof(MenuViewModel));

    private ObservableHandler<HomeViewModel> _homeHandler;

    [GenerateProperty] private HomeViewModel _homeVm;
    [GenerateProperty] private SteamGameViewModel _gameVm;
    [GenerateProperty] private ApplicationMode _mode;

    public bool IsLibrary => _mode == ApplicationMode.Default;
    public bool IsManager => _mode == ApplicationMode.Manager;

    public MenuViewModel(HomeViewModel homeVm)
    {
        _homeVm = homeVm;
        _mode = ApplicationMode.Default;

        _homeHandler = new ObservableHandler<HomeViewModel>(homeVm)
            .Add(h => h.CurrentVm, OnHomeViewChanged);
    }

    public MenuViewModel(SteamGameViewModel gameVm)
    {
        _gameVm = gameVm;
        _mode = ApplicationMode.Manager;
    }

    [GenerateCommand]
    public void OpenSteamConsole()
    {
        BrowserHelper.OpenSteamConsole();
    }

    [GenerateCommand]
    public void ResetAllSettings()
    {
        try
        {
            const string PROMPT = @"Are you sure you want to reset your app settings?";
            var result = MessageBox.Show(PROMPT, @"Confirm Reset", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

            if (result != MessageBoxResult.Yes)
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

            HomeVm?.CurrentVm?.UnHideAll();

            HomeVm?.CurrentVm?.Library?.Items.Where(a => a.IsFavorite).ForEach(a => a.IsFavorite = false);

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
        BrowserHelper.OpenUrl(GITHUB_CHANGELOG_URL);
    }

    [GenerateCommand]
    public void ViewIssuesOnGitHub()
    {
        BrowserHelper.OpenUrl(GITHUB_ISSUES_URL);
    }

    [GenerateCommand]
    public void ViewOnGitHub()
    {
        BrowserHelper.OpenUrl(GITHUB_URL);
    }

    [GenerateCommand]
    public void ViewLogs()
    {
        const string LOG_DIR_NAME = @"logs";

        try
        {
            var assemblyLocation = AppContext.BaseDirectory;
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

            var apps = SteamLibraryManager.DefaultLibrary?.Items;
            var ids = apps?.Select(a => new { a.Id, a.Name, a.IsHidden, a.IsFavorite, a.GameInfoType }).ToList();
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

    [GenerateCommand]
    public void Exit()
    {
        Environment.Exit(0);
    }

    private void Refresh()
    {
        
    }

    private void OnHomeViewChanged()
    {
        // not currently used
    }
}
