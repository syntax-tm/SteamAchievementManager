using System;
using System.Diagnostics;
using SAM.API.Stats;

namespace SAM.Core.Stats
{
    [DebuggerDisplay("{DisplayName} ({Id})")]
    public class FloatSteamStatistic : SteamStatisticBase
    {
        public override StatType StatType { get; }
        
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
        public float AvgRateNumerator
        {
            get => GetProperty(() => AvgRateNumerator);
            set => SetProperty(() => AvgRateNumerator, value, OnAvgRateValueChanged);
        }
        public double AvgRateDenominator
        {
            get => GetProperty(() => AvgRateDenominator);
            set => SetProperty(() => AvgRateDenominator, value, OnAvgRateValueChanged);
        }

        public FloatSteamStatistic()
        {

        }

        public FloatSteamStatistic(StatInfoBase stat, StatType type) : base(stat)
        {
            StatType = type;

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
            AvgRateNumerator = 0;
            AvgRateDenominator = 0;
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

            AvgRateNumerator = 0;
            AvgRateDenominator = 0;
            IsModified = false;

            _loading = false;
        }

        protected void OnValueChanged()
        {
            if (_loading) return;
            if (IsAverageRate) return;

            const double TOLERANCE = 0.000000001;

            IsModified = Math.Abs(Value - OriginalValue) < TOLERANCE;
        }

        protected void OnAvgRateValueChanged()
        {
            if (_loading) return;
            if (IsAverageRate) return;

            IsModified = AvgRateDenominator > 0 || AvgRateNumerator > 0;
        }
    }
}
