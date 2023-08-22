using SAM.API.Game.Stats;

namespace SAM.WPF.Core.Stats
{
    public static class SteamAchievementFactory
    {

        public static AchievementInfo CreateAchievementInfo(AchievementDefinition definition, bool isAchieved)
        {
            // TODO: Create a constructor for AchievementInfo that takes an achievement definition param
            // TODO: Remove AchievementInfo class entirely and just use SteamAchievement
            var info = new AchievementInfo
            {
                Id = definition.Id,
                IsAchieved = isAchieved,
                IconNormal = string.IsNullOrEmpty(definition.IconNormal) ? null : definition.IconNormal,
                IconLocked = string.IsNullOrEmpty(definition.IconLocked) ? definition.IconNormal : definition.IconLocked,
                Permission = definition.Permission,
                Name = definition.Name,
                Description = definition.Description,
            };

            return info;
        }

    }
}
