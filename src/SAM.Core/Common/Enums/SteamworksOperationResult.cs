using System.ComponentModel;

namespace SAM.Core;

[DefaultValue(None)]
public enum SteamworksOperationResult
{
    RateLimited = -2,
    Failed = -1,
    None = 0,
    Success = 1
}
