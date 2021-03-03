using SAM.API.Types;

namespace SAM.API.Stats
{
    public class IntegerStatDefinition : StatDefinition
    {
        public int MinValue;
        public int MaxValue;
        public int MaxChange;
        public bool IncrementOnly;
        public int DefaultValue;

        public IntegerStatDefinition()
        {

        }

        public IntegerStatDefinition(KeyValue stat, string currentLanguage)
        {
            Id = stat[@"name"].AsString();
            DisplayName = GetLocalizedString(stat[@"display"][@"name"], currentLanguage, Id);
            MinValue = stat[@"min"].AsInteger(int.MinValue);
            MaxValue = stat[@"max"].AsInteger(int.MaxValue);
            MaxChange = stat[@"maxchange"].AsInteger();
            IncrementOnly = stat[@"incrementonly"].AsBoolean();
            DefaultValue = stat[@"default"].AsInteger();
            Permission = stat[@"permission"].AsInteger();
        }
    }
}
