using System;
using System.Collections.ObjectModel;
using SAM.Core.Interfaces;
using ValveKeyValue;

namespace SAM.Core.AppInfo;

public class AppInfo : IAppInfo
{
    private string _name;
    private string _type;

    public uint AppId { get; set; }
    public uint InfoState { get; set; }
    public DateTime LastUpdated { get; set; }
    public ulong Token { get; set; }
    public ReadOnlyCollection<byte> Hash { get; set; }
    public ReadOnlyCollection<byte> BinaryDataHash { get; set; }
    public uint ChangeNumber { get; set; }
    public KVObject Data { get; set; }
    public KVValue CommonData => Data["common"];

    public string Name
    {
        get
        {
            if (_name != null) return _name;
            if (CommonData == null) return null;

            _name = CommonData["name"]?.ToString();

            return _name;
        }
    }

    public string Type
    {
        get
        {
            if (_type != null) return _type;
            if (CommonData == null) return null;

            _type = CommonData["type"]?.ToString();

            return _type;
        }
    }

    public AppInfoType AppInfoType
    {
        get
        {
            if (Enum.TryParse<AppInfoType>(Type, true, out var appInfoType))
            {
                return appInfoType;
            }

            return AppInfoType.Unknown;
        }
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name)) return $"{AppId}";

        return $"{Name} ({AppId})";
    }
}
