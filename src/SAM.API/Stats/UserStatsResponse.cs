using System.Runtime.InteropServices;

namespace SAM.API.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct UserStatsResponse
	{
		public ulong GameId;
		public int Result;

		public readonly bool IsSuccess => Result == 1;
	}
}
