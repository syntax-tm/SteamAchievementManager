using System.Diagnostics;
using SAM.API.Stats;

namespace SAM.Core.Stats
{
    [DebuggerDisplay("{DisplayName} ({Id})")]
    public class AverageRateSteamStatistic : SteamStatisticBase
    {
        public override StatType StatType => StatType.AvgRate;
        
        public float Value
        {
            get => GetProperty(() => Value);
            set => SetProperty(() => Value, value);
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

        public AverageRateSteamStatistic()
        {

        }

        public AverageRateSteamStatistic(StatInfoBase stat) : base(stat)
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

            AvgRateNumerator = 0;
            AvgRateDenominator = 0;
            IsModified = false;

            _loading = false;
        }

        protected void OnAvgRateValueChanged()
        {
            if (_loading) return;

            IsModified = AvgRateDenominator > 0 || AvgRateNumerator > 0;
        }
    }
}
