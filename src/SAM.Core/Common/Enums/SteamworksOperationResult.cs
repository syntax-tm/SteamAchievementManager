using System.ComponentModel;

namespace SAM.Core;

[DefaultValue(None)]
public enum SteamworksOperationResult
{
    Invalid = -3,
    RateLimited = -2,
    Failed = -1,
    None = 0,
    Success = 1,
    Skipped = 2
}
