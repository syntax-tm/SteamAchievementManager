using System.Diagnostics;
using SAM.API.Stats;
using SAM.Core;
using SAM.Core.Interfaces;
using SAM.Managers;

namespace SAM.Stats;

[DebuggerDisplay("{DisplayName} ({Id})")]
public class IntegerSteamStatistic : SteamStatisticBase, IIntegerSteamStatistic
{
    public override StatType StatType => StatType.Integer;

    public int OriginalValue
    {
        get => GetProperty(() => OriginalValue);
        set => SetProperty(() => OriginalValue, value);
    }
    public int Value
    {
        get => GetProperty(() => Value);
        set => SetProperty(() => Value, value, OnValueChanged);
    }

    public IntegerSteamStatistic()
    {

    }

    public IntegerSteamStatistic(StatInfoBase stat) : base(stat)
    {
        Refresh();

        _loading = false;
    }

    public override object GetValue()
    {
        return Value;
    }

    public override void Reset()
    {
        Value = OriginalValue;
        IsModified = false;
    }

    public override void Refresh()
    {
        _loading = true;

        var success = SteamClientManager.Default.SteamUserStats.GetStatValue(Id, out int value);
        if (success)
        {
            Value = value;
        }
        else
        {
            log.Warn($"Failed to get {StatType} stat '{Id}' value.");
        }

        OriginalValue = Value;

        IsModified = false;

        _loading = false;
    }

    protected void OnValueChanged()
    {
        if (_loading) return;

        IsModified = Value != OriginalValue;
    }
}
