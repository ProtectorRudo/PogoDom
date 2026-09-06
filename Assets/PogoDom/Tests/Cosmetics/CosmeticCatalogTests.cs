using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class CosmeticCatalogTests
    {
        [Test]
        public void CatalogRejectsDuplicateIds()
        {
            var catalog = new CosmeticCatalog();
            var id = new CosmeticId("trail-a");
            catalog.Add(new SimpleCosmeticDefinition(id, "A", CosmeticKind.Trail, CosmeticRarity.Common, "trail/a"));
            Assert.Throws<System.InvalidOperationException>(() => catalog.Add(new SimpleCosmeticDefinition(id, "B", CosmeticKind.Trail, CosmeticRarity.Rare, "trail/b")));
        }

        [Test]
        public void CharacterDefaultPogoMustBeCompatible()
        {
            var catalog = new CosmeticCatalog();
            var pogo = new CosmeticId("pogo-heavy");
            catalog.Add(new PogoDefinition(pogo, "Heavy", CosmeticRarity.Common, "pogo/heavy", "heavy"));
            catalog.Add(new CharacterDefinition(new CosmeticId("char-small"), "Small", CosmeticRarity.Common, "char/small", "small", pogo));
            Assert.Throws<System.InvalidOperationException>(() => catalog.Validate());
        }
    }
}
