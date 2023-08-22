using SAM.API.Types;

namespace SAM.API.Stats
{
    public class FloatStatDefinition : StatDefinition
    {
        public float DefaultValue;
        public readonly bool IncrementOnly;
        public float MaxChange;
        public float MaxValue;
        public float MinValue;

        public FloatStatDefinition()
        {
        }

        public FloatStatDefinition(KeyValue stat, string currentLanguage)
        {
            Id = stat[@"name"].AsString();
            DisplayName = GetLocalizedString(stat[@"display"][@"name"], currentLanguage, Id);
            MinValue = stat[@"min"].AsFloat(float.MinValue);
            MaxValue = stat[@"max"].AsFloat(float.MaxValue);
            MaxChange = stat[@"maxchange"].AsFloat();
            IncrementOnly = stat[@"incrementonly"].AsBoolean();
            DefaultValue = stat[@"default"].AsFloat();
            Permission = stat[@"permission"].AsInteger();
        }
    }
}
