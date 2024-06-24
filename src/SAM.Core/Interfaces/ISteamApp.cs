using System.Drawing;

namespace SAM.Core.Interfaces;

public interface ISteamApp
{
    uint Id { get; set; }
    string Name { get; set; }
    GameInfoType GameInfoType { get; set; }
    bool IsLoading { get; set; }
    bool Loaded { get; set; }
    string Publisher { get; set; }
    string Developer { get; set; }
    ISteamStoreApp StoreInfo { get; set; }
    string Group { get; }
    bool IsHidden { get; set; }
    bool IsFavorite { get; set; }
    bool IsMenuOpen { get; set; }

    bool IsJunk { get; }
    bool IsDemo { get; }
    bool IsNormal { get; }
    bool IsTool { get; }
    bool IsMod { get; }
    bool StoreInfoLoaded { get; }
}
