using System;

namespace PogoDom.Cosmetics
{
    public readonly struct FramePressureSample
    {
        public float SmoothedFrameMilliseconds { get; }
        public float PressureSeconds { get; }

        public FramePressureSample(float smoothedFrameMilliseconds, float pressureSeconds)
        {
            if (smoothedFrameMilliseconds < 0f) throw new ArgumentOutOfRangeException(nameof(smoothedFrameMilliseconds));
            if (pressureSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(pressureSeconds));
            SmoothedFrameMilliseconds = smoothedFrameMilliseconds;
            PressureSeconds = pressureSeconds;
        }
    }

    /// <summary>
    /// Conservative quality governor. It can only step DOWN during a match.
    /// Recovery/upgrades happen on a future session so a live battle never pulses
    /// between visual tiers while the player is trying to read the board.
    /// </summary>
    public static class AdaptiveVisualQualityPolicy
    {
        public const float PressureFrameMilliseconds = 28.0f;
        public const float CriticalFrameMilliseconds = 40.0f;
        public const float PressureSecondsToDowngrade = 2.5f;
        public const float CriticalSecondsToDowngrade = 0.9f;

        public static bool ShouldDowngrade(FramePressureSample sample)
        {
            if (sample.SmoothedFrameMilliseconds >= CriticalFrameMilliseconds)
                return sample.PressureSeconds >= CriticalSecondsToDowngrade;
            return sample.SmoothedFrameMilliseconds >= PressureFrameMilliseconds &&
                   sample.PressureSeconds >= PressureSecondsToDowngrade;
        }

        public static VisualQualityTier NextLower(VisualQualityTier tier)
        {
            switch (tier)
            {
                case VisualQualityTier.Showcase: return VisualQualityTier.Balanced;
                case VisualQualityTier.Balanced: return VisualQualityTier.Lite;
                default: return VisualQualityTier.Lite;
            }
        }

        public static float UpdatePressureSeconds(float currentSeconds, float smoothedFrameMilliseconds, float deltaSeconds)
        {
            if (currentSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(currentSeconds));
            if (smoothedFrameMilliseconds < 0f) throw new ArgumentOutOfRangeException(nameof(smoothedFrameMilliseconds));
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

            if (smoothedFrameMilliseconds >= PressureFrameMilliseconds)
                return currentSeconds + deltaSeconds;

            // Healthy frames drain pressure faster than it builds so a short
            // loading hitch cannot downgrade the entire next minute of gameplay.
            return Math.Max(0f, currentSeconds - deltaSeconds * 2f);
        }
    }
}
