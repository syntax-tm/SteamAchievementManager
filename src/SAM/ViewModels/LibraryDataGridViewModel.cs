using DevExpress.Mvvm.CodeGenerators;

namespace SAM.ViewModels;

[GenerateViewModel]
public partial class LibraryDataGridViewModel : LibraryViewModel
{
    [GenerateProperty] private LibraryGridSettings _settings;

    public LibraryDataGridViewModel(LibraryGridSettings settings) : base(settings)
    {
        Settings = settings;

        Refresh();

        _loading = false;
    }
}
