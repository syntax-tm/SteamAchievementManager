using DevExpress.Mvvm.CodeGenerators;

namespace SAM;

[GenerateViewModel(ImplementISupportParentViewModel = true)]
public partial class LibraryTileSettings : LibrarySettings
{
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showImages = true;
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _localImagesOnly = true;
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private TileSize _tileSize = TileSize.M;

    protected override void Changed()
    {
        base.Changed();
    }
}
