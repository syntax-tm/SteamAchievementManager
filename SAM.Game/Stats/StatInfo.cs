namespace SAM.Game.Stats
{
    public abstract class StatInfo
    {
        public abstract bool IsModified { get; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public abstract object Value { get; set; }
        public bool IsIncrementOnly { get; set; }
        public int Permission { get; set; }

        public string Extra
        {
            get
            {
                var flags = StatFlags.None;
                flags |= this.IsIncrementOnly == false ? 0 : StatFlags.IncrementOnly;
                flags |= ((this.Permission & 2) != 0) == false ? 0 : StatFlags.Protected;
                flags |= ((this.Permission & ~2) != 0) == false ? 0 : StatFlags.UnknownPermission;
                return flags.ToString();
            }
        }
    }
}
