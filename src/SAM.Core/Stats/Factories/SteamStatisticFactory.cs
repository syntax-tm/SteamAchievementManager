using System;
using SAM.API;
using SAM.API.Stats;

namespace SAM.Core.Stats;

public static class SteamStatisticFactory
{

	public static SteamStatisticBase CreateStat (Client client, StatInfoBase infoBase)
	{
		if (string.IsNullOrEmpty(infoBase?.Id))
			{
			throw new ArgumentNullException(nameof(infoBase));
		}

		switch (infoBase)
		{
			// STAT_INT
			case IntegerStatInfo intDefinition:
			{
				if (!client.SteamUserStats.GetStatValue(infoBase.Id, out int value))
					{
					break;
				}

				intDefinition.Value = value;

				return new IntegerSteamStatistic(intDefinition);
			}
			// STAT_AVGRATE
			case AvgRateStatInfo avgRateDefinition:
			{
				if (!client.SteamUserStats.GetStatValue(avgRateDefinition.Id, out float value))
					{
					break;
				}

				avgRateDefinition.Value = value;

				return new AverageRateSteamStatistic(avgRateDefinition);
			}
			// STAT_FLOAT
			case FloatStatInfo floatDefinition:
			{
				if (!client.SteamUserStats.GetStatValue(floatDefinition.Id, out float value))
					{
					break;
				}

				floatDefinition.Value = value;

				return new FloatSteamStatistic(floatDefinition);
			}
		}

		throw new ArgumentOutOfRangeException(nameof(infoBase));
	}
}
