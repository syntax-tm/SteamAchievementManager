using SAM.API.Types;

namespace SAM.API.Stats
{
    public class IntegerStatDefinition : StatDefinition
    {
        public int DefaultValue;
        public bool IncrementOnly;
        public int MaxChange;
        public int MaxValue;
        public int MinValue;

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
