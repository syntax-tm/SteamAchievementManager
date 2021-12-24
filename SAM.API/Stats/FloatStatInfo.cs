using System.Globalization;

namespace SAM.API.Stats
{
    public class FloatStatInfo : StatInfo
    {
        public float FloatValue;
        public float OriginalValue;

        public override object Value
        {
            get => FloatValue;
            set
            {
                var f = float.Parse((string)value, CultureInfo.CurrentCulture);

                if ((Permission & 2) != 0 && !FloatValue.Equals(f)) throw new StatIsProtectedException();

                FloatValue = f;
            }
        }

        public override bool IsModified => !FloatValue.Equals(OriginalValue);
    }
}
