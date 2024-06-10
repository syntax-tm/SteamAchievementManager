using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using SAM.Core.Interfaces;

namespace SAM.Core;

[DebuggerDisplay("{Id} ({Type})")]
public record SupportedApp : ISupportedApp, IEqualityOperators<SupportedApp, SupportedApp, bool>
{
    private GameInfoType? _gameInfoType;

    public uint Id { get; init; }
    public string Type { get; init; }
    public string Name { get; init; }

    public GameInfoType GameInfoType
    {
        get
        {
            _gameInfoType ??= Enum.Parse<GameInfoType>(Type, true);

            return _gameInfoType.Value;
        }
    }

    protected SupportedApp()
    {

    }

    public SupportedApp(uint id, string type)
    {
        Id = id;
        Type = type;
    }

    public SupportedApp(uint id, string type, string name)
    {
        Id = id;
        Type = type;
        Name = name;
    }

    public SupportedApp(KeyValuePair<uint, string> kvPair) => (Id, Type) = kvPair;

    public override string ToString()
    {
        return $"{Id} ({Type})";
    }
}
