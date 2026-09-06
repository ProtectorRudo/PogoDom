using System;
using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public enum PickupVisualArchetype
    {
        BankVault = 0,
        MysteryRelic = 1,
        DirectionArrow = 2,
        SpeedBurst = 3,
        MissileRocket = 4,
        PadlockShield = 5
    }

    public readonly struct PickupVisualIdentity
    {
        public PowerUpKind Kind { get; }
        public PickupVisualArchetype Archetype { get; }
        public float BobAmplitude { get; }
        public float SpinDegreesPerSecond { get; }
        public float PulseScale { get; }
        public int AccentPartBudget { get; }

        public PickupVisualIdentity(
            PowerUpKind kind,
            PickupVisualArchetype archetype,
            float bobAmplitude,
            float spinDegreesPerSecond,
            float pulseScale,
            int accentPartBudget)
        {
            Kind = kind;
            Archetype = archetype;
            BobAmplitude = bobAmplitude;
            SpinDegreesPerSecond = spinDegreesPerSecond;
            PulseScale = pulseScale;
            AccentPartBudget = accentPartBudget;
        }
    }

    /// <summary>
    /// Presentation-only identity contract. Each gameplay pickup must be readable
    /// by silhouette/motion before the player learns its color.
    /// </summary>
    public static class PickupVisualIdentityPolicy
    {
        public static PickupVisualIdentity Get(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.BankCrate:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.BankVault, 0.025f, 22f, 0.035f, 4);
                case PowerUpKind.MysteryCrate:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.MysteryRelic, 0.055f, 58f, 0.065f, 4);
                case PowerUpKind.Arrow:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.DirectionArrow, 0.018f, 0f, 0.045f, 3);
                case PowerUpKind.Speed:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.SpeedBurst, 0.070f, 95f, 0.075f, 4);
                case PowerUpKind.Missile:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.MissileRocket, 0.035f, 34f, 0.040f, 4);
                case PowerUpKind.Padlock:
                    return new PickupVisualIdentity(kind, PickupVisualArchetype.PadlockShield, 0.030f, 18f, 0.050f, 4);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Pickup has no launch visual identity.");
            }
        }

        public static bool IsLaunchPickup(PowerUpKind kind)
        {
            return kind == PowerUpKind.BankCrate ||
                   kind == PowerUpKind.MysteryCrate ||
                   kind == PowerUpKind.Arrow ||
                   kind == PowerUpKind.Speed ||
                   kind == PowerUpKind.Missile ||
                   kind == PowerUpKind.Padlock;
        }
    }
}
