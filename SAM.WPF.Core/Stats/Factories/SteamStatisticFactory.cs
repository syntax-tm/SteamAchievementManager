using System;
using SAM.API;
using SAM.API.Stats;

namespace SAM.WPF.Core.Stats
{
    public static class SteamStatisticFactory
    {

        public static SteamStatistic CreateStat(Client client, StatDefinition definition)
        {
            if (string.IsNullOrEmpty(definition?.Id)) throw new ArgumentNullException(nameof(definition));

            switch (definition)
            {
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

                    return new SteamStatistic(intStat);
                }
                case FloatStatDefinition floatDefinition:
                {
                    if (!client.SteamUserStats.GetStatValue(floatDefinition.Id, out float value)) break;

                    var floatStat = new FloatStatInfo
                    {
                        Id = floatDefinition.Id,
                        DisplayName = floatDefinition.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatDefinition.IncrementOnly,
                        Permission = floatDefinition.Permission,
                    };

                    return new SteamStatistic(floatStat);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(definition));
        }

    }
}
