namespace SAM.API.Stats
{
    public class IntStatInfo : StatInfo
    {
        public int OriginalValue;
        public int IntValue;

        public override object Value
        {
            get { return IntValue; }
            set
            {
                var i = int.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);

                if ((Permission & 2) != 0 &&
                    IntValue != i)
                {
                    throw new StatIsProtectedException();
                }

                IntValue = i;
            }
        }

        public override bool IsModified
        {
            get { return IntValue != OriginalValue; }
        }
    }
}
