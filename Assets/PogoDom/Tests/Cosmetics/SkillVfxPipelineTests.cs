using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class SkillVfxPipelineTests
    {
        [Test]
        public void EquippedSkillVisualResolvesFromRealBattleEvent()
        {
            var catalog = new CosmeticCatalog();
            var loadout = new CosmeticLoadout(new CosmeticId("character"), new CosmeticId("pogo"));
            var visual = new SkillVisualDefinition(
                new CosmeticId("bank-epic"),
                "Epic Bank",
                CosmeticRarity.Epic,
                "skills/bank/epic",
                SkillVisualTrigger.Bank);
            catalog.Add(visual);
            loadout.SkillVisualIds.Add(visual.Id);

            var e = new MatchEvent(MatchEventType.Banked, playerId: 0, value: 12);
            var resolved = SkillVisualResolver.Resolve(catalog, loadout, e);
            VisualEventCue cue;

            Assert.AreSame(visual, resolved);
            Assert.IsTrue(VisualEventPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(SkillVisualTrigger.Bank, cue.Trigger);
            Assert.AreEqual(VisualImpactTier.Spectacle, cue.Impact);
        }

        [Test]
        public void ExactCratePowerVisualWinsRegardlessOfEquipOrder()
        {
            var catalog = new CosmeticCatalog();
            var loadout = new CosmeticLoadout(new CosmeticId("character"), new CosmeticId("pogo"));
            var generic = new SkillVisualDefinition(
                new CosmeticId("crate-generic"),
                "Generic Crate",
                CosmeticRarity.Rare,
                "skills/crate/generic",
                SkillVisualTrigger.CrateOpen);
            var missile = new SkillVisualDefinition(
                new CosmeticId("crate-missile"),
                "Missile Crate",
                CosmeticRarity.Legendary,
                "skills/crate/missile",
                SkillVisualTrigger.CrateOpen,
                PowerUpKind.Missile);
            catalog.Add(generic);
            catalog.Add(missile);

            loadout.SkillVisualIds.Add(generic.Id);
            loadout.SkillVisualIds.Add(missile.Id);
            var e = new MatchEvent(MatchEventType.CrateOpened, playerId: 1, itemKind: PowerUpKind.Missile);
            Assert.AreSame(missile, SkillVisualResolver.Resolve(catalog, loadout, e));

            loadout.SkillVisualIds.Clear();
            loadout.SkillVisualIds.Add(missile.Id);
            loadout.SkillVisualIds.Add(generic.Id);
            Assert.AreSame(missile, SkillVisualResolver.Resolve(catalog, loadout, e));
        }

        [Test]
        public void CosmeticRarityCannotIncreaseGameplayDerivedVfxBudget()
        {
            var e = new MatchEvent(MatchEventType.MissileFired, playerId: 0, secondaryPlayerId: 2);
            VisualEventCue before;
            Assert.IsTrue(VisualEventPolicy.TryDescribe(e, out before));

            var common = new SkillVisualDefinition(
                new CosmeticId("missile-common"),
                "Common Missile",
                CosmeticRarity.Common,
                "skills/missile/common",
                SkillVisualTrigger.Missile);
            var legendary = new SkillVisualDefinition(
                new CosmeticId("missile-legendary"),
                "Legendary Missile",
                CosmeticRarity.Legendary,
                "skills/missile/legendary",
                SkillVisualTrigger.Missile);

            // Definitions may alter presentation style, but the event policy owns
            // the hard mobile budget. Rarity must not mutate that source of truth.
            VisualEventCue after;
            Assert.IsTrue(VisualEventPolicy.TryDescribe(e, out after));
            Assert.AreEqual(before.ParticleBudget, after.ParticleBudget);
            Assert.AreEqual(before.CameraImpulse, after.CameraImpulse);
            Assert.AreEqual(before.RadiusTiles, after.RadiusTiles);
            Assert.AreNotEqual(common.Rarity, legendary.Rarity);
        }

        [Test]
        public void OrdinaryTileStealStaysAmbientEvenWithLegendaryCosmetic()
        {
            var catalog = new CosmeticCatalog();
            var loadout = new CosmeticLoadout(new CosmeticId("character"), new CosmeticId("pogo"));
            var legendary = new SkillVisualDefinition(
                new CosmeticId("steal-legendary"),
                "Legendary Steal",
                CosmeticRarity.Legendary,
                "skills/steal/legendary",
                SkillVisualTrigger.TileSteal);
            catalog.Add(legendary);
            loadout.SkillVisualIds.Add(legendary.Id);

            var e = new MatchEvent(MatchEventType.TileStolen, playerId: 0, secondaryPlayerId: 1);
            var resolved = SkillVisualResolver.Resolve(catalog, loadout, e);
            VisualEventCue cue;

            Assert.AreSame(legendary, resolved);
            Assert.IsTrue(VisualEventPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(VisualImpactTier.Ambient, cue.Impact);
            Assert.LessOrEqual(cue.CameraImpulse, 0.05f);
        }
    }
}
