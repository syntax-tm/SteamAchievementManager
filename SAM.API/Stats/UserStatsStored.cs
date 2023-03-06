using System.Runtime.InteropServices;

namespace SAM.API.Stats
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserStatsStored
    {
        public readonly ulong GameId;
        public readonly int Result;
    }
}
