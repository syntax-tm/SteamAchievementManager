namespace SAM.Game.Stats
{
    public class FloatStatInfo : StatInfo
    {
        public float OriginalValue;
        public float FloatValue;

        public override object Value
        {
            get { return this.FloatValue; }
            set
            {
                var f = float.Parse((string)value, System.Globalization.CultureInfo.CurrentCulture);

                if ((this.Permission & 2) != 0 &&
                    this.FloatValue.Equals(f) == false)
                {
                    throw new StatIsProtectedException();
                }

                this.FloatValue = f;
            }
        }

        public override bool IsModified
        {
            get { return this.FloatValue.Equals(this.OriginalValue) == false; }
        }
    }
}
