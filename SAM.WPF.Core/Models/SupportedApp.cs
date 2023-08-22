using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    [DebuggerDisplay("{Id} ({Type})")]
    public class SupportedApp
    {

        private GameInfoType? _gameInfoType;

        public uint Id { get; set; }
        public string Type { get; set; }

        public GameInfoType GameInfoType
        {
            get
            {
                _gameInfoType ??= Enum.Parse<GameInfoType>(Type, true);

                return _gameInfoType.Value;
            }
        }

        public SupportedApp()
        {

        }

        public SupportedApp(uint id, string type)
        {
            Id = id;
            Type = type;
        }

        public SupportedApp(KeyValuePair<uint, string> kvPair)
        {
            Id = kvPair.Key;
            Type = kvPair.Value;
        }
    }
}
