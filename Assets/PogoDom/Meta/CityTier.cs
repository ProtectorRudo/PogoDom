using System;

namespace PogoDom.Meta
{
    public enum CityTier
    {
        VerySmall = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        Giant = 4
    }

    public readonly struct CityTierSpec
    {
        public CityTier Tier { get; }
        public int MaxHp { get; }
        public int BotRosterSize { get; }

        public CityTierSpec(CityTier tier, int maxHp, int botRosterSize)
        {
            Tier = tier;
            MaxHp = maxHp;
            BotRosterSize = botRosterSize;
        }
    }

    public static class CityTierPolicy
    {
        public static CityTierSpec FromPopulation(long population)
        {
            if (population < 0) throw new ArgumentOutOfRangeException(nameof(population));
            if (population >= 5_000_000) return new CityTierSpec(CityTier.Giant, 1000, 30);
            if (population >= 1_000_000) return new CityTierSpec(CityTier.Large, 2000, 24);
            if (population >= 250_000) return new CityTierSpec(CityTier.Medium, 3000, 18);
            if (population >= 50_000) return new CityTierSpec(CityTier.Small, 4000, 12);
            return new CityTierSpec(CityTier.VerySmall, 5000, 6);
        }
    }
}
