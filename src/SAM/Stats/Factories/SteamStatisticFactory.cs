using System;
using SAM.API;
using SAM.API.Stats;

namespace SAM.Stats;

public static class SteamStatisticFactory
{
    /// <summary>
    /// Creates an instance of <see cref="SteamStatisticBase"/> based on the supplied <see cref="StatInfoBase"/> <paramref name="stat"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="infoBase"></param>
    /// <returns>
    ///   An instance of <see cref="SteamStatisticBase"/>. For example, if <paramref name="stat"/> is of type <see cref="IntegerStatInfo"/>
    ///   then a <see cref="IntegerSteamStatistic"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">Occurs when the <paramref name="client"/>, <paramref name="stat"/>, or the <paramref name="stat"/> <see cref="StatInfoBase.Id"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when an unkown <paramref name="stat"/> type is supplied.</exception>
    public static SteamStatisticBase CreateStat(Client client, StatInfoBase stat)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrEmpty(stat?.Id)) throw new ArgumentNullException(nameof(stat));

        switch (stat)
        {
            // STAT_INT
            case IntegerStatInfo intDefinition:
            {
                if (!client.SteamUserStats.GetStatValue(intDefinition.Id, out int value)) break;

                intDefinition.Value = value;

                return new IntegerSteamStatistic(intDefinition);
            }
            // STAT_AVGRATE
            case AvgRateStatInfo avgRateDefinition:
            {
                if (!client.SteamUserStats.GetStatValue(avgRateDefinition.Id, out float value)) break;

                avgRateDefinition.Value = value;

                // NOTE: average rate stats are always treated as floats
                return new FloatSteamStatistic(avgRateDefinition);
            }
            // STAT_FLOAT
            case FloatStatInfo floatDefinition:
            {
                if (!client.SteamUserStats.GetStatValue(floatDefinition.Id, out float value)) break;

                floatDefinition.Value = value;

                return new FloatSteamStatistic(floatDefinition);
            }
        }

        throw new ArgumentOutOfRangeException(nameof(stat));
    }

}
