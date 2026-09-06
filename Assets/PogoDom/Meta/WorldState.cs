using System;
using System.Collections.Generic;

namespace PogoDom.Meta
{
    public sealed class WorldState
    {
        private readonly Dictionary<CityId, CityState> _cities = new Dictionary<CityId, CityState>();
        private readonly Dictionary<NationId, NationState> _nations = new Dictionary<NationId, NationState>();
        private readonly HashSet<string> _processedMatches = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyDictionary<CityId, CityState> Cities => _cities;
        public IReadOnlyDictionary<NationId, NationState> Nations => _nations;
        public List<ObjectiveState> Objectives { get; } = new List<ObjectiveState>();

        public void AddNation(NationState nation)
        {
            if (nation == null) throw new ArgumentNullException(nameof(nation));
            _nations.Add(nation.Id, nation);
        }

        public void AddCity(CityState city)
        {
            if (city == null) throw new ArgumentNullException(nameof(city));
            if (!_nations.ContainsKey(city.NationId))
                throw new InvalidOperationException("A city's nation must exist before the city is registered.");
            _cities.Add(city.Id, city);
        }

        public CityState City(CityId id)
        {
            CityState city;
            if (!_cities.TryGetValue(id, out city)) throw new KeyNotFoundException("Unknown city: " + id);
            return city;
        }

        public NationState Nation(NationId id)
        {
            NationState nation;
            if (!_nations.TryGetValue(id, out nation)) throw new KeyNotFoundException("Unknown nation: " + id);
            return nation;
        }

        public bool WasProcessed(string matchId)
        {
            return !string.IsNullOrWhiteSpace(matchId) && _processedMatches.Contains(matchId);
        }

        internal bool MarkProcessed(string matchId)
        {
            return _processedMatches.Add(matchId);
        }
    }
}
