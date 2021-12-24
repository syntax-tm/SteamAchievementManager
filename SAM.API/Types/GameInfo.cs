using System.Globalization;

namespace SAM.API.Types
{
    public class GameInfo
    {
        private string _Name;

        public uint Id;
        public int ImageIndex;

        public string Logo;
        public string Type;

        public string Name
        {
            get => _Name;
            set => _Name = value ?? "App " + Id.ToString(CultureInfo.InvariantCulture);
        }

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
