namespace SAM.API.Callbacks
{
    public class UserStatsReceived : Callback<Types.UserStatsReceived>
    {
        public override int Id => 1101;

        public override bool IsServer => false;
    }
}
