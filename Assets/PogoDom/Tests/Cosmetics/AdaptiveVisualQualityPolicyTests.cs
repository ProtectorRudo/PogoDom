using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AdaptiveVisualQualityPolicyTests
    {
        [Test]
        public void HealthyFramesDrainPressureInsteadOfDowngrading()
        {
            var pressure = 1.8f;
            pressure = AdaptiveVisualQualityPolicy.UpdatePressureSeconds(pressure, 16.7f, 0.5f);
            Assert.AreEqual(0.8f, pressure, 0.0001f);
            Assert.IsFalse(AdaptiveVisualQualityPolicy.ShouldDowngrade(new FramePressureSample(16.7f, pressure)));
        }

        [Test]
        public void BriefHitchDoesNotDowngrade()
        {
            var sample = new FramePressureSample(35f, 0.4f);
            Assert.IsFalse(AdaptiveVisualQualityPolicy.ShouldDowngrade(sample));
        }

        [Test]
        public void SustainedPressureDowngrades()
        {
            var sample = new FramePressureSample(
                AdaptiveVisualQualityPolicy.PressureFrameMilliseconds + 2f,
                AdaptiveVisualQualityPolicy.PressureSecondsToDowngrade);
            Assert.IsTrue(AdaptiveVisualQualityPolicy.ShouldDowngrade(sample));
        }

        [Test]
        public void CriticalPressureDowngradesFaster()
        {
            var belowTime = new FramePressureSample(
                AdaptiveVisualQualityPolicy.CriticalFrameMilliseconds + 5f,
                AdaptiveVisualQualityPolicy.CriticalSecondsToDowngrade - 0.01f);
            var atTime = new FramePressureSample(
                AdaptiveVisualQualityPolicy.CriticalFrameMilliseconds + 5f,
                AdaptiveVisualQualityPolicy.CriticalSecondsToDowngrade);

            Assert.IsFalse(AdaptiveVisualQualityPolicy.ShouldDowngrade(belowTime));
            Assert.IsTrue(AdaptiveVisualQualityPolicy.ShouldDowngrade(atTime));
        }

        [Test]
        public void QualityCanOnlyStepDownAndLiteIsFloor()
        {
            Assert.AreEqual(VisualQualityTier.Balanced, AdaptiveVisualQualityPolicy.NextLower(VisualQualityTier.Showcase));
            Assert.AreEqual(VisualQualityTier.Lite, AdaptiveVisualQualityPolicy.NextLower(VisualQualityTier.Balanced));
            Assert.AreEqual(VisualQualityTier.Lite, AdaptiveVisualQualityPolicy.NextLower(VisualQualityTier.Lite));
        }

        [Test]
        public void AuraDensityDropsBeforeEssentialGameplayVisuals()
        {
            var lite = VisualQualityPolicy.Get(VisualQualityTier.Lite);
            var balanced = VisualQualityPolicy.Get(VisualQualityTier.Balanced);
            var showcase = VisualQualityPolicy.Get(VisualQualityTier.Showcase);

            Assert.Less(lite.MaxAuraOrbiters, balanced.MaxAuraOrbiters);
            Assert.Less(balanced.MaxAuraOrbiters, showcase.MaxAuraOrbiters);
            Assert.GreaterOrEqual(lite.MaxAuraOrbiters, 1);
            Assert.IsTrue(VisualQualityPolicy.IsEssentialVisualName("YOU"));
            Assert.IsTrue(VisualQualityPolicy.IsEssentialVisualName("Tile_0_0"));
            Assert.IsTrue(VisualQualityPolicy.IsEssentialVisualName("Missile_7"));
        }
    }
}
