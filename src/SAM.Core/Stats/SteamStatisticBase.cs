using System;
using System.Diagnostics;
using System.Reflection;
using DevExpress.Mvvm;
using log4net;
using SAM.API.Stats;

namespace SAM.Core.Stats
{
	[DebuggerDisplay("{DisplayName} ({Id})")]
	public abstract class SteamStatisticBase : BindableBase
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

		protected bool _loading = true;

		public string Id => StatInfo?.Id;
		public string DisplayName => StatInfo?.DisplayName;
		public bool IsIncrementOnly => StatInfo?.IsIncrementOnly ?? default;
		public int Permission => StatInfo?.Permission ?? default;
		public bool IsAverageRate => StatType == StatType.AvgRate;
		public bool IsFloat => StatType is StatType.Float or StatType.Unknown;
		public bool IsInteger => StatType == StatType.Integer;

		public StatInfoBase StatInfo
		{
			get;
		}
		public abstract StatType StatType
		{
			get;
		}

		public bool IsModified
		{
			get => GetProperty(() => IsModified);
			set => SetProperty(() => IsModified, value);
		}

		public double Maximum
		{
			get => GetProperty(() => Maximum);
			set => SetProperty(() => Maximum, value);
		}

		public double Minimum
		{
			get => GetProperty(() => Minimum);
			set => SetProperty(() => Minimum, value);
		}

		protected SteamStatisticBase ()
		{

		}

		protected SteamStatisticBase (StatInfoBase stat)
		{
			StatInfo = stat ?? throw new ArgumentNullException(nameof(stat));

			Reset();
		}

		public virtual void CommitChanges ()
		{
			if (!IsModified)
				return;

			Refresh();
		}

		public abstract object GetValue ();

		public abstract void Reset ();

		public abstract void Refresh ();
	}
}
