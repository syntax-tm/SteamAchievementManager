using System;
using SAM.API.Types;

namespace SAM.API.Stats
{
    public abstract class StatDefinition
    {
        public string DisplayName;
        public string Id;
        public int Permission;

        protected static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString();
            if (!string.IsNullOrEmpty(name)) return name;

            if (language != @"english")
            {
                name = kv[@"english"].AsString();

                if (!string.IsNullOrEmpty(name)) return name;
            }

            name = kv.AsString();
            return string.IsNullOrEmpty(name) ? defaultValue : name;
        }

        public static StatDefinition Create<T>(KeyValue stat, string currentLanguage)
            where T : StatDefinition
        {
            if (typeof(T) == typeof(IntegerStatDefinition))
            {
                return new IntegerStatDefinition(stat, currentLanguage);
            }
            if (typeof(T) == typeof(FloatStatDefinition))
            {
                return new FloatStatDefinition(stat, currentLanguage);
            }
            if (typeof(T) == typeof(FloatStatDefinition))
            {
                return new FloatStatDefinition(stat, currentLanguage);
            }

            throw new ArgumentOutOfRangeException(typeof(T).Name);
        }
    }
}
