using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using SAM.Core.Messages;
using SAM.Extensions;

namespace SAM;

[GenerateViewModel]
public partial class LibrarySettings
{
    [GenerateProperty(OnChangedMethod = nameof(Changed))] private bool _showHidden;
    [GenerateProperty(OnChangedMethod = nameof(Changed))] private bool _showFavoritesOnly;
    [GenerateProperty(OnChangedMethod = nameof(Changed))] private bool _enableGrouping = true;
    
    [GenerateCommand]
    public void ToggleShowHidden()
    {
        ShowHidden = !ShowHidden;
    }

    [GenerateCommand]
    public void ToggleShowFavoritesOnly()
    {
        ShowFavoritesOnly = !ShowFavoritesOnly;
    }

    [GenerateCommand]
    public void ToggleGrouping()
    {
        EnableGrouping = !EnableGrouping;
    }

    protected virtual void Changed()
    {
        Messenger.Default.SendAction(EntityType.HomeSettings, ActionType.Changed);
    }
}
