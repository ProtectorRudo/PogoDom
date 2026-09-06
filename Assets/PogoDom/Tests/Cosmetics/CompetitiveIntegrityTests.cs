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
    }
}
