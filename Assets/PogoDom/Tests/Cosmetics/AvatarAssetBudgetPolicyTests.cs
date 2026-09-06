using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarAssetBudgetPolicyTests
    {
        [Test]
        public void LaunchBudgetIsStrictEnoughForFourSimultaneousAvatars()
        {
            var budget = AvatarAssetBudgetPolicy.MobileLaunch;
            Assert.LessOrEqual(budget.MaxTriangles, 40000);
            Assert.LessOrEqual(budget.MaxSkinnedMeshRenderers, 3);
            Assert.LessOrEqual(budget.MaxMaterialSlots, 10);
            Assert.LessOrEqual(budget.MaxBones, 90);
            Assert.LessOrEqual(budget.MaxEmbeddedParticleSystems, 1);
            Assert.AreEqual(1, budget.MaxAnimators);
            Assert.AreEqual(0, budget.MaxLights);
            Assert.AreEqual(0, budget.MaxClothComponents);
            Assert.AreEqual(0, budget.MaxAudioSources);
        }

        [Test]
        public void HealthyLaunchAvatarPassesBudget()
        {
            Assert.IsTrue(AvatarAssetBudgetPolicy.IsWithinBudget(
                AvatarAssetBudgetPolicy.MobileLaunch,
                triangles: 26000,
                skinnedMeshRenderers: 2,
                materialSlots: 6,
                bones: 64,
                embeddedParticleSystems: 0,
                animators: 1,
                lights: 0,
                clothComponents: 0,
                audioSources: 0));
        }

        [TestCase(50000, 2, 6, 64, 0, 1, 0, 0, 0)]
        [TestCase(26000, 4, 6, 64, 0, 1, 0, 0, 0)]
        [TestCase(26000, 2, 11, 64, 0, 1, 0, 0, 0)]
        [TestCase(26000, 2, 6, 100, 0, 1, 0, 0, 0)]
        [TestCase(26000, 2, 6, 64, 2, 1, 0, 0, 0)]
        [TestCase(26000, 2, 6, 64, 0, 2, 0, 0, 0)]
        [TestCase(26000, 2, 6, 64, 0, 1, 1, 0, 0)]
        [TestCase(26000, 2, 6, 64, 0, 1, 0, 1, 0)]
        [TestCase(26000, 2, 6, 64, 0, 1, 0, 0, 1)]
        public void AnySingleBudgetViolationRejectsAvatar(
            int triangles,
            int skinned,
            int materials,
            int bones,
            int particles,
            int animators,
            int lights,
            int cloth,
            int audio)
        {
            Assert.IsFalse(AvatarAssetBudgetPolicy.IsWithinBudget(
                AvatarAssetBudgetPolicy.MobileLaunch,
                triangles,
                skinned,
                materials,
                bones,
                particles,
                animators,
                lights,
                cloth,
                audio));
        }

        [Test]
        public void NegativeCountsAreAlwaysInvalid()
        {
            Assert.IsFalse(AvatarAssetBudgetPolicy.IsWithinBudget(
                AvatarAssetBudgetPolicy.MobileLaunch,
                -1, 1, 1, 1, 0, 1, 0, 0, 0));
        }
    }
}
