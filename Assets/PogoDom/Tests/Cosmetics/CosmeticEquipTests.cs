using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class CosmeticEquipTests
    {
        [Test]
        public void UnownedCosmeticCannotBeEquipped()
        {
            var catalog = BuildCatalog();
            var inventory = new CosmeticInventory();
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));

            Assert.Throws<System.InvalidOperationException>(() => service.EquipPogo(loadout, new CosmeticId("pogo-b")));
        }

        [Test]
        public void IncompatiblePogoCannotBeEquipped()
        {
            var catalog = BuildCatalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(new CosmeticId("pogo-b"));
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));

            Assert.Throws<System.InvalidOperationException>(() => service.EquipPogo(loadout, new CosmeticId("pogo-b")));
        }

        [Test]
        public void CharacterDefaultPogoMustBeCompatible()
        {
            var catalog = BuildCatalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(new CosmeticId("char-b"));
            inventory.Unlock(new CosmeticId("pogo-b"));
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));

            service.EquipCharacter(loadout, new CosmeticId("char-b"));

            Assert.AreEqual(new CosmeticId("char-b"), loadout.CharacterId);
            Assert.AreEqual(new CosmeticId("pogo-b"), loadout.PogoId);
        }

        [Test]
        public void EquippingVisualInSameTriggerContextReplacesPreviousVisual()
        {
            var catalog = BuildCatalog();
            var first = new CosmeticId("missile-blue");
            var second = new CosmeticId("missile-dragon");
            catalog.Add(new SkillVisualDefinition(first, "Blue Missile", CosmeticRarity.Rare, "fx/missile_blue", SkillVisualTrigger.Missile, PowerUpKind.Missile));
            catalog.Add(new SkillVisualDefinition(second, "Dragon Missile", CosmeticRarity.Legendary, "fx/missile_dragon", SkillVisualTrigger.Missile, PowerUpKind.Missile));
            var inventory = new CosmeticInventory();
            inventory.Unlock(first);
            inventory.Unlock(second);
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));

            service.EquipSkillVisual(loadout, first);
            service.EquipSkillVisual(loadout, second);

            Assert.AreEqual(1, loadout.SkillVisualIds.Count);
            Assert.AreEqual(second, loadout.SkillVisualIds[0]);
        }

        private static CosmeticCatalog BuildCatalog()
        {
            var catalog = new CosmeticCatalog();
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-a"), "Pogo A", CosmeticRarity.Common, "pogo/a", "rig-a"));
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-b"), "Pogo B", CosmeticRarity.Common, "pogo/b", "rig-b"));
            catalog.Add(new CharacterDefinition(new CosmeticId("char-a"), "A", CosmeticRarity.Common, "char/a", "rig-a", new CosmeticId("pogo-a")));
            catalog.Add(new CharacterDefinition(new CosmeticId("char-b"), "B", CosmeticRarity.Common, "char/b", "rig-b", new CosmeticId("pogo-b")));
            return catalog;
        }
    }
}
