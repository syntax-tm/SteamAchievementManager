using SAM.Core.AppInfo;

namespace SAM.Core.Interfaces;

public interface IAppInfo
{

    AppInfoType AppInfoType { get; }
    string Type { get; }
    string Name { get; }
    uint AppId { get; }

}
