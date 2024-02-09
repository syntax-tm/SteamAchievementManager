using SAM.API.Stats;

namespace SAM.API;

public class UserStatsReceived : Callback<UserStatsResponse>
{
	public override int Id => 1101;
	public override bool IsServer => false;
}
