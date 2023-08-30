using System;
using SAM.API;
using SAM.API.Stats;

namespace SAM.Core.Stats
{
    public static class SteamStatisticFactory
    {

        public static SteamStatisticBase CreateStat(Client client, StatDefinition definition)
        {
            if (string.IsNullOrEmpty(definition?.Id)) throw new ArgumentNullException(nameof(definition));

            switch (definition)
            {
                // STAT_INT
                case IntegerStatDefinition intDefinition:
                {
                    if (!client.SteamUserStats.GetStatValue(definition.Id, out int value)) break;

                    var intStat = new IntStatInfo
                    {
                        Id = definition.Id,
                        DisplayName = definition.DisplayName,
                        IntValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = intDefinition.IncrementOnly,
                        Permission = definition.Permission,
                    };

                    return new IntegerSteamStatistic(intStat);
                }
                // STAT_AVGRATE
                case AvgRateStatDefinition avgRateDefinition:
                {
                    if (!client.SteamUserStats.GetStatValue(avgRateDefinition.Id, out float value)) break;

                    var avgRateStat = new FloatStatInfo(UserStatType.AverageRate)
                    {
                        Id = avgRateDefinition.Id,
                        DisplayName = avgRateDefinition.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = avgRateDefinition.IncrementOnly,
                        Permission = avgRateDefinition.Permission,
                    };

                    return new FloatSteamStatistic(avgRateStat, StatType.AvgRate);
                }
                // STAT_FLOAT
                case FloatStatDefinition floatDefinition:
                {
                    if (!client.SteamUserStats.GetStatValue(floatDefinition.Id, out float value)) break;

                    var floatStat = new FloatStatInfo(UserStatType.Float)
                    {
                        Id = floatDefinition.Id,
                        DisplayName = floatDefinition.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatDefinition.IncrementOnly,
                        Permission = floatDefinition.Permission,
                    };

                    return new FloatSteamStatistic(floatStat, StatType.Float);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(definition));
        }

    }
}
