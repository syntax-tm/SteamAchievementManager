#pragma warning disable CA1720

using System.ComponentModel;

namespace SAM.Core;

public enum StatType
{
	[Description("Float")]
	Float,
	[Description("Integer")]
	Integer,
	[Description("Avg Rate")]
	AvgRate,
	[Description("Unknown")]
	Unknown
}
