using System.Globalization;

namespace SAM.API.Stats
{
    public class IntStatInfo : StatInfo
    {
        public int IntValue;
        public int OriginalValue;

        public override object Value
        {
            get => IntValue;
            set
            {
                var i = int.Parse((string)value, CultureInfo.CurrentCulture);

                if ((Permission & 2) != 0 &&
                    IntValue != i)
                    throw new StatIsProtectedException();

                IntValue = i;
            }
        }

        public override bool IsModified => IntValue != OriginalValue;
    }
}
