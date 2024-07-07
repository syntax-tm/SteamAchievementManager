using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using SAM.Core.Messages;
using SAM.Extensions;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class LibraryTileViewModel : LibraryViewModel
{
    protected bool _imagesLoaded;

    [GenerateProperty] private LibraryTileSettings _settings;
    [GenerateProperty] protected bool _showImages;
    [GenerateProperty] protected bool _localImagesOnly;

    public LibraryTileViewModel(LibraryTileSettings settings) : base(settings)
    {
        Settings = settings;

        Refresh();

        _loading = false;
    }

    [GenerateCommand]
    public void ToggleShowImages()
    {
        if (Settings == null) return;

        Settings.ShowImages = !Settings.ShowImages;
    }

    [GenerateCommand]
    public void ToggleLocalImagesOnly()
    {
        if (Settings == null) return;

        Settings.LocalImagesOnly = !Settings.LocalImagesOnly;
    }

    [GenerateCommand]
    public async Task LoadAllImages()
    {
        if (Library == null) return;
        if (_imagesLoaded) return;

        var apps = Library.Items.ToList();
        var loadImageTasks = apps.Select(a => a.LoadImagesAsync()).ToArray();
        await Task.WhenAll(loadImageTasks).ConfigureAwait(false);

        _imagesLoaded = true;
    }

    public bool CanLoadAllImages()
    {
        return !_imagesLoaded;
    }

    protected void OnLocalImagesOnlyChanged()
    {
        if (_loading) return;

        _settings.LocalImagesOnly = LocalImagesOnly;

        Messenger.Default.SendAction(EntityType.HomeSettings, ActionType.Changed);
    }

    protected void OnShowImagesChanged()
    {
        if (_loading) return;

        _settings.ShowImages = LocalImagesOnly;

        Messenger.Default.SendAction(EntityType.HomeSettings, ActionType.Changed);

        Refresh();
    }

}
