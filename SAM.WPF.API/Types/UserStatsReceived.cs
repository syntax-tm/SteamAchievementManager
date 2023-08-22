using System.Runtime.InteropServices;

namespace SAM.API.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserStatsReceived
    {
        public ulong GameId;
        public int Result;

        public bool IsSuccess => Result == 1;
    }
}
