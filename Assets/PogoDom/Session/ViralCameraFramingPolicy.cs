using System;

namespace PogoDom.Session
{
    public readonly struct ViralCameraFrame
    {
        public float Distance { get; }
        public float VerticalFovDegrees { get; }
        public float PitchDegrees { get; }
        public float SafeWidthFraction { get; }
        public float SafeHeightFraction { get; }

        public ViralCameraFrame(float distance, float verticalFovDegrees, float pitchDegrees, float safeWidthFraction, float safeHeightFraction)
        {
            Distance = distance;
            VerticalFovDegrees = verticalFovDegrees;
            PitchDegrees = pitchDegrees;
            SafeWidthFraction = safeWidthFraction;
            SafeHeightFraction = safeHeightFraction;
        }
    }

    /// <summary>
    /// Pure phone/replay camera math. Keeps the whole arena legible under
    /// portrait crops without letting aspect ratio change battle state.
    /// </summary>
    public static class ViralCameraFramingPolicy
    {
        public const float GameplayFov = 39f;
        public const float GameplayPitch = 48f;
        public const float PortraitSafeWidth = 0.88f;
        public const float PortraitSafeHeight = 0.74f;
        public const float LandscapeSafeWidth = 0.90f;
        public const float LandscapeSafeHeight = 0.82f;

        public static ViralCameraFrame Fit(
            float boardWidth,
            float boardDepth,
            float aspectRatio,
            float visualHeight = 2.0f,
            float worldPadding = 1.25f)
        {
            if (boardWidth <= 0f) throw new ArgumentOutOfRangeException(nameof(boardWidth));
            if (boardDepth <= 0f) throw new ArgumentOutOfRangeException(nameof(boardDepth));
            if (aspectRatio <= 0f) throw new ArgumentOutOfRangeException(nameof(aspectRatio));
            if (visualHeight < 0f) throw new ArgumentOutOfRangeException(nameof(visualHeight));
            if (worldPadding < 0f) throw new ArgumentOutOfRangeException(nameof(worldPadding));

            var portrait = aspectRatio < 1f;
            var safeWidth = portrait ? PortraitSafeWidth : LandscapeSafeWidth;
            var safeHeight = portrait ? PortraitSafeHeight : LandscapeSafeHeight;
            var pitch = GameplayPitch * (float)Math.PI / 180f;
            var verticalFov = GameplayFov * (float)Math.PI / 180f;
            var horizontalFov = 2f * (float)Math.Atan(Math.Tan(verticalFov * 0.5f) * aspectRatio);

            var halfWidth = boardWidth * 0.5f + worldPadding;
            var halfDepth = boardDepth * 0.5f + worldPadding;
            var halfHeight = visualHeight * 0.5f;

            // Board depth projects vertically because the camera is pitched.
            // Avatar/pickup height consumes the remaining vertical extent.
            var projectedVerticalHalf =
                halfDepth * (float)Math.Sin(pitch) +
                halfHeight * (float)Math.Cos(pitch);

            var horizontalDistance = halfWidth / Math.Max(0.001f, (float)Math.Tan(horizontalFov * 0.5f) * safeWidth);
            var verticalDistance = projectedVerticalHalf / Math.Max(0.001f, (float)Math.Tan(verticalFov * 0.5f) * safeHeight);

            // Small fixed breathing room protects outlines, trails and corner beacons.
            var distance = Math.Max(horizontalDistance, verticalDistance) + 0.75f;
            return new ViralCameraFrame(distance, GameplayFov, GameplayPitch, safeWidth, safeHeight);
        }
    }
}
