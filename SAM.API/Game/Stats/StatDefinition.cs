namespace SAM.API.Game.Stats
{
    public abstract class StatDefinition
    {
        public string Id;
        public string DisplayName;
        public int Permission;

        protected static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            if (language != @"english")
            {
                name = kv[@"english"].AsString();

                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            name = kv.AsString();
            return string.IsNullOrEmpty(name) ? defaultValue : name;
        }
    }
}
