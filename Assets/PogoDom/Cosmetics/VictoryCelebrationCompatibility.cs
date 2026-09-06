using System;

namespace PogoDom.Cosmetics
{
    /// <summary>
    /// Keeps victory cosmetics safe across the mixed avatar roster. A mascot
    /// without usable hands must never explode because a two-hand emote was
    /// equipped; it deterministically falls back to a compatible celebration.
    /// </summary>
    public static class VictoryCelebrationCompatibility
    {
        public static bool CanRender(
            CelebrationRigCapability required,
            CelebrationRigCapability available)
        {
            return (int)available >= (int)required;
        }

        public static VictoryCelebrationSelection ResolveCompatible(
            CosmeticCatalog catalog,
            CosmeticLoadout loadout,
            CelebrationRigCapability availableCapability,
            CosmeticId fallbackId)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));

            var requested = VictoryCelebrationResolver.Resolve(catalog, loadout, fallbackId);
            if (IsCompatible(requested, availableCapability))
                return requested;

            var fallbackDefinition = catalog.Get(fallbackId);
            if (fallbackDefinition.Kind != CosmeticKind.VictoryEmote)
                throw new InvalidOperationException("Victory fallback is not a victory emote: " + fallbackId);

            var fallback = new VictoryCelebrationSelection(fallbackDefinition);
            if (!IsCompatible(fallback, availableCapability))
                throw new InvalidOperationException(
                    "Victory fallback requires a rig capability that the avatar does not have: " + fallbackId);

            return fallback;
        }

        public static VictoryCelebrationSelection ResolveWithStarterFallback(
            CosmeticCatalog catalog,
            CosmeticLoadout loadout,
            CelebrationRigCapability availableCapability)
        {
            return ResolveCompatible(
                catalog,
                loadout,
                availableCapability,
                VictoryCelebrationLibrary.StarterBounceId);
        }

        public static bool IsCompatible(
            VictoryCelebrationSelection selection,
            CelebrationRigCapability availableCapability)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            // Authored clip emotes are validated against their concrete avatar
            // binding by the presentation/content layer. Procedural definitions
            // carry an explicit portable capability contract here.
            if (!selection.IsProcedural || selection.ProceduralDefinition == null)
                return true;

            return CanRender(
                selection.ProceduralDefinition.RequiredRigCapability,
                availableCapability);
        }
    }
}
