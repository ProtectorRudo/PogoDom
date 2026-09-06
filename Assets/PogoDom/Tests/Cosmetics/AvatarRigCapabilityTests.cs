using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarRigCapabilityTests
    {
        [Test]
        public void HumanoidCanEquipHandDependentVictoryEmote()
        {
            var catalog = BuildCatalog();
            var emote = new CosmeticId("emote-hands");
            catalog.Add(new SimpleCosmeticDefinition(
                emote,
                "Hands Up",
                CosmeticKind.VictoryEmote,
                CosmeticRarity.Epic,
                "emote/hands_up",
                AvatarRigCapability.Hands | AvatarRigCapability.FullBodyEmote));

            var inventory = new CosmeticInventory();
            inventory.Unlock(emote);
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-human"), new CosmeticId("pogo-human"));

            service.EquipSimple(loadout, emote);

            Assert.AreEqual(emote, loadout.VictoryEmoteId);
        }

        [Test]
        public void MascotCannotEquipSkillThatRequiresHands()
        {
            var catalog = BuildCatalog();
            var visual = new CosmeticId("skill-hand-lightning");
            catalog.Add(new SkillVisualDefinition(
                visual,
                "Hand Lightning",
                CosmeticRarity.Legendary,
                "fx/hand_lightning",
                SkillVisualTrigger.Missile,
                PowerUpKind.Missile,
                AvatarRigCapability.Hands));

            var inventory = new CosmeticInventory();
            inventory.Unlock(visual);
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-mascot"), new CosmeticId("pogo-mascot"));

            Assert.Throws<System.InvalidOperationException>(() => service.EquipSkillVisual(loadout, visual));
        }

        [Test]
        public void SwitchingToMascotRemovesOnlyIncompatiblePresentation()
        {
            var catalog = BuildCatalog();
            var handEmote = new CosmeticId("emote-hands");
            var aura = new CosmeticId("aura-star");
            var skill = new CosmeticId("skill-hand-lightning");

            catalog.Add(new SimpleCosmeticDefinition(handEmote, "Hands Up", CosmeticKind.VictoryEmote, CosmeticRarity.Epic, "emote/hands", AvatarRigCapability.Hands));
            catalog.Add(new SimpleCosmeticDefinition(aura, "Star Aura", CosmeticKind.Aura, CosmeticRarity.Rare, "aura/star", AvatarRigCapability.AuraSocket));
            catalog.Add(new SkillVisualDefinition(skill, "Hand Lightning", CosmeticRarity.Legendary, "fx/hand", SkillVisualTrigger.Missile, PowerUpKind.Missile, AvatarRigCapability.Hands));

            var inventory = new CosmeticInventory();
            inventory.Unlock(handEmote);
            inventory.Unlock(aura);
            inventory.Unlock(skill);
            inventory.Unlock(new CosmeticId("char-mascot"));
            inventory.Unlock(new CosmeticId("pogo-mascot"));

            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-human"), new CosmeticId("pogo-human"));
            service.EquipSimple(loadout, handEmote);
            service.EquipSimple(loadout, aura);
            service.EquipSkillVisual(loadout, skill);

            service.EquipCharacter(loadout, new CosmeticId("char-mascot"));

            Assert.AreEqual(new CosmeticId("char-mascot"), loadout.CharacterId);
            Assert.AreEqual(new CosmeticId("pogo-mascot"), loadout.PogoId);
            Assert.IsTrue(string.IsNullOrEmpty(loadout.VictoryEmoteId.Value));
            Assert.AreEqual(aura, loadout.AuraId);
            Assert.AreEqual(0, loadout.SkillVisualIds.Count);
        }

        private static CosmeticCatalog BuildCatalog()
        {
            var catalog = new CosmeticCatalog();
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-human"), "Human Pogo", CosmeticRarity.Common, "pogo/human", "human"));
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-mascot"), "Mascot Pogo", CosmeticRarity.Common, "pogo/mascot", "mascot"));
            catalog.Add(new CharacterDefinition(
                new CosmeticId("char-human"),
                "Human",
                CosmeticRarity.Common,
                "char/human",
                "human",
                new CosmeticId("pogo-human"),
                capabilities: AvatarRigCapabilities.StandardHumanoid));
            catalog.Add(new CharacterDefinition(
                new CosmeticId("char-mascot"),
                "Mascot",
                CosmeticRarity.Common,
                "char/mascot",
                "mascot",
                new CosmeticId("pogo-mascot"),
                capabilities: AvatarRigCapabilities.Mascot));
            return catalog;
        }
    }
}
