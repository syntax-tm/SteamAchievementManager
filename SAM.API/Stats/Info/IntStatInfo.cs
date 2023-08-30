using System.Globalization;

namespace SAM.API.Stats;

public class IntStatInfo : StatInfo
{
    public int IntValue;
    public int OriginalValue;

    public override UserStatType Type => UserStatType.Integer;

    public override object Value => IntValue;
    public override bool IsModified => IntValue != OriginalValue;
}
