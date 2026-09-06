using System;

namespace PogoDom.Cosmetics
{
    public readonly struct VisualRuntimeBudget
    {
        public int MaxUniqueToonMaterials { get; }
        public int MaxMeshRenderers { get; }
        public int MaxLineRenderers { get; }
        public int MaxTrailRenderers { get; }
        public int MaxParticleSystems { get; }

        public VisualRuntimeBudget(
            int maxUniqueToonMaterials,
            int maxMeshRenderers,
            int maxLineRenderers,
            int maxTrailRenderers,
            int maxParticleSystems)
        {
            if (maxUniqueToonMaterials < 1) throw new ArgumentOutOfRangeException(nameof(maxUniqueToonMaterials));
            if (maxMeshRenderers < 1) throw new ArgumentOutOfRangeException(nameof(maxMeshRenderers));
            if (maxLineRenderers < 0) throw new ArgumentOutOfRangeException(nameof(maxLineRenderers));
            if (maxTrailRenderers < 0) throw new ArgumentOutOfRangeException(nameof(maxTrailRenderers));
            if (maxParticleSystems < 0) throw new ArgumentOutOfRangeException(nameof(maxParticleSystems));
            MaxUniqueToonMaterials = maxUniqueToonMaterials;
            MaxMeshRenderers = maxMeshRenderers;
            MaxLineRenderers = maxLineRenderers;
            MaxTrailRenderers = maxTrailRenderers;
            MaxParticleSystems = maxParticleSystems;
        }
    }

    /// <summary>
    /// Performance guardrails for the four-player launch arena. These are not
    /// renderer feature switches; they are budgets that make visual bloat visible
    /// in development before it reaches a phone recording a vertical clip.
    /// </summary>
    public static class VisualRuntimeBudgetPolicy
    {
        public static VisualRuntimeBudget Get(VisualQualityTier tier)
        {
            switch (tier)
            {
                case VisualQualityTier.Lite:
                    return new VisualRuntimeBudget(18, 230, 12, 4, 6);
                case VisualQualityTier.Showcase:
                    return new VisualRuntimeBudget(34, 390, 24, 4, 10);
                default:
                    return new VisualRuntimeBudget(26, 310, 18, 4, 8);
            }
        }

        public static bool IsDynamicColorObject(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return false;
            return objectName.StartsWith("Tile_", StringComparison.Ordinal) ||
                   objectName.StartsWith("TileGlow", StringComparison.Ordinal) ||
                   objectName.StartsWith("Telegraph_", StringComparison.Ordinal) ||
                   objectName.StartsWith("BurstCore", StringComparison.Ordinal) ||
                   objectName.StartsWith("SkillBurst_", StringComparison.Ordinal) ||
                   objectName.StartsWith("CombatTrace_", StringComparison.Ordinal) ||
                   objectName.StartsWith("CombatReadabilityRing", StringComparison.Ordinal);
        }

        public static bool IsShareableToonCandidate(string objectName, bool isAvatar)
        {
            if (IsDynamicColorObject(objectName)) return false;
            if (isAvatar) return true;
            if (string.IsNullOrEmpty(objectName)) return false;

            return objectName.StartsWith("MysteryCrate_", StringComparison.Ordinal) ||
                   objectName.StartsWith("BankCrate_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Missile_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Speed_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Padlock_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Arrow_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Vault", StringComparison.Ordinal) ||
                   objectName.StartsWith("Relic", StringComparison.Ordinal) ||
                   objectName.StartsWith("Rocket", StringComparison.Ordinal) ||
                   objectName.StartsWith("Lock", StringComparison.Ordinal) ||
                   objectName.StartsWith("Speed", StringComparison.Ordinal) ||
                   objectName.StartsWith("Arrow", StringComparison.Ordinal) ||
                   objectName == "PickupHalo";
        }

        public static uint StablePresentationHash(string value)
        {
            unchecked
            {
                // FNV-1a: process-independent, small and sufficient for visual
                // phase offsets. Never use string.GetHashCode for replay visuals.
                var hash = 2166136261u;
                if (value == null) return hash;
                for (var i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619u;
                }
                return hash;
            }
        }

        public static float StablePhase01(string value)
        {
            return (StablePresentationHash(value) & 0x00FFFFFFu) / 16777215f;
        }
    }
}
