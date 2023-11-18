using SAM.API.Types;

namespace SAM.API.Stats;

public class FloatStatInfo : StatInfoBase<float>
{
    public float DefaultValue { get; set; }
    public bool IncrementOnly { get; set; }
    public float MaxChange { get; set; }
    public float MaxValue { get; set; }
    public float MinValue { get; set; }
        
    public override UserStatType Type => UserStatType.Float;
    public override float Value { get; set; }

    public FloatStatInfo()
    {
    }

    public FloatStatInfo(KeyValue stat, string currentLanguage)
    {
        var name = stat[@"display"][@"name"];

        Id = stat[@"name"].AsString();
        DisplayName = name.GetLocalizedString(currentLanguage, Id);
        MinValue = stat[@"min"].AsFloat(float.MinValue);
        MaxValue = stat[@"max"].AsFloat(float.MaxValue);
        MaxChange = stat[@"maxchange"].AsFloat();
        IncrementOnly = stat[@"incrementonly"].AsBoolean();
        DefaultValue = stat[@"default"].AsFloat();
        Permission = stat[@"permission"].AsInteger();
    }
}
