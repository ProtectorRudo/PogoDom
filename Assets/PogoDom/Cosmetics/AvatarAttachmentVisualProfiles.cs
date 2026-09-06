using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public enum HeadwearVisualFamily
    {
        SportCap = 0,
        SplitCrown = 1,
        TechAntenna = 2,
        MascotEars = 3,
        StreetBeanie = 4,
        CaptainCrest = 5
    }

    public enum BackVisualFamily
    {
        SportPack = 0,
        TricksterFins = 1,
        TechBattery = 2,
        MascotTail = 3,
        StreetCape = 4,
        CaptainBanner = 5
    }

    public enum AuraVisualFamily
    {
        OrbitRing = 0,
        SplitOrbit = 1,
        HexPulse = 2,
        BubbleOrbit = 3,
        GraffitiOrbit = 4,
        CrestOrbit = 5
    }

    public enum TrailVisualFamily
    {
        Dash = 0,
        Zigzag = 1,
        Pulse = 2,
        Bubbles = 3,
        Ribbon = 4,
        Royal = 5
    }

    public enum LandingVisualFamily
    {
        Ring = 0,
        SplitRing = 1,
        TechRing = 2,
        BubblePop = 3,
        StreetStamp = 4,
        CrownBurst = 5
    }

    /// <summary>
    /// Presentation-only identity kit. It deliberately contains no competitive
    /// values: no movement, jump cadence, collision, score, stun or power stats.
    /// </summary>
    public sealed class AvatarAttachmentVisualProfile
    {
        public string Id { get; }
        public HeadwearVisualFamily Headwear { get; }
        public BackVisualFamily Back { get; }
        public AuraVisualFamily Aura { get; }
        public TrailVisualFamily Trail { get; }
        public LandingVisualFamily Landing { get; }
        public float HeadwearScale { get; }
        public float BackScale { get; }
        public float AuraRadius { get; }
        public float TrailWidth { get; }
        public float LandingRadius { get; }

        public AvatarAttachmentVisualProfile(
            string id,
            HeadwearVisualFamily headwear,
            BackVisualFamily back,
            AuraVisualFamily aura,
            TrailVisualFamily trail,
            LandingVisualFamily landing,
            float headwearScale,
            float backScale,
            float auraRadius,
            float trailWidth,
            float landingRadius)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Profile id is required.", nameof(id));
            if (headwearScale <= 0f || headwearScale > 1.5f) throw new ArgumentOutOfRangeException(nameof(headwearScale));
            if (backScale <= 0f || backScale > 1.5f) throw new ArgumentOutOfRangeException(nameof(backScale));
            if (auraRadius <= 0.2f || auraRadius > 1.2f) throw new ArgumentOutOfRangeException(nameof(auraRadius));
            if (trailWidth <= 0.02f || trailWidth > 0.30f) throw new ArgumentOutOfRangeException(nameof(trailWidth));
            if (landingRadius <= 0.2f || landingRadius > 1.4f) throw new ArgumentOutOfRangeException(nameof(landingRadius));

            Id = id.Trim().ToLowerInvariant();
            Headwear = headwear;
            Back = back;
            Aura = aura;
            Trail = trail;
            Landing = landing;
            HeadwearScale = headwearScale;
            BackScale = backScale;
            AuraRadius = auraRadius;
            TrailWidth = trailWidth;
            LandingRadius = landingRadius;
        }

        public string Signature => Headwear + "|" + Back + "|" + Aura + "|" + Trail + "|" + Landing;
    }

    public static class AvatarAttachmentVisualProfiles
    {
        private static readonly AvatarAttachmentVisualProfile[] LaunchProfiles =
        {
            new AvatarAttachmentVisualProfile("hero", HeadwearVisualFamily.SportCap, BackVisualFamily.SportPack, AuraVisualFamily.OrbitRing, TrailVisualFamily.Dash, LandingVisualFamily.Ring, 1.00f, 0.96f, 0.58f, 0.12f, 0.72f),
            new AvatarAttachmentVisualProfile("trickster", HeadwearVisualFamily.SplitCrown, BackVisualFamily.TricksterFins, AuraVisualFamily.SplitOrbit, TrailVisualFamily.Zigzag, LandingVisualFamily.SplitRing, 0.94f, 1.02f, 0.62f, 0.105f, 0.76f),
            new AvatarAttachmentVisualProfile("tech", HeadwearVisualFamily.TechAntenna, BackVisualFamily.TechBattery, AuraVisualFamily.HexPulse, TrailVisualFamily.Pulse, LandingVisualFamily.TechRing, 0.92f, 0.94f, 0.55f, 0.10f, 0.70f),
            new AvatarAttachmentVisualProfile("mascot", HeadwearVisualFamily.MascotEars, BackVisualFamily.MascotTail, AuraVisualFamily.BubbleOrbit, TrailVisualFamily.Bubbles, LandingVisualFamily.BubblePop, 1.08f, 1.05f, 0.64f, 0.13f, 0.80f),
            new AvatarAttachmentVisualProfile("street", HeadwearVisualFamily.StreetBeanie, BackVisualFamily.StreetCape, AuraVisualFamily.GraffitiOrbit, TrailVisualFamily.Ribbon, LandingVisualFamily.StreetStamp, 1.02f, 1.08f, 0.60f, 0.115f, 0.78f),
            new AvatarAttachmentVisualProfile("captain", HeadwearVisualFamily.CaptainCrest, BackVisualFamily.CaptainBanner, AuraVisualFamily.CrestOrbit, TrailVisualFamily.Royal, LandingVisualFamily.CrownBurst, 1.00f, 1.00f, 0.57f, 0.105f, 0.74f)
        };

        public static IReadOnlyList<AvatarAttachmentVisualProfile> Launch => LaunchProfiles;

        public static AvatarAttachmentVisualProfile ForStyle(int styleIndex)
        {
            var index = styleIndex % LaunchProfiles.Length;
            if (index < 0) index += LaunchProfiles.Length;
            return LaunchProfiles[index];
        }
    }
}
