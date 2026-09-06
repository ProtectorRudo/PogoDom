using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VisualQualityPolicyTests
    {
        [Test]
        public void HigherTierOnlyAddsPresentationBudget()
        {
            var lite = VisualQualityPolicy.Get(VisualQualityTier.Lite);
            var balanced = VisualQualityPolicy.Get(VisualQualityTier.Balanced);
            var showcase = VisualQualityPolicy.Get(VisualQualityTier.Showcase);

            Assert.Less(lite.ParticleMultiplier, balanced.ParticleMultiplier);
            Assert.Less(balanced.ParticleMultiplier, showcase.ParticleMultiplier);
            Assert.Less(lite.MaxConcurrentSpectacleBursts, balanced.MaxConcurrentSpectacleBursts);
            Assert.Less(balanced.MaxConcurrentSpectacleBursts, showcase.MaxConcurrentSpectacleBursts);
            Assert.Less(lite.TrailSeconds, balanced.TrailSeconds);
            Assert.Less(balanced.TrailSeconds, showcase.TrailSeconds);
            Assert.Less(lite.EnvironmentDensity, balanced.EnvironmentDensity);
            Assert.Less(balanced.EnvironmentDensity, showcase.EnvironmentDensity);
        }

        [TestCase(0, 0, 0, VisualQualityTier.Lite)]
        [TestCase(1024, 8000, 50, VisualQualityTier.Lite)]
        [TestCase(2048, 4096, 40, VisualQualityTier.Balanced)]
        [TestCase(8192, 12000, 50, VisualQualityTier.Showcase)]
        public void AutoSelectionIsConservative(int graphicsMb, int systemMb, int shaderLevel, VisualQualityTier expected)
        {
            Assert.AreEqual(expected, VisualQualityPolicy.SelectAuto(graphicsMb, systemMb, shaderLevel));
        }

        [Test]
        public void ParticleScalingNeverInflatesRequestedBudget()
        {
            foreach (VisualQualityTier tier in System.Enum.GetValues(typeof(VisualQualityTier)))
            {
                var budget = VisualQualityPolicy.Get(tier);
                for (var requested = 0; requested <= 100; requested++)
                    Assert.LessOrEqual(budget.ScaleParticles(requested), requested == 0 ? 0 : requested, tier + " requested=" + requested);
            }
        }

        [TestCase("Tile_0_0")]
        [TestCase("Telegraph_Tnt_2")]
        [TestCase("MysteryCrate_9")]
        [TestCase("Missile_7")]
        [TestCase("YOU")]
        [TestCase("BOT A")]
        public void ReadabilityCriticalObjectsAreNeverClassifiedAsDecoration(string objectName)
        {
            Assert.IsTrue(VisualQualityPolicy.IsEssentialVisualName(objectName));
        }

        [TestCase("CityBlock_7")]
        [TestCase("CityLight_3")]
        [TestCase("PickupHalo")]
        [TestCase("SkillOrb_2")]
        public void KnownDecorationRemainsScalable(string objectName)
        {
            Assert.IsFalse(VisualQualityPolicy.IsEssentialVisualName(objectName));
        }
    }
}
