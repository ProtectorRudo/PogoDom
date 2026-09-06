using System;
using System.Reflection;
using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class CompetitiveIntegrityTests
    {
        [Test]
        public void CosmeticDefinitionsExposeNoGameplayConfigOrPlayerState()
        {
            var types = new[]
            {
                typeof(CharacterDefinition),
                typeof(PogoDefinition),
                typeof(SimpleCosmeticDefinition),
                typeof(SkillVisualDefinition),
                typeof(CosmeticLoadout),
                typeof(CosmeticOffer)
            };

            foreach (var type in types)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (var i = 0; i < properties.Length; i++)
                {
                    Assert.AreNotEqual(typeof(MatchConfig), properties[i].PropertyType, type.Name + " must not expose MatchConfig.");
                    Assert.AreNotEqual(typeof(PlayerState), properties[i].PropertyType, type.Name + " must not expose PlayerState.");
                    var name = properties[i].Name.ToLowerInvariant();
                    Assert.IsFalse(name.Contains("damage") || name.Contains("multiplier") || name.Contains("speedbonus") || name.Contains("scorebonus") || name.Contains("stunbonus"), "Gameplay advantage field detected: " + type.Name + "." + properties[i].Name);
                }
            }
        }

        [Test]
        public void SkillVisualResolvesPresentationOnlyFromGameplayEvent()
        {
            var catalog = new CosmeticCatalog();
            var skillId = new CosmeticId("dragon-missile");
            catalog.Add(new SkillVisualDefinition(skillId, "Dragon Missile", CosmeticRarity.Legendary, "fx/dragon_missile", SkillVisualTrigger.Missile, PowerUpKind.Missile));
            var loadout = new CosmeticLoadout(new CosmeticId("char"), new CosmeticId("pogo"));
            loadout.SkillVisualIds.Add(skillId);
            var e = new MatchEvent(MatchEventType.MissileFired, 0, itemKind: PowerUpKind.Missile);

            var resolved = SkillVisualResolver.Resolve(catalog, loadout, e);

            Assert.IsNotNull(resolved);
            Assert.AreEqual("fx/dragon_missile", resolved.AssetKey);
        }

        [Test]
        public void CratePayloadCanSelectAVisualWithoutChangingPowerEffect()
        {
            var catalog = new CosmeticCatalog();
            var genericId = new CosmeticId("crate-stars");
            var missileId = new CosmeticId("crate-dragon-missile");
            catalog.Add(new SkillVisualDefinition(genericId, "Star Crate", CosmeticRarity.Rare, "fx/crate_stars", SkillVisualTrigger.CrateOpen));
            catalog.Add(new SkillVisualDefinition(missileId, "Dragon Reveal", CosmeticRarity.Legendary, "fx/crate_dragon", SkillVisualTrigger.CrateOpen, PowerUpKind.Missile));
            var loadout = new CosmeticLoadout(new CosmeticId("char"), new CosmeticId("pogo"));
            loadout.SkillVisualIds.Add(genericId);
            loadout.SkillVisualIds.Add(missileId);

            var missileEvent = new MatchEvent(MatchEventType.CrateOpened, 0, itemKind: PowerUpKind.Missile);
            var speedEvent = new MatchEvent(MatchEventType.CrateOpened, 0, itemKind: PowerUpKind.Speed);

            Assert.AreEqual(missileId, SkillVisualResolver.Resolve(catalog, loadout, missileEvent).Id);
            Assert.AreEqual(genericId, SkillVisualResolver.Resolve(catalog, loadout, speedEvent).Id);
        }

        [Test]
        public void NewBattleMomentsExposeCosmeticTriggers()
        {
            var cases = new[]
            {
                new { Event = new MatchEvent(MatchEventType.EnclosureCaptured, 0, value: 5), Trigger = SkillVisualTrigger.AreaCapture },
                new { Event = new MatchEvent(MatchEventType.PadlockActivated, 0, itemKind: PowerUpKind.Padlock), Trigger = SkillVisualTrigger.Padlock },
                new { Event = new MatchEvent(MatchEventType.TileProtected, 0, secondaryPlayerId: 1), Trigger = SkillVisualTrigger.ShieldBlock },
                new { Event = new MatchEvent(MatchEventType.HazardDetonated, position: new GridPos(2, 2), value: 4, itemKind: PowerUpKind.Tnt), Trigger = SkillVisualTrigger.HazardBlast },
                new { Event = new MatchEvent(MatchEventType.PlayerStunned, 1, itemKind: PowerUpKind.Missile), Trigger = SkillVisualTrigger.Stun }
            };

            for (var i = 0; i < cases.Length; i++)
            {
                var catalog = new CosmeticCatalog();
                var id = new CosmeticId("visual-" + i);
                catalog.Add(new SkillVisualDefinition(id, "Visual " + i, CosmeticRarity.Epic, "fx/" + i, cases[i].Trigger));
                var loadout = new CosmeticLoadout(new CosmeticId("char"), new CosmeticId("pogo"));
                loadout.SkillVisualIds.Add(id);
                Assert.IsNotNull(SkillVisualResolver.Resolve(catalog, loadout, cases[i].Event), "Missing trigger mapping: " + cases[i].Trigger);
            }
        }
    }
}
