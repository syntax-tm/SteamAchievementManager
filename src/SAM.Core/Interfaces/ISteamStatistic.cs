using SAM.API.Stats;

namespace SAM.Core.Interfaces;

public interface ISteamStatistic
{
    string Id { get; }
    string DisplayName { get; }
    bool IsIncrementOnly { get; }
    int Permission { get; }
    bool IsAverageRate { get; }
    bool IsFloat { get; }
    bool IsInteger { get; }

    StatInfoBase StatInfo { get; }
    abstract StatType StatType { get; }
    bool IsModified { get; set; }
    double Maximum { get; set; }
    double Minimum { get; set; }

    void CommitChanges();
    object GetValue();
    void Reset();
    void Refresh();
}