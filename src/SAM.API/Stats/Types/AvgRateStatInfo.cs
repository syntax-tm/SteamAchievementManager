using SAM.API.Types;

namespace SAM.API.Stats;

public class AvgRateStatInfo : FloatStatInfo
{
	public override UserStatType Type => UserStatType.AverageRate;

	public AvgRateStatInfo ()
	{

	}

	public AvgRateStatInfo (KeyValue stat, string currentLanguage) : base(stat, currentLanguage)
	{

	}
}
