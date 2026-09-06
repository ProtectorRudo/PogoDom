using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VictoryCelebrationResolverTests
    {
        [Test]
        public void EmptyLoadoutFallsBackToStarterCelebration()
        {
            var catalog = CreateCatalog();
            var loadout = NewLoadout();

            var selection = VictoryCelebrationResolver.ResolveWithStarterFallback(catalog, loadout);

            Assert.AreEqual(VictoryCelebrationLibrary.StarterBounceId, selection.Id);
            Assert.IsTrue(selection.IsProcedural);
            Assert.AreEqual(VictoryCelebrationMotion.VictoryBounce, selection.ProceduralDefinition.Motion);
        }

        [Test]
        public void EquippedAura67WinsOverFallback()
        {
            var catalog = CreateCatalog();
            var inventory = new CosmeticInventory();
            inventory.Unlock(VictoryCelebrationLibrary.Aura67Id);
            var loadout = NewLoadout();
            new CosmeticEquipService(catalog, inventory).EquipSimple(loadout, VictoryCelebrationLibrary.Aura67Id);

            var selection = VictoryCelebrationResolver.ResolveWithStarterFallback(catalog, loadout);

            Assert.AreEqual(VictoryCelebrationLibrary.Aura67Id, selection.Id);
            Assert.IsTrue(selection.IsProcedural);
            Assert.AreEqual(VictoryCelebrationMotion.Aura67, selection.ProceduralDefinition.Motion);
            Assert.AreEqual(CosmeticRarity.Legendary, selection.Rarity);
        }

        [Test]
        public void AuthoredClipVictoryEmoteUsesSameResolutionPath()
        {
            var catalog = CreateCatalog();
            var authoredId = new CosmeticId("victory-authored-wave");
            catalog.Add(new SimpleCosmeticDefinition(
                authoredId,
                "Authored Wave",
                CosmeticKind.VictoryEmote,
                CosmeticRarity.Epic,
                "animation://victory/wave-v1"));
            var inventory = new CosmeticInventory();
            inventory.Unlock(authoredId);
            var loadout = NewLoadout();
            new CosmeticEquipService(catalog, inventory).EquipSimple(loadout, authoredId);

            var selection = VictoryCelebrationResolver.ResolveWithStarterFallback(catalog, loadout);

            Assert.AreEqual(authoredId, selection.Id);
            Assert.IsFalse(selection.IsProcedural);
            Assert.IsNull(selection.ProceduralDefinition);
            Assert.AreEqual("animation://victory/wave-v1", selection.AssetKey);
        }

        [Test]
        public void NonVictoryFallbackIsRejected()
        {
            var catalog = CreateCatalog();
            var trailId = new CosmeticId("trail-test");
            catalog.Add(new SimpleCosmeticDefinition(
                trailId,
                "Trail",
                CosmeticKind.Trail,
                CosmeticRarity.Common,
                "trail://test"));

            Assert.Throws<System.InvalidOperationException>(() =>
                VictoryCelebrationResolver.Resolve(catalog, NewLoadout(), trailId));
        }

        private static CosmeticCatalog CreateCatalog()
        {
            var catalog = new CosmeticCatalog();
            VictoryCelebrationLibrary.AddDefaults(catalog);
            return catalog;
        }

        private static CosmeticLoadout NewLoadout()
        {
            return new CosmeticLoadout(new CosmeticId("character-test"), new CosmeticId("pogo-test"));
        }
    }
}
