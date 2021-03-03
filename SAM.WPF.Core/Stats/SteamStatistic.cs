using System.Diagnostics;
using DevExpress.Mvvm;
using SAM.API.Stats;

namespace SAM.WPF.Core.Stats
{
    [DebuggerDisplay("{DisplayName} ({Id})")]
    public class SteamStatistic : BindableBase
    {
        public string Id => StatInfo?.Id;
        public string DisplayName => StatInfo?.DisplayName;
        public object OriginalValue => StatInfo?.Value;
        public bool IsIncrementOnly => StatInfo?.IsIncrementOnly ?? default;
        public bool IsStatInfoModified => StatInfo?.IsModified ?? default;
        public int Permission => StatInfo?.Permission ?? default;

        public StatInfo StatInfo { get; set; }

        public object Value
        {
            get => GetProperty(() => Value);
            set => SetProperty(() => Value, value, OnValueChanged);
        }
        public bool IsModified
        {
            get => GetProperty(() => IsModified);
            set => SetProperty(() => IsModified, value);
        }

        public SteamStatistic()
        {

        }

        public SteamStatistic(StatInfo stat)
        {
            StatInfo = stat;
        }
        
        public void Reset()
        {
            Value = OriginalValue;
            IsModified = false;
        }

        protected void OnValueChanged()
        {
            IsModified = Value.Equals(OriginalValue);
        }
    }
}
