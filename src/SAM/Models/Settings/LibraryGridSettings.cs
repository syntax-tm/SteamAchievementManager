using DevExpress.Mvvm.CodeGenerators;

namespace SAM;

[GenerateViewModel(ImplementISupportParentViewModel = true)]
public partial class LibraryGridSettings : LibrarySettings
{
    [GenerateProperty(OnChangedMethod = nameof(Changed))]
    private bool _showInstallPath;

    protected override void Changed()
    {
        base.Changed();
    }
}
