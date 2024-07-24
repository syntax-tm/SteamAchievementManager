using System;

namespace SAM;

public interface ISteamImageSource
{
    bool IsAnimated { get; set; }
    Uri Uri { get; set; }
}
