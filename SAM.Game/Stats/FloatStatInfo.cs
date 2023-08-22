namespace SAM.Game.Stats
{
    public class FloatStatInfo : StatInfo
    {
        public float OriginalValue;
        public float FloatValue;

        public override object Value
        {
            get => FloatValue;
            set
            {
                var f = float.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);

                if ((Permission & 2) != 0 && !FloatValue.Equals(f))
                {
                    throw new StatIsProtectedException();
                }

                FloatValue = f;
            }
        }

        public override bool IsModified
        {
            get => !FloatValue.Equals(OriginalValue);
        }
    }
}
