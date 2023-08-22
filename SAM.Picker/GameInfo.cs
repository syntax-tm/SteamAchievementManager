using System.Globalization;

namespace SAM.Picker
{
    public class GameInfo
    {
        private string _Name;

        public uint Id;
        public string Type;
        public int ImageIndex;

        public string Name
        {
            get { return _Name; }
            set { _Name = value ?? "App " + Id.ToString(CultureInfo.InvariantCulture); }
        }

        public string Logo;

        public GameInfo(uint id, string type)
        {
            Id = id;
            Type = type;
            Name = null;
            ImageIndex = 0;
            Logo = null;
        }
    }
}
