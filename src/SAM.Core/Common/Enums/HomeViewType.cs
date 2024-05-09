using System.ComponentModel;

namespace SAM.Core;

[DefaultValue(FlowLayout)]
public enum HomeViewType
{
    /// <summary>
    /// Uses the LibraryItemsControlView layout for the home screen.
    /// </summary>
    FlowLayout = 0,
    /// <summary>
    /// Uses the LibraryDataGridView layout for the home screen.
    /// </summary>
    DataGrid = 1
}
