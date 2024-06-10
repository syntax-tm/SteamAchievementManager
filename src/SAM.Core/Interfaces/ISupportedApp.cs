namespace SAM.Core.Interfaces;

public interface ISupportedApp
{
    uint Id { get; init; }
    string Type { get; init; }
    string Name { get; init; }

    GameInfoType GameInfoType { get; }
}
