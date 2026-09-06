using NUnit.Framework;
using PogoDom.Meta;

namespace PogoDom.Tests.Meta
{
    public sealed class CityTierPolicyTests
    {
        [TestCase(4_999_999, CityTier.Large, 2000, 24)]
        [TestCase(5_000_000, CityTier.Giant, 1000, 30)]
        [TestCase(999_999, CityTier.Medium, 3000, 18)]
        [TestCase(1_000_000, CityTier.Large, 2000, 24)]
        [TestCase(249_999, CityTier.Small, 4000, 12)]
        [TestCase(250_000, CityTier.Medium, 3000, 18)]
        [TestCase(49_999, CityTier.VerySmall, 5000, 6)]
        [TestCase(50_000, CityTier.Small, 4000, 12)]
        public void PopulationBoundariesMatchProductRules(long population, CityTier tier, int hp, int bots)
        {
            var spec = CityTierPolicy.FromPopulation(population);
            Assert.AreEqual(tier, spec.Tier);
            Assert.AreEqual(hp, spec.MaxHp);
            Assert.AreEqual(bots, spec.BotRosterSize);
        }
    }
}
