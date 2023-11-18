using SAM.API.Types;

namespace SAM.API.Stats;

public static class UserStatsFactory
{
    public static AchievementInfo Create(KeyValue bit, string currentLanguage)
    {
        var id = bit[@"name"].AsString();
        var name = bit[@"display"][@"name"];
        var desc = bit[@"display"][@"desc"];

        var achievement = new AchievementInfo
        {
            Id = id,
            Name = name.GetLocalizedString(currentLanguage, id),
            Description = desc.GetLocalizedString(currentLanguage, string.Empty),
            IconNormal = bit[@"display"][@"icon"].AsString(),
            IconLocked = bit[@"display"][@"icon_gray"].AsString(),
            IsHidden = bit[@"display"][@"hidden"].AsBoolean(),
            Permission = bit[@"permission"].AsInteger()
        };

        return achievement;
    }
}
