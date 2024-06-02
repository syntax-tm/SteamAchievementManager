using System;
using System.Diagnostics;
using SAM.API.Stats;
using SAM.Core;
using SAM.Core.Interfaces;

namespace SAM.Stats;

[DebuggerDisplay("{DisplayName} ({Id})")]
public class FloatSteamStatistic : SteamStatisticBase, IFloatSteamStatistic
{
    public override StatType StatType => StatType.Float;

    public float OriginalValue
    {
        get => GetProperty(() => OriginalValue);
        set => SetProperty(() => OriginalValue, value);
    }
    public float Value
    {
        get => GetProperty(() => Value);
        set => SetProperty(() => Value, value, OnValueChanged);
    }

    public FloatSteamStatistic()
    {

    }

    public FloatSteamStatistic(StatInfoBase stat) : base(stat)
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

        var success = SteamClientManager.Default.SteamUserStats.GetStatValue(Id, out float floatValue);
        if (success)
        {
            Value = floatValue;
        }

        if (!success)
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

        const double TOLERANCE = 0.000000001;

        IsModified = Math.Abs(Value - OriginalValue) > TOLERANCE;
    }
}
