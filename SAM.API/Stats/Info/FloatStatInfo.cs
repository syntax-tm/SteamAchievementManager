namespace SAM.API.Stats;

public class FloatStatInfo : StatInfo
{
    public float FloatValue;
    public float OriginalValue;

    public override UserStatType Type { get; }

    public override object Value => FloatValue;
    public override bool IsModified => !FloatValue.Equals(OriginalValue);

    public FloatStatInfo(UserStatType type)
    {
        Type = type;
    }
}
