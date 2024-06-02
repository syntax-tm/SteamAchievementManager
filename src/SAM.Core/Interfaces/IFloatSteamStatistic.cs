namespace SAM.Core.Interfaces;

public interface IFloatSteamStatistic : ISteamStatistic
{
    float OriginalValue { get; set;}
    float Value { get; set;}
}