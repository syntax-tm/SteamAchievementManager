namespace SAM.Core.Interfaces;

public interface IIntegerSteamStatistic : ISteamStatistic
{
    int OriginalValue { get; set;}
    int Value { get; set;}
}