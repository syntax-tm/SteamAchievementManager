using System.ComponentModel;

namespace SAM;

public interface ILibrarySettings : INotifyPropertyChanged
{
    bool EnableGrouping { get; set; }
    bool ShowFavoritesOnly { get; set; }
    bool ShowHidden { get; set; }

    void Save();
}
