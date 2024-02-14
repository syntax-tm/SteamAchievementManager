namespace SAM.API.Stats;

public class AchievementInfo
{
	public string Description
	{
		get; init;
	}
	public string IconLocked
	{
		get; init;
	}
	public string IconNormal
	{
		get; init;
	}
	public string Id
	{
		get; init;
	}
	public bool IsHidden
	{
		get; init;
	}
	public string Name
	{
		get; init;
	}
	public int Permission
	{
		get; init;
	}
	public bool IsAchieved
	{
		get; set;
	}

	public override string ToString ()
	{
		var name = Name ?? Id ?? base.ToString();
		return $"{name}: {Permission}";
	}
}
