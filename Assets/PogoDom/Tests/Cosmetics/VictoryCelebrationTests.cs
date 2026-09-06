using System;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VictoryCelebrationTests
    {
        [Test]
        public void Aura67AlternatesHandsWhileKeepingBothPalmsPresented()
        {
            var definition = VictoryCelebrationLibrary.Aura67();
            var first = VictoryCelebrationSampler.Sample(definition, 0.30f);
            var second = VictoryCelebrationSampler.Sample(definition, 0.70f);

            Assert.Greater(first.LeftArmLift, first.RightArmLift);
            Assert.Greater(second.RightArmLift, second.LeftArmLift);
            Assert.Greater(first.LeftPalmUp, 0.90f);
            Assert.Greater(first.RightPalmUp, 0.90f);
            Assert.Greater(second.LeftPalmUp, 0.90f);
            Assert.Greater(second.RightPalmUp, 0.90f);
        }

        [Test]
        public void CelebrationStartsAndEndsAtNeutralPose()
        {
            var definition = VictoryCelebrationLibrary.Aura67();
            AssertNeutral(VictoryCelebrationSampler.Sample(definition, 0f));
            AssertNeutral(VictoryCelebrationSampler.Sample(definition, definition.DurationSeconds));
            AssertNeutral(VictoryCelebrationSampler.Sample(definition, definition.DurationSeconds + 10f));
        }

        [Test]
        public void SamplerIsDeterministicForSameDefinitionAndTime()
        {
            var definition = VictoryCelebrationLibrary.Aura67();
            for (var i = 1; i < 100; i++)
            {
                var t = definition.DurationSeconds * i / 100f;
                var a = VictoryCelebrationSampler.Sample(definition, t);
                var b = VictoryCelebrationSampler.Sample(definition, t);
                Assert.AreEqual(a.BodyBob, b.BodyBob);
                Assert.AreEqual(a.BodyLean, b.BodyLean);
                Assert.AreEqual(a.LeftArmLift, b.LeftArmLift);
                Assert.AreEqual(a.RightArmLift, b.RightArmLift);
                Assert.AreEqual(a.Accent, b.Accent);
            }
        }

        [Test]
        public void Aura67IsARealVictoryCosmeticAndCanBeEquipped()
        {
            var catalog = new CosmeticCatalog();
            var definition = VictoryCelebrationLibrary.Aura67();
            catalog.Add(definition);
            var inventory = new CosmeticInventory();
            inventory.Unlock(definition.Id);
            var loadout = new CosmeticLoadout(new CosmeticId("character-test"), new CosmeticId("pogo-test"));

            new CosmeticEquipService(catalog, inventory).EquipSimple(loadout, definition.Id);

            Assert.AreEqual(CosmeticKind.VictoryEmote, definition.Kind);
            Assert.AreEqual(definition.Id, loadout.VictoryEmoteId);
            Assert.AreEqual(CelebrationRigCapability.TwoHands, definition.RequiredRigCapability);
        }

        [Test]
        public void InvalidCelebrationDurationIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new VictoryCelebrationDefinition(
                new CosmeticId("bad"),
                "Bad",
                CosmeticRarity.Common,
                "celebration://bad",
                VictoryCelebrationMotion.VictoryBounce,
                CelebrationRigCapability.RootOnly,
                0f));
        }

        private static void AssertNeutral(VictoryCelebrationPose pose)
        {
            Assert.AreEqual(0f, pose.BodyBob);
            Assert.AreEqual(0f, pose.BodyLean);
            Assert.AreEqual(0f, pose.BodyYaw);
            Assert.AreEqual(0f, pose.HeadYaw);
            Assert.AreEqual(0f, pose.LeftArmLift);
            Assert.AreEqual(0f, pose.RightArmLift);
            Assert.AreEqual(0f, pose.LeftPalmUp);
            Assert.AreEqual(0f, pose.RightPalmUp);
            Assert.AreEqual(0f, pose.Accent);
        }
    }
}
