namespace SAM.API.Game.Stats
{
    public class FloatStatDefinition : StatDefinition
    {
        public float MinValue;
        public float MaxValue;
        public float MaxChange;
        public bool IncrementOnly;
        public float DefaultValue;

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
