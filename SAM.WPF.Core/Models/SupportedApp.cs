using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using SAM.WPF.Core.Extensions;

namespace SAM.WPF.Core
{
    [DebuggerDisplay("{Id} ({Type})")]
    public class SupportedApp : IEquatable<SupportedApp>
    {

        private GameInfoType? _gameInfoType;

        public uint Id { get; }
        public string Type { get; }

        public GameInfoType GameInfoType
        {
            get
            {
                _gameInfoType ??= Enum.Parse<GameInfoType>(Type, true);

                return _gameInfoType.Value;
            }
        }

        [JsonConstructor]
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
            Id = kvPair.Key;
            Type = kvPair.Value;
        }

        public override string ToString()
        {
            return $"{Id} ({Type})";
        }

        public static bool operator ==(SupportedApp app, SupportedApp otherApp)
        {
            if (ReferenceEquals(app, null))
            {
                return ReferenceEquals(otherApp, null);
            }

            return app.Equals(otherApp);
        }

        public static bool operator !=(SupportedApp app, SupportedApp otherApp)
        {
            return !(app == otherApp);
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is SupportedApp other))
            {
                throw new ArgumentException($"Parameter {nameof(obj)} must be of type {nameof(SupportedApp)}.", nameof(obj));
            }

            return GetHashCode() == other.GetHashCode();
        }

        public bool Equals(SupportedApp other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id && Type.EqualsIgnoreCase(other.Type);
        }

        public override int GetHashCode()
        {
            return $"{Id}{Type}".GetHashCode();
        }

    }
}
