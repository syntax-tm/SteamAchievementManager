using SAM.API.Stats;

namespace SAM.API
{
    public record UserStatsReceived : Callback<UserStatsResponse>
    {
        public override int Id => 1101;
        public override bool IsServer => false;
    }
}
