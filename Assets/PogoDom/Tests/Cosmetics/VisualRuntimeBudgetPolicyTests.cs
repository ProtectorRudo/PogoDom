using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VisualRuntimeBudgetPolicyTests
    {
        [Test]
        public void QualityBudgetsScaleUpMonotonically()
        {
            var lite = VisualRuntimeBudgetPolicy.Get(VisualQualityTier.Lite);
            var balanced = VisualRuntimeBudgetPolicy.Get(VisualQualityTier.Balanced);
            var showcase = VisualRuntimeBudgetPolicy.Get(VisualQualityTier.Showcase);

            Assert.LessOrEqual(lite.MaxUniqueToonMaterials, balanced.MaxUniqueToonMaterials);
            Assert.LessOrEqual(balanced.MaxUniqueToonMaterials, showcase.MaxUniqueToonMaterials);
            Assert.LessOrEqual(lite.MaxMeshRenderers, balanced.MaxMeshRenderers);
            Assert.LessOrEqual(balanced.MaxMeshRenderers, showcase.MaxMeshRenderers);
            Assert.AreEqual(4, lite.MaxTrailRenderers);
            Assert.AreEqual(4, balanced.MaxTrailRenderers);
            Assert.AreEqual(4, showcase.MaxTrailRenderers);
        }

        [TestCase("Tile_0_0")]
        [TestCase("Tile_0_0_Glow")]
        [TestCase("Telegraph_Tnt_4")]
        [TestCase("BurstCore")]
        [TestCase("CombatTrace_2")]
        public void DynamicStateVisualsAreNeverMaterialSharingCandidates(string name)
        {
            Assert.IsTrue(VisualRuntimeBudgetPolicy.IsDynamicColorObject(name));
            Assert.IsFalse(VisualRuntimeBudgetPolicy.IsShareableToonCandidate(name, false));
        }

        [TestCase("Missile_4")]
        [TestCase("BankCrate_2")]
        [TestCase("RocketNose")]
        [TestCase("LockBody")]
        [TestCase("PickupHalo")]
        public void StaticPickupPresentationCanShareEquivalentToonMaterials(string name)
        {
            Assert.IsTrue(VisualRuntimeBudgetPolicy.IsShareableToonCandidate(name, false));
        }

        [Test]
        public void AvatarStaticMeshesCanShareButDynamicTileCannotEvenInsideAvatarFlag()
        {
            Assert.IsTrue(VisualRuntimeBudgetPolicy.IsShareableToonCandidate("Head", true));
            Assert.IsTrue(VisualRuntimeBudgetPolicy.IsShareableToonCandidate("EquippedHeadwear", true));
            Assert.IsFalse(VisualRuntimeBudgetPolicy.IsShareableToonCandidate("Tile_3_3", true));
        }

        [Test]
        public void PresentationHashIsStableAndSensitiveToName()
        {
            const string value = "MysteryCrate_17";
            var first = VisualRuntimeBudgetPolicy.StablePresentationHash(value);
            for (var i = 0; i < 100; i++)
                Assert.AreEqual(first, VisualRuntimeBudgetPolicy.StablePresentationHash(value));

            Assert.AreNotEqual(first, VisualRuntimeBudgetPolicy.StablePresentationHash("MysteryCrate_18"));
            Assert.That(VisualRuntimeBudgetPolicy.StablePhase01(value), Is.InRange(0f, 1f));
        }

        [Test]
        public void FnvHashHasPinnedValueSoImplementationCannotSilentlyDrift()
        {
            Assert.AreEqual(3826002220u, VisualRuntimeBudgetPolicy.StablePresentationHash("a"));
        }
    }
}
