using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using SAM.Core.Extensions;
using SAM.Core.Storage;

namespace SAM.Core
{
    public static class LocationHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LocationHelper));
        
        private const string COUNTRIES_STATES_JSON_URL = @"https://raw.githubusercontent.com/RudeySH/SteamCountries/master/json/countries-states.min.json";

        private static bool _isInitialized;

        private static List<SteamCountry> _countries;

        public static string GetShortLocation(string original)
        {
            if (string.IsNullOrEmpty(original)) throw new ArgumentNullException(nameof(original));

            Init();

            var location = new SteamLocation(original);

            return location.GetShortDisplayText();
        }

        private static void Init()
        {
            if (_isInitialized) return;

            var fileName = Path.GetFileName(COUNTRIES_STATES_JSON_URL);
            var cacheKey = new CacheKey(fileName, CacheKeyType.Data);
            var countryStateJson = WebManager.DownloadString(COUNTRIES_STATES_JSON_URL, cacheKey);

            var jo = JObject.Parse(countryStateJson);
            var countriesToken = jo.SelectToken("countries");

            var countries = countriesToken!.ToObject<SteamCountry[]>();

            _countries = new (countries);

            _isInitialized = true;
        }

        internal class SteamLocation
        {
            private readonly string _location;

            public bool IsValid { get; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }

            public static SteamLocation Default => new ()
            {
                City = string.Empty,
                State = string.Empty,
                Country = string.Empty
            };

            protected SteamLocation()
            {

            }

            public SteamLocation(string location)
            {
                _location = location;

                var segments = location.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (segments.Length is < 2 or > 3)
                {
                    // if it's anything else don't bother trying to shorten it
                    log.Warn($"Unknown location format ('{location}'). Location will not be shortened.");
                    return;
                }
                
                // city, state, country
                if (segments.Length == 3)
                {
                    Country = segments[2];
                    State = segments[1];
                    City = segments[0];
                }
                // city, country
                else
                {
                    Country = segments[1];
                    City = segments[0];
                }

                IsValid = true;
            }

            public string GetShortDisplayText()
            {
                if (!IsValid) return _location;

                var country = _countries.FirstOrDefault(c => c.Name.EqualsIgnoreCase(Country));
                if (country == null)
                {
                    return _location;
                }

                var countryCode = country.Code;
                var stateCode = State != null
                    ? country.GetStateByName(State)?.Code
                    : State;
                
                return !string.IsNullOrEmpty(stateCode)
                    ? $"{City}, {stateCode}, {countryCode}"
                    : $"{City}, {countryCode}";
            }
        }

        internal class SteamCountry
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public bool HasStates => States != null && States.Any();
            public List<SteamState> States { get; set; } = new ();

            public SteamState GetStateByName(string name)
            {
                return States.FirstOrDefault(s => s.Name.EqualsIgnoreCase(name));
            }
        }

        internal class SteamState
        {
            public string Code { get; }
            public string Name { get; }

            public SteamState(string code, string name)
            {
                Code = code;
                Name = name;
            }
        }
    }
}
