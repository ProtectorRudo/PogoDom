using System;

namespace PogoDom.Meta
{
    public sealed class CityState
    {
        public CityId Id { get; }
        public NationId NationId { get; }
        public long Population { get; }
        public CityTier Tier { get; }
        public int BotRosterSize { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; internal set; }
        public long GlobalPoints { get; internal set; }
        public CityId? ConqueredByCity { get; internal set; }

        public bool IsConquered => CurrentHp <= 0 && ConqueredByCity.HasValue;

        public CityState(CityId id, NationId nationId, long population)
        {
            if (population < 0) throw new ArgumentOutOfRangeException(nameof(population));
            Id = id;
            NationId = nationId;
            Population = population;
            var spec = CityTierPolicy.FromPopulation(population);
            Tier = spec.Tier;
            MaxHp = spec.MaxHp;
            BotRosterSize = spec.BotRosterSize;
            CurrentHp = MaxHp;
        }
    }
}
