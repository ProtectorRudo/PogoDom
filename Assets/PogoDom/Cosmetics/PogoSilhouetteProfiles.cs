using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public enum PogoSilhouetteFamily
    {
        HeroSport = 0,
        TricksterCoil = 1,
        TechPulse = 2,
        MascotBubble = 3,
        StreetDeck = 4,
        CaptainCrest = 5
    }

    public readonly struct PogoSilhouetteProfile
    {
        public PogoSilhouetteFamily Family { get; }
        public string Signature { get; }
        public float FootWidthScale { get; }
        public float SpringWidthScale { get; }
        public float HandleWidthScale { get; }
        public int AccentPartBudget { get; }
        public bool UsesEmissionAccent { get; }

        public PogoSilhouetteProfile(
            PogoSilhouetteFamily family,
            string signature,
            float footWidthScale,
            float springWidthScale,
            float handleWidthScale,
            int accentPartBudget,
            bool usesEmissionAccent)
        {
            if (string.IsNullOrWhiteSpace(signature)) throw new ArgumentException("Pogo silhouette signature is required.", nameof(signature));
            if (footWidthScale <= 0f) throw new ArgumentOutOfRangeException(nameof(footWidthScale));
            if (springWidthScale <= 0f) throw new ArgumentOutOfRangeException(nameof(springWidthScale));
            if (handleWidthScale <= 0f) throw new ArgumentOutOfRangeException(nameof(handleWidthScale));
            if (accentPartBudget < 1 || accentPartBudget > 5) throw new ArgumentOutOfRangeException(nameof(accentPartBudget));

            Family = family;
            Signature = signature.Trim().ToLowerInvariant();
            FootWidthScale = footWidthScale;
            SpringWidthScale = springWidthScale;
            HandleWidthScale = handleWidthScale;
            AccentPartBudget = accentPartBudget;
            UsesEmissionAccent = usesEmissionAccent;
        }
    }

    /// <summary>
    /// Cosmetic-only pogo silhouettes paired with the six launch avatar archetypes.
    /// These values describe presentation proportions only; no movement/jump stat
    /// may ever be derived from this policy.
    /// </summary>
    public static class PogoSilhouetteProfiles
    {
        private static readonly PogoSilhouetteProfile[] Profiles =
        {
            new PogoSilhouetteProfile(PogoSilhouetteFamily.HeroSport, "wide-foot|aero-fin|split-grip", 1.18f, 0.92f, 1.08f, 3, true),
            new PogoSilhouetteProfile(PogoSilhouetteFamily.TricksterCoil, "double-coil|tiny-foot|ring-grip", 0.88f, 1.24f, 0.94f, 4, false),
            new PogoSilhouetteProfile(PogoSilhouetteFamily.TechPulse, "pulse-core|fork-stem|neon-rails", 1.02f, 0.82f, 1.16f, 5, true),
            new PogoSilhouetteProfile(PogoSilhouetteFamily.MascotBubble, "round-foot|bubble-core|soft-grips", 1.12f, 1.10f, 1.00f, 4, true),
            new PogoSilhouetteProfile(PogoSilhouetteFamily.StreetDeck, "deck-foot|angled-stem|bar-grip", 1.28f, 0.90f, 1.18f, 4, false),
            new PogoSilhouetteProfile(PogoSilhouetteFamily.CaptainCrest, "crest-foot|twin-post|guard-grip", 1.08f, 1.02f, 1.22f, 5, true)
        };

        public static IReadOnlyList<PogoSilhouetteProfile> All => Profiles;

        public static PogoSilhouetteProfile Get(PogoSilhouetteFamily family)
        {
            var index = (int)family;
            if (index < 0 || index >= Profiles.Length)
                throw new ArgumentOutOfRangeException(nameof(family));
            return Profiles[index];
        }

        public static PogoSilhouetteFamily ForPlayerSlot(int playerId)
        {
            if (playerId < 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            return (PogoSilhouetteFamily)(playerId % Profiles.Length);
        }
    }
}
