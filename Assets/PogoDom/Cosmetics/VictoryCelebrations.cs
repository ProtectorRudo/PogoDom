using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public enum VictoryCelebrationMotion
    {
        VictoryBounce = 0,
        Aura67 = 1,
        PogoBow = 2,
        SkyPop = 3,
        SideSway = 4,
        CrownPulse = 5
    }

    public enum CelebrationRigCapability
    {
        RootOnly = 0,
        TwoHands = 1,
        Humanoid = 2
    }

    /// <summary>
    /// Engine-independent normalized pose channels. They describe intent rather
    /// than bones so the same celebration can be adapted to humanoids, creatures
    /// with hand anchors, or a root-only fallback without touching gameplay.
    /// </summary>
    public readonly struct VictoryCelebrationPose
    {
        public float BodyBob { get; }
        public float BodyLean { get; }
        public float BodyYaw { get; }
        public float HeadYaw { get; }
        public float LeftArmLift { get; }
        public float RightArmLift { get; }
        public float LeftForearmOpen { get; }
        public float RightForearmOpen { get; }
        public float LeftPalmUp { get; }
        public float RightPalmUp { get; }
        public float Accent { get; }

        public VictoryCelebrationPose(
            float bodyBob,
            float bodyLean,
            float bodyYaw,
            float headYaw,
            float leftArmLift,
            float rightArmLift,
            float leftForearmOpen,
            float rightForearmOpen,
            float leftPalmUp,
            float rightPalmUp,
            float accent)
        {
            BodyBob = ClampSigned(bodyBob);
            BodyLean = ClampSigned(bodyLean);
            BodyYaw = ClampSigned(bodyYaw);
            HeadYaw = ClampSigned(headYaw);
            LeftArmLift = Clamp01(leftArmLift);
            RightArmLift = Clamp01(rightArmLift);
            LeftForearmOpen = Clamp01(leftForearmOpen);
            RightForearmOpen = Clamp01(rightForearmOpen);
            LeftPalmUp = Clamp01(leftPalmUp);
            RightPalmUp = Clamp01(rightPalmUp);
            Accent = Clamp01(accent);
        }

        public static VictoryCelebrationPose Neutral => new VictoryCelebrationPose(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
        private static float ClampSigned(float value) => value < -1f ? -1f : value > 1f ? 1f : value;
    }

    public sealed class VictoryCelebrationDefinition : ICosmeticDefinition
    {
        public CosmeticId Id { get; }
        public string DisplayName { get; }
        public CosmeticKind Kind => CosmeticKind.VictoryEmote;
        public CosmeticRarity Rarity { get; }
        public string AssetKey { get; }
        public VictoryCelebrationMotion Motion { get; }
        public CelebrationRigCapability RequiredRigCapability { get; }
        public float DurationSeconds { get; }

        public VictoryCelebrationDefinition(
            CosmeticId id,
            string displayName,
            CosmeticRarity rarity,
            string assetKey,
            VictoryCelebrationMotion motion,
            CelebrationRigCapability requiredRigCapability,
            float durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(assetKey)) throw new ArgumentException("Asset key is required.", nameof(assetKey));
            if (durationSeconds <= 0f || float.IsNaN(durationSeconds) || float.IsInfinity(durationSeconds))
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));

            Id = id;
            DisplayName = displayName.Trim();
            Rarity = rarity;
            AssetKey = assetKey.Trim();
            Motion = motion;
            RequiredRigCapability = requiredRigCapability;
            DurationSeconds = durationSeconds;
        }
    }

    /// <summary>
    /// Small launch-facing celebration collection. The goal is not catalogue
    /// bloat: every entry has a different readable silhouette while keeping the
    /// result screen short enough that REMATCH remains the primary action.
    /// </summary>
    public static class VictoryCelebrationLibrary
    {
        public static readonly CosmeticId StarterBounceId = new CosmeticId("victory-starter-bounce");
        public static readonly CosmeticId PogoBowId = new CosmeticId("victory-pogo-bow");
        public static readonly CosmeticId SkyPopId = new CosmeticId("victory-sky-pop");
        public static readonly CosmeticId SideSwayId = new CosmeticId("victory-side-sway");
        public static readonly CosmeticId Aura67Id = new CosmeticId("victory-aura-67");
        public static readonly CosmeticId CrownPulseId = new CosmeticId("victory-crown-pulse");

        public static VictoryCelebrationDefinition StarterBounce()
        {
            return new VictoryCelebrationDefinition(
                StarterBounceId,
                "Victory Bounce",
                CosmeticRarity.Common,
                "celebration://procedural/victory-bounce-v1",
                VictoryCelebrationMotion.VictoryBounce,
                CelebrationRigCapability.RootOnly,
                1.8f);
        }

        public static VictoryCelebrationDefinition PogoBow()
        {
            return new VictoryCelebrationDefinition(
                PogoBowId,
                "Pogo Bow",
                CosmeticRarity.Rare,
                "celebration://procedural/pogo-bow-v1",
                VictoryCelebrationMotion.PogoBow,
                CelebrationRigCapability.RootOnly,
                1.7f);
        }

        public static VictoryCelebrationDefinition SkyPop()
        {
            return new VictoryCelebrationDefinition(
                SkyPopId,
                "Sky Pop",
                CosmeticRarity.Rare,
                "celebration://procedural/sky-pop-v1",
                VictoryCelebrationMotion.SkyPop,
                CelebrationRigCapability.TwoHands,
                1.9f);
        }

        public static VictoryCelebrationDefinition SideSway()
        {
            return new VictoryCelebrationDefinition(
                SideSwayId,
                "Side Sway",
                CosmeticRarity.Epic,
                "celebration://procedural/side-sway-v1",
                VictoryCelebrationMotion.SideSway,
                CelebrationRigCapability.TwoHands,
                2.1f);
        }

        public static VictoryCelebrationDefinition Aura67()
        {
            return new VictoryCelebrationDefinition(
                Aura67Id,
                "Aura 67",
                CosmeticRarity.Legendary,
                "celebration://procedural/aura-67-v1",
                VictoryCelebrationMotion.Aura67,
                CelebrationRigCapability.TwoHands,
                2.4f);
        }

        public static VictoryCelebrationDefinition CrownPulse()
        {
            return new VictoryCelebrationDefinition(
                CrownPulseId,
                "Crown Pulse",
                CosmeticRarity.Mythic,
                "celebration://procedural/crown-pulse-v1",
                VictoryCelebrationMotion.CrownPulse,
                CelebrationRigCapability.Humanoid,
                2.5f);
        }

        public static IReadOnlyList<VictoryCelebrationDefinition> All()
        {
            return new[]
            {
                StarterBounce(),
                PogoBow(),
                SkyPop(),
                SideSway(),
                Aura67(),
                CrownPulse()
            };
        }

        public static void AddDefaults(CosmeticCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var all = All();
            for (var i = 0; i < all.Count; i++)
                catalog.Add(all[i]);
        }
    }

    /// <summary>
    /// Deterministic procedural choreography. No audio, brand asset, animation
    /// clip or Unity dependency is required. Runtime renderers translate these
    /// channels into the capabilities of each character rig.
    /// </summary>
    public static class VictoryCelebrationSampler
    {
        public static VictoryCelebrationPose Sample(VictoryCelebrationDefinition definition, float elapsedSeconds)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (elapsedSeconds <= 0f || elapsedSeconds >= definition.DurationSeconds)
                return VictoryCelebrationPose.Neutral;

            var t = elapsedSeconds / definition.DurationSeconds;
            switch (definition.Motion)
            {
                case VictoryCelebrationMotion.Aura67:
                    return SampleAura67(t);
                case VictoryCelebrationMotion.PogoBow:
                    return SamplePogoBow(t);
                case VictoryCelebrationMotion.SkyPop:
                    return SampleSkyPop(t);
                case VictoryCelebrationMotion.SideSway:
                    return SampleSideSway(t);
                case VictoryCelebrationMotion.CrownPulse:
                    return SampleCrownPulse(t);
                case VictoryCelebrationMotion.VictoryBounce:
                default:
                    return SampleVictoryBounce(t);
            }
        }

        private static VictoryCelebrationPose SampleAura67(float t)
        {
            // Three full see-saw cycles = six alternating hand accents. Both
            // palms stay presented upward while the hands trade height.
            var envelope = EntranceExitEnvelope(t);
            var wave = (float)Math.Sin(t * Math.PI * 6.0);
            var pulse = Math.Abs(wave);
            var leftLift = envelope * (0.58f + 0.34f * wave);
            var rightLift = envelope * (0.58f - 0.34f * wave);

            return new VictoryCelebrationPose(
                envelope * (0.07f + 0.09f * (float)pulse),
                envelope * wave * 0.10f,
                envelope * -wave * 0.06f,
                envelope * -wave * 0.10f,
                leftLift,
                rightLift,
                envelope * 0.72f,
                envelope * 0.72f,
                envelope,
                envelope,
                envelope * (0.55f + 0.45f * (float)pulse));
        }

        private static VictoryCelebrationPose SamplePogoBow(float t)
        {
            var envelope = EntranceExitEnvelope(t);
            var pulse = 0.5f - 0.5f * (float)Math.Cos(t * Math.PI * 4.0);
            return new VictoryCelebrationPose(
                -envelope * pulse * 0.12f,
                envelope * pulse * 0.62f,
                0f,
                0f,
                envelope * 0.14f,
                envelope * 0.14f,
                envelope * 0.18f,
                envelope * 0.18f,
                0f,
                0f,
                envelope * pulse);
        }

        private static VictoryCelebrationPose SampleSkyPop(float t)
        {
            var envelope = EntranceExitEnvelope(t);
            var wave = (float)Math.Sin(t * Math.PI * 5.0);
            var pulse = Math.Abs(wave);
            var lift = envelope * (0.42f + 0.52f * (float)pulse);
            return new VictoryCelebrationPose(
                envelope * (float)pulse * 0.20f,
                envelope * wave * 0.04f,
                envelope * wave * 0.10f,
                envelope * -wave * 0.08f,
                lift,
                lift,
                envelope * 0.42f,
                envelope * 0.42f,
                envelope * 0.16f,
                envelope * 0.16f,
                envelope * (float)pulse);
        }

        private static VictoryCelebrationPose SampleSideSway(float t)
        {
            var envelope = EntranceExitEnvelope(t);
            var wave = (float)Math.Sin(t * Math.PI * 4.0);
            var pulse = Math.Abs(wave);
            return new VictoryCelebrationPose(
                envelope * (0.05f + (float)pulse * 0.07f),
                envelope * wave * 0.30f,
                envelope * -wave * 0.20f,
                envelope * -wave * 0.18f,
                envelope * (0.52f + 0.24f * wave),
                envelope * (0.52f - 0.24f * wave),
                envelope * 0.58f,
                envelope * 0.58f,
                envelope * 0.72f,
                envelope * 0.72f,
                envelope * (0.50f + 0.50f * (float)pulse));
        }

        private static VictoryCelebrationPose SampleCrownPulse(float t)
        {
            var envelope = EntranceExitEnvelope(t);
            var pulse = Math.Abs((float)Math.Sin(t * Math.PI * 3.0));
            var sway = (float)Math.Sin(t * Math.PI * 2.0);
            var lift = envelope * (0.64f + 0.30f * (float)pulse);
            return new VictoryCelebrationPose(
                envelope * (0.08f + 0.15f * (float)pulse),
                envelope * sway * 0.08f,
                envelope * sway * 0.14f,
                envelope * -sway * 0.18f,
                lift,
                lift,
                envelope * 0.62f,
                envelope * 0.62f,
                envelope * 0.28f,
                envelope * 0.28f,
                envelope * (0.62f + 0.38f * (float)pulse));
        }

        private static VictoryCelebrationPose SampleVictoryBounce(float t)
        {
            var envelope = EntranceExitEnvelope(t);
            var wave = (float)Math.Sin(t * Math.PI * 4.0);
            var pulse = (float)Math.Abs(wave);
            var lift = envelope * (0.35f + pulse * 0.35f);
            return new VictoryCelebrationPose(
                envelope * pulse * 0.22f,
                envelope * wave * 0.05f,
                envelope * wave * 0.12f,
                envelope * -wave * 0.08f,
                lift,
                lift,
                envelope * 0.35f,
                envelope * 0.35f,
                0f,
                0f,
                envelope * pulse);
        }

        private static float EntranceExitEnvelope(float t)
        {
            var enter = SmoothStep(0f, 0.12f, t);
            var exit = 1f - SmoothStep(0.88f, 1f, t);
            return Clamp01(enter * exit);
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            if (value <= edge0) return 0f;
            if (value >= edge1) return 1f;
            var x = (value - edge0) / (edge1 - edge0);
            return x * x * (3f - 2f * x);
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
