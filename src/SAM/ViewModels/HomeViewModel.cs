using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using log4net;
using SAM.Core.Messages;

namespace SAM.ViewModels;

[GenerateViewModel(ImplementISupportServices = true)]
public partial class HomeViewModel
{
    private readonly ILog log = LogManager.GetLogger(nameof(HomeViewModel));
    
    [GenerateProperty] private LibraryViewModel _currentVm;
    [GenerateProperty] private LibraryView _libraryViewType;
    [GenerateProperty] private HomeSettings _settings;
    [GenerateProperty] private ILibrarySettings _currentSettings;

    public HomeViewModel(HomeSettings settings = null)
    {
        // use the settings we were passed in (if any) or use current
        settings ??= HomeSettings.Load();

        LoadSettings(settings);

        Messenger.Default.Register<ActionMessage>(this, OnActionMessage);
    }

    [GenerateCommand]
    public void ShowLibraryGrid()
    {
        if (LibraryViewType == LibraryView.DataGrid) return;
        
        LoadView(LibraryView.DataGrid);

        SaveSettings();
    }

    [GenerateCommand]
    public void ShowLibraryTile()
    {
        if (LibraryViewType == LibraryView.Tile) return;
        
        LoadView(LibraryView.Tile);

        SaveSettings();
    }

    private void LoadView(LibraryView type)
    {
        if (Settings == null) throw new ArgumentNullException(nameof(Settings));

        try
        {
            LibraryViewType = type;
            Settings.View = type;

            if (type == LibraryView.Tile)
            {
                var tileSettings = Settings.TileSettings;
                var vm = new LibraryTileViewModel(tileSettings);
                
                CurrentVm = vm;
                CurrentSettings = tileSettings;

                return;
            }

            if (type == LibraryView.DataGrid)
            {
                var gridSettings = Settings.GridSettings;
                var vm = new LibraryDataGridViewModel(gridSettings);
                
                CurrentVm = vm;
                CurrentSettings = gridSettings;

                return;
            }

            throw new ArgumentOutOfRangeException($"{nameof(LibraryView)} {type:G} is not supported.");
        }
        catch (Exception e)
        {
            log.Error($"An error occurred attempting to load the {type:G} {nameof(LibraryView)}. {e.Message}", e);
        }
    }
    
    private void OnActionMessage(ActionMessage obj)
    {
        if (obj.EntityType == EntityType.HomeSettings && obj.ActionType == ActionType.Changed)
        {
            SaveSettings();
        }
    }

    private void LoadSettings(HomeSettings settings)
    {
        Settings = settings;
        
        LoadView(settings.View);
    }

    private void SaveSettings()
    {
        if (Settings == null) return;

        Settings?.Save();
    }
}
