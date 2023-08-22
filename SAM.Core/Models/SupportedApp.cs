using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SAM.Core
{
    [DebuggerDisplay("{Id} ({Type})")]
    public record SupportedApp
    {
        private GameInfoType? _gameInfoType;

        public uint Id { get; init; }
        public string Type { get; init; }

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

        public SupportedApp(KeyValuePair<uint, string> kvPair)
        {
            (Id, Type) = kvPair;
        }

        public override string ToString()
        {
            return $"{Id} ({Type})";
        }
    }
}
