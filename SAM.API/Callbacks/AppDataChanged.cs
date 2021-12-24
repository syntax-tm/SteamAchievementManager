namespace SAM.API.Callbacks
{
    public class AppDataChanged : Callback<Types.AppDataChanged>
    {
        public override int Id => 1001;

        public override bool IsServer => false;
    }
}
