using System;
using System.Collections.Generic;

namespace PogoDom.Cosmetics
{
    public enum AvatarSilhouetteFamily
    {
        Hero = 0,
        Trickster = 1,
        Tech = 2,
        Mascot = 3,
        Street = 4,
        Captain = 5
    }

    public sealed class AvatarArchetypeProfile
    {
        public string Id { get; }
        public AvatarSilhouetteFamily Silhouette { get; }
        public float HeadToBodyRatio { get; }
        public float BodyWidth { get; }
        public float TopFeature { get; }
        public float SideFeature { get; }
        public float Asymmetry { get; }
        public AvatarRigCapability Capabilities { get; }

        public AvatarArchetypeProfile(
            string id,
            AvatarSilhouetteFamily silhouette,
            float headToBodyRatio,
            float bodyWidth,
            float topFeature,
            float sideFeature,
            float asymmetry,
            AvatarRigCapability capabilities)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Profile id is required.", nameof(id));
            Require01(headToBodyRatio, nameof(headToBodyRatio));
            Require01(bodyWidth, nameof(bodyWidth));
            Require01(topFeature, nameof(topFeature));
            Require01(sideFeature, nameof(sideFeature));
            Require01(asymmetry, nameof(asymmetry));

            Id = id.Trim().ToLowerInvariant();
            Silhouette = silhouette;
            HeadToBodyRatio = headToBodyRatio;
            BodyWidth = bodyWidth;
            TopFeature = topFeature;
            SideFeature = sideFeature;
            Asymmetry = asymmetry;
            Capabilities = capabilities;
        }

        public string SilhouetteSignature =>
            ((int)Silhouette) + ":" + Quantize(HeadToBodyRatio) + ":" + Quantize(BodyWidth) + ":" +
            Quantize(TopFeature) + ":" + Quantize(SideFeature) + ":" + Quantize(Asymmetry);

        private static int Quantize(float value) => (int)Math.Round(value * 10f, MidpointRounding.AwayFromZero);

        private static void Require01(float value, string name)
        {
            if (value < 0f || value > 1f) throw new ArgumentOutOfRangeException(name, "Value must be normalized 0..1.");
        }
    }

    public static class AvatarArchetypeProfiles
    {
        private static readonly AvatarArchetypeProfile[] Defaults =
        {
            new AvatarArchetypeProfile("hero", AvatarSilhouetteFamily.Hero, 0.72f, 0.48f, 0.58f, 0.18f, 0.08f, AvatarRigCapabilities.StandardHumanoid),
            new AvatarArchetypeProfile("trickster", AvatarSilhouetteFamily.Trickster, 0.66f, 0.43f, 0.20f, 0.78f, 0.30f, AvatarRigCapabilities.StandardHumanoid),
            new AvatarArchetypeProfile("tech", AvatarSilhouetteFamily.Tech, 0.62f, 0.40f, 0.88f, 0.22f, 0.15f, AvatarRigCapabilities.StandardHumanoid),
            new AvatarArchetypeProfile("mascot", AvatarSilhouetteFamily.Mascot, 0.86f, 0.64f, 0.72f, 0.52f, 0.10f, AvatarRigCapabilities.Mascot),
            new AvatarArchetypeProfile("street", AvatarSilhouetteFamily.Street, 0.70f, 0.54f, 0.34f, 0.38f, 0.72f, AvatarRigCapabilities.StandardHumanoid),
            new AvatarArchetypeProfile("captain", AvatarSilhouetteFamily.Captain, 0.68f, 0.58f, 0.74f, 0.32f, 0.22f, AvatarRigCapabilities.StandardHumanoid)
        };

        public static IReadOnlyList<AvatarArchetypeProfile> All => Defaults;

        public static AvatarArchetypeProfile Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Profile id is required.", nameof(id));
            var normalized = id.Trim().ToLowerInvariant();
            for (var i = 0; i < Defaults.Length; i++)
                if (Defaults[i].Id == normalized) return Defaults[i];
            throw new KeyNotFoundException("Unknown avatar archetype: " + id);
        }
    }
}
