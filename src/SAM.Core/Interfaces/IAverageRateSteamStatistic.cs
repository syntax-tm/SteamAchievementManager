namespace SAM.Core.Interfaces;

public interface IAverageRateSteamStatistic : ISteamStatistic
{
    float Value { get; set; }
    float AvgRateNumerator { get; set; }
    double AvgRateDenominator { get; set; }
}