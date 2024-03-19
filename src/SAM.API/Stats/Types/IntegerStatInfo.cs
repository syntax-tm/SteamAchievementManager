using SAM.API.Types;

namespace SAM.API.Stats;

public class IntegerStatInfo : StatInfoBase<int>
{
    public int DefaultValue { get; set; }
    public int MaxChange { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }
        
    public override UserStatType Type => UserStatType.Integer;
    public override int Value { get; set; }
        
    public IntegerStatInfo()
    {
    }

    public IntegerStatInfo(KeyValue stat, string currentLanguage)
    {
        var name = stat[@"display"][@"name"];

        Id = stat[@"name"].AsString();
        DisplayName = name.GetLocalizedString(currentLanguage, Id);
        MinValue = stat[@"min"].AsInteger(int.MinValue);
        MaxValue = stat[@"max"].AsInteger(int.MaxValue);
        MaxChange = stat[@"maxchange"].AsInteger();
        IncrementOnly = stat[@"incrementonly"].AsBoolean();
        DefaultValue = stat[@"default"].AsInteger();
        Permission = stat[@"permission"].AsInteger();
    }
}
