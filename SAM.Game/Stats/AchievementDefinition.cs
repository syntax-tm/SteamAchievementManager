namespace SAM.Game.Stats
{
    public class AchievementDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public string IconNormal;
        public string IconLocked;
        public bool IsHidden;
        public int Permission;

        public AchievementDefinition()
        {

        }

        public AchievementDefinition(KeyValue bit, string currentLanguage)
        {
            Id = bit[@"name"].AsString();
            
            Name = GetLocalizedString(bit[@"display"][@"name"], currentLanguage, Id);
            Description = GetLocalizedString(bit[@"display"][@"desc"], currentLanguage, string.Empty);
            
            IconNormal = bit[@"display"][@"icon"].AsString();
            IconLocked = bit[@"display"][@"icon_gray"].AsString();
            IsHidden = bit[@"display"][@"hidden"].AsBoolean();
            Permission = bit[@"permission"].AsInteger();
        }

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

        public override string ToString()
        {
            var name = Name ?? Id ?? base.ToString();
            return $"{name}: {Permission}";
        }
    }
}
