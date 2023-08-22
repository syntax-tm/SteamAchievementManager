using System;

namespace SAM.API.Stats
{
    [Flags]
    public enum StatFlags
    {
        None = 0,
        IncrementOnly = 1 << 0,
        Protected = 1 << 1,
        UnknownPermission = 1 << 2,
    }
}
