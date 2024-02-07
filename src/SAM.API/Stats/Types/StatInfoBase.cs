namespace SAM.API.Stats;

public abstract class StatInfoBase
{
	public string Id
	{
		get; protected init;
	}
	public string DisplayName
	{
		get; protected init;
	}
	public bool IsIncrementOnly
	{
		get; protected init;
	}
	public int Permission
	{
		get; protected init;
	}
	public abstract UserStatType Type
	{
		get;
	}

	public string Extra
	{
		get
		{
			var flags = StatFlags.None;
			flags |= IsIncrementOnly == false ? 0 : StatFlags.IncrementOnly;
			flags |= (Permission & 2) != 0 == false ? 0 : StatFlags.Protected;
			flags |= (Permission & ~2) != 0 == false ? 0 : StatFlags.UnknownPermission;
			return flags.ToString();
		}
	}
}

public abstract class StatInfoBase<T> : StatInfoBase
{
	public abstract T Value
	{
		get; set;
	}
}
