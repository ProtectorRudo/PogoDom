using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class CosmeticEquipTests
    {
        [Test]
        public void UnownedCosmeticCannotBeEquipped()
        {
            var catalog = Catalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(new CosmeticId("char-a"));
            inventory.Unlock(new CosmeticId("pogo-a"));
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));
            var service = new CosmeticEquipService(catalog, inventory);

            Assert.Throws<System.InvalidOperationException>(() => service.EquipSimple(loadout, new CosmeticId("trail-a")));
        }

        [Test]
        public void IncompatiblePogoCannotBeEquipped()
        {
            var catalog = Catalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(new CosmeticId("char-a"));
            inventory.Unlock(new CosmeticId("pogo-a"));
            inventory.Unlock(new CosmeticId("pogo-b"));
            var loadout = new CosmeticLoadout(new CosmeticId("char-a"), new CosmeticId("pogo-a"));
            var service = new CosmeticEquipService(catalog, inventory);

            Assert.Throws<System.InvalidOperationException>(() => service.EquipPogo(loadout, new CosmeticId("pogo-b")));
        }

        private static CosmeticCatalog Catalog()
        {
            var catalog = new CosmeticCatalog();
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-a"), "A", CosmeticRarity.Common, "pogo/a", "small"));
            catalog.Add(new PogoDefinition(new CosmeticId("pogo-b"), "B", CosmeticRarity.Common, "pogo/b", "large"));
            catalog.Add(new CharacterDefinition(new CosmeticId("char-a"), "A", CosmeticRarity.Common, "char/a", "small", new CosmeticId("pogo-a")));
            catalog.Add(new SimpleCosmeticDefinition(new CosmeticId("trail-a"), "Trail", CosmeticKind.Trail, CosmeticRarity.Rare, "trail/a"));
            catalog.Validate();
            return catalog;
        }
    }
}
