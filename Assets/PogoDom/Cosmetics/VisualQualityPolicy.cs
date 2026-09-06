using System;

namespace PogoDom.Cosmetics
{
    public enum VisualQualityTier
    {
        Lite = 0,
        Balanced = 1,
        Showcase = 2
    }

    public readonly struct VisualQualityBudget
    {
        public VisualQualityTier Tier { get; }
        public float ParticleMultiplier { get; }
        public int MaxConcurrentSpectacleBursts { get; }
        public float TrailSeconds { get; }
        public float OutlineScale { get; }
        public float EnvironmentDensity { get; }
        public int MaxSkillOrbiters { get; }
        public bool DecorativeCityLights { get; }

        public VisualQualityBudget(
            VisualQualityTier tier,
            float particleMultiplier,
            int maxConcurrentSpectacleBursts,
            float trailSeconds,
            float outlineScale,
            float environmentDensity,
            int maxSkillOrbiters,
            bool decorativeCityLights)
        {
            Tier = tier;
            ParticleMultiplier = particleMultiplier;
            MaxConcurrentSpectacleBursts = maxConcurrentSpectacleBursts;
            TrailSeconds = trailSeconds;
            OutlineScale = outlineScale;
            EnvironmentDensity = environmentDensity;
            MaxSkillOrbiters = maxSkillOrbiters;
            DecorativeCityLights = decorativeCityLights;
        }

        public int ScaleParticles(int requested)
        {
            if (requested <= 0) return 0;
            return Math.Max(1, (int)Math.Round(requested * ParticleMultiplier, MidpointRounding.AwayFromZero));
        }
    }

    /// <summary>
    /// Presentation-only device budget. Tiers are allowed to remove decoration,
    /// never gameplay information such as tiles, players, pickups or hazards.
    /// </summary>
    public static class VisualQualityPolicy
    {
        public static VisualQualityBudget Get(VisualQualityTier tier)
        {
            switch (tier)
            {
                case VisualQualityTier.Lite:
                    return new VisualQualityBudget(tier, 0.45f, 4, 0.13f, 0.62f, 0.50f, 2, false);
                case VisualQualityTier.Balanced:
                    return new VisualQualityBudget(tier, 0.72f, 6, 0.18f, 0.82f, 0.75f, 3, true);
                case VisualQualityTier.Showcase:
                    return new VisualQualityBudget(tier, 1.00f, 8, 0.22f, 1.00f, 1.00f, 4, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tier));
            }
        }

        public static VisualQualityTier SelectAuto(int graphicsMemoryMb, int systemMemoryMb, int shaderLevel)
        {
            // Unknown/zero hardware reports are treated conservatively instead
            // of gambling frame pacing for visual decoration.
            if (graphicsMemoryMb >= 4096 && systemMemoryMb >= 6000 && shaderLevel >= 45)
                return VisualQualityTier.Showcase;
            if (graphicsMemoryMb >= 1536 && systemMemoryMb >= 3500 && shaderLevel >= 35)
                return VisualQualityTier.Balanced;
            return VisualQualityTier.Lite;
        }

        public static bool IsEssentialVisualName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return false;
            return objectName.StartsWith("Tile_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Telegraph_", StringComparison.Ordinal) ||
                   objectName.StartsWith("BankCrate_", StringComparison.Ordinal) ||
                   objectName.StartsWith("MysteryCrate_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Arrow_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Speed_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Missile_", StringComparison.Ordinal) ||
                   objectName.StartsWith("Padlock_", StringComparison.Ordinal) ||
                   objectName == "YOU" ||
                   objectName.StartsWith("BOT ", StringComparison.Ordinal);
        }
    }
}
