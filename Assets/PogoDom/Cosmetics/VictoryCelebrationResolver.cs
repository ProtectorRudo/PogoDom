using System;

namespace PogoDom.Cosmetics
{
    public sealed class VictoryCelebrationSelection
    {
        public CosmeticId Id { get; }
        public string AssetKey { get; }
        public CosmeticRarity Rarity { get; }
        public bool IsProcedural { get; }
        public VictoryCelebrationDefinition ProceduralDefinition { get; }

        public VictoryCelebrationSelection(ICosmeticDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (definition.Kind != CosmeticKind.VictoryEmote)
                throw new ArgumentException("Selected cosmetic is not a victory emote.", nameof(definition));

            Id = definition.Id;
            AssetKey = definition.AssetKey;
            Rarity = definition.Rarity;
            ProceduralDefinition = definition as VictoryCelebrationDefinition;
            IsProcedural = ProceduralDefinition != null;
        }
    }

    /// <summary>
    /// Converts an equipped cosmetic loadout into the exact celebration asset
    /// the winner should render. The resolver is presentation-only: it never
    /// inspects score, RNG or battle rules and therefore cannot change outcomes.
    /// </summary>
    public static class VictoryCelebrationResolver
    {
        public static VictoryCelebrationSelection Resolve(
            CosmeticCatalog catalog,
            CosmeticLoadout loadout,
            CosmeticId fallbackId)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));

            var requested = HasValue(loadout.VictoryEmoteId) ? loadout.VictoryEmoteId : fallbackId;
            if (!HasValue(requested))
                throw new InvalidOperationException("No equipped victory emote and no fallback was provided.");

            var definition = catalog.Get(requested);
            if (definition.Kind != CosmeticKind.VictoryEmote)
                throw new InvalidOperationException("Resolved cosmetic is not a victory emote: " + requested);

            return new VictoryCelebrationSelection(definition);
        }

        public static VictoryCelebrationSelection ResolveWithStarterFallback(
            CosmeticCatalog catalog,
            CosmeticLoadout loadout)
        {
            return Resolve(catalog, loadout, VictoryCelebrationLibrary.StarterBounceId);
        }

        private static bool HasValue(CosmeticId id)
        {
            return !string.IsNullOrWhiteSpace(id.Value);
        }
    }
}
