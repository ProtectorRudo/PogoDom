using System;
using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VictoryCelebrationCollectionTests
    {
        [Test]
        public void DefaultCollectionIsSmallUniqueAndRigDiverse()
        {
            var all = VictoryCelebrationLibrary.All();
            Assert.AreEqual(6, all.Count, "Launch collection should stay curated rather than bloated.");

            var ids = new HashSet<string>();
            var hasRoot = false;
            var hasHands = false;
            var hasHumanoid = false;
            for (var i = 0; i < all.Count; i++)
            {
                Assert.IsTrue(ids.Add(all[i].Id.Value), "Duplicate victory id: " + all[i].Id);
                hasRoot |= all[i].RequiredRigCapability == CelebrationRigCapability.RootOnly;
                hasHands |= all[i].RequiredRigCapability == CelebrationRigCapability.TwoHands;
                hasHumanoid |= all[i].RequiredRigCapability == CelebrationRigCapability.Humanoid;
            }

            Assert.IsTrue(hasRoot);
            Assert.IsTrue(hasHands);
            Assert.IsTrue(hasHumanoid);
        }

        [Test]
        public void EveryProceduralCelebrationStartsAndEndsNeutralButMovesInside()
        {
            var all = VictoryCelebrationLibrary.All();
            for (var i = 0; i < all.Count; i++)
            {
                var definition = all[i];
                AssertPoseNeutral(VictoryCelebrationSampler.Sample(definition, 0f), definition.Id + " start");
                AssertPoseNeutral(VictoryCelebrationSampler.Sample(definition, definition.DurationSeconds), definition.Id + " end");

                var sample = VictoryCelebrationSampler.Sample(definition, definition.DurationSeconds * 0.37f);
                Assert.Greater(Activity(sample), 0.05f, definition.Id + " has no readable motion inside its window.");
            }
        }

        [Test]
        public void ResultCelebrationsStayCompactForFastRematch()
        {
            var all = VictoryCelebrationLibrary.All();
            for (var i = 0; i < all.Count; i++)
            {
                Assert.LessOrEqual(all[i].DurationSeconds, 2.5f, all[i].Id + " blocks the result screen too long.");
                Assert.GreaterOrEqual(all[i].DurationSeconds, 1.0f, all[i].Id + " is too short to read cleanly.");
            }
        }

        [Test]
        public void RootOnlyAvatarFallsBackFromAura67WithoutChangingLoadout()
        {
            var catalog = BuildCatalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(VictoryCelebrationLibrary.Aura67Id);
            var loadout = new CosmeticLoadout(new CosmeticId("char-any"), new CosmeticId("pogo-any"));
            var equip = new CosmeticEquipService(catalog, inventory);
            equip.EquipSimple(loadout, VictoryCelebrationLibrary.Aura67Id);

            var selection = VictoryCelebrationCompatibility.ResolveWithStarterFallback(
                catalog,
                loadout,
                CelebrationRigCapability.RootOnly);

            Assert.AreEqual(VictoryCelebrationLibrary.StarterBounceId, selection.Id);
            Assert.AreEqual(VictoryCelebrationLibrary.Aura67Id, loadout.VictoryEmoteId, "Fallback must not silently rewrite ownership/loadout.");
        }

        [Test]
        public void TwoHandAvatarKeepsAura67AndHumanoidKeepsCrownPulse()
        {
            var catalog = BuildCatalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(VictoryCelebrationLibrary.Aura67Id);
            inventory.Unlock(VictoryCelebrationLibrary.CrownPulseId);
            var loadout = new CosmeticLoadout(new CosmeticId("char-any"), new CosmeticId("pogo-any"));
            var equip = new CosmeticEquipService(catalog, inventory);

            equip.EquipSimple(loadout, VictoryCelebrationLibrary.Aura67Id);
            var aura = VictoryCelebrationCompatibility.ResolveWithStarterFallback(
                catalog,
                loadout,
                CelebrationRigCapability.TwoHands);
            Assert.AreEqual(VictoryCelebrationLibrary.Aura67Id, aura.Id);

            equip.EquipSimple(loadout, VictoryCelebrationLibrary.CrownPulseId);
            var crown = VictoryCelebrationCompatibility.ResolveWithStarterFallback(
                catalog,
                loadout,
                CelebrationRigCapability.Humanoid);
            Assert.AreEqual(VictoryCelebrationLibrary.CrownPulseId, crown.Id);
        }

        [Test]
        public void PogoBowGivesNonHumanoidCharactersARealUnlockBeyondStarter()
        {
            var definition = VictoryCelebrationLibrary.PogoBow();
            Assert.AreEqual(CelebrationRigCapability.RootOnly, definition.RequiredRigCapability);
            Assert.AreNotEqual(CosmeticRarity.Common, definition.Rarity);
            Assert.IsTrue(VictoryCelebrationCompatibility.CanRender(
                definition.RequiredRigCapability,
                CelebrationRigCapability.RootOnly));
        }

        private static CosmeticCatalog BuildCatalog()
        {
            var catalog = new CosmeticCatalog();
            VictoryCelebrationLibrary.AddDefaults(catalog);
            return catalog;
        }

        private static float Activity(VictoryCelebrationPose pose)
        {
            return Math.Abs(pose.BodyBob)
                + Math.Abs(pose.BodyLean)
                + Math.Abs(pose.BodyYaw)
                + Math.Abs(pose.HeadYaw)
                + pose.LeftArmLift
                + pose.RightArmLift
                + pose.LeftForearmOpen
                + pose.RightForearmOpen
                + pose.LeftPalmUp
                + pose.RightPalmUp
                + pose.Accent;
        }

        private static void AssertPoseNeutral(VictoryCelebrationPose pose, string label)
        {
            Assert.AreEqual(0f, Activity(pose), 0.0001f, label);
        }
    }
}
