using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarAttachmentEquipTests
    {
        [Test]
        public void AvatarCanEquipHeadwearBackAccessoryAndAuraIndependently()
        {
            var catalog = new CosmeticCatalog();
            var head = new CosmeticId("head-neon-cap");
            var back = new CosmeticId("back-mini-flag");
            var aura = new CosmeticId("aura-city-pulse");
            catalog.Add(new SimpleCosmeticDefinition(head, "Neon Cap", CosmeticKind.Headwear, CosmeticRarity.Rare, "head/neon_cap"));
            catalog.Add(new SimpleCosmeticDefinition(back, "Mini Flag", CosmeticKind.BackAccessory, CosmeticRarity.Epic, "back/mini_flag"));
            catalog.Add(new SimpleCosmeticDefinition(aura, "City Pulse", CosmeticKind.Aura, CosmeticRarity.Legendary, "aura/city_pulse"));

            var inventory = new CosmeticInventory();
            inventory.Unlock(head);
            inventory.Unlock(back);
            inventory.Unlock(aura);
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("character-placeholder"), new CosmeticId("pogo-placeholder"));

            service.EquipSimple(loadout, head);
            service.EquipSimple(loadout, back);
            service.EquipSimple(loadout, aura);

            Assert.AreEqual(head, loadout.HeadwearId);
            Assert.AreEqual(back, loadout.BackAccessoryId);
            Assert.AreEqual(aura, loadout.AuraId);
        }

        [Test]
        public void EquippingSecondAttachmentReplacesOnlyItsOwnSlot()
        {
            var catalog = new CosmeticCatalog();
            var capA = new CosmeticId("head-a");
            var capB = new CosmeticId("head-b");
            var aura = new CosmeticId("aura-a");
            catalog.Add(new SimpleCosmeticDefinition(capA, "Cap A", CosmeticKind.Headwear, CosmeticRarity.Common, "head/a"));
            catalog.Add(new SimpleCosmeticDefinition(capB, "Cap B", CosmeticKind.Headwear, CosmeticRarity.Epic, "head/b"));
            catalog.Add(new SimpleCosmeticDefinition(aura, "Aura A", CosmeticKind.Aura, CosmeticRarity.Rare, "aura/a"));

            var inventory = new CosmeticInventory();
            inventory.Unlock(capA);
            inventory.Unlock(capB);
            inventory.Unlock(aura);
            var service = new CosmeticEquipService(catalog, inventory);
            var loadout = new CosmeticLoadout(new CosmeticId("character-placeholder"), new CosmeticId("pogo-placeholder"));

            service.EquipSimple(loadout, capA);
            service.EquipSimple(loadout, aura);
            service.EquipSimple(loadout, capB);

            Assert.AreEqual(capB, loadout.HeadwearId);
            Assert.AreEqual(aura, loadout.AuraId);
        }
    }
}
