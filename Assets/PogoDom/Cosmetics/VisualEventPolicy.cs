using System;
using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public enum VisualImpactTier
    {
        Ambient = 0,
        Readable = 1,
        Spectacle = 2
    }

    public readonly struct VisualEventCue
    {
        public SkillVisualTrigger Trigger { get; }
        public VisualImpactTier Impact { get; }
        public float LifetimeSeconds { get; }
        public float RadiusTiles { get; }
        public float CameraImpulse { get; }
        public int ParticleBudget { get; }

        public VisualEventCue(
            SkillVisualTrigger trigger,
            VisualImpactTier impact,
            float lifetimeSeconds,
            float radiusTiles,
            float cameraImpulse,
            int particleBudget)
        {
            if (lifetimeSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds));
            if (radiusTiles <= 0f) throw new ArgumentOutOfRangeException(nameof(radiusTiles));
            if (cameraImpulse < 0f || cameraImpulse > 1f) throw new ArgumentOutOfRangeException(nameof(cameraImpulse));
            if (particleBudget < 0) throw new ArgumentOutOfRangeException(nameof(particleBudget));
            Trigger = trigger;
            Impact = impact;
            LifetimeSeconds = lifetimeSeconds;
            RadiusTiles = radiusTiles;
            CameraImpulse = cameraImpulse;
            ParticleBudget = particleBudget;
        }
    }

    /// <summary>
    /// Presentation-only translation from deterministic match events into a small
    /// mobile VFX budget. Nothing from this policy is ever fed back into Core.
    /// </summary>
    public static class VisualEventPolicy
    {
        public static bool TryDescribe(MatchEvent e, out VisualEventCue cue)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            switch (e.Type)
            {
                case MatchEventType.TileStolen:
                    cue = new VisualEventCue(SkillVisualTrigger.TileSteal, VisualImpactTier.Ambient, 0.22f, 0.48f, 0f, 5);
                    return true;
                case MatchEventType.TileProtected:
                    cue = new VisualEventCue(SkillVisualTrigger.ShieldBlock, VisualImpactTier.Readable, 0.34f, 0.62f, 0.04f, 9);
                    return true;
                case MatchEventType.EnclosureCaptured:
                    cue = new VisualEventCue(
                        SkillVisualTrigger.AreaCapture,
                        VisualImpactTier.Spectacle,
                        0.62f,
                        Clamp(0.75f + Math.Max(0, e.Value) * 0.055f, 0.75f, 2.15f),
                        Clamp(0.08f + Math.Max(0, e.Value) * 0.012f, 0.08f, 0.30f),
                        ClampInt(12 + Math.Max(0, e.Value) * 2, 12, 42));
                    return true;
                case MatchEventType.Banked:
                {
                    var big = e.Value >= 8;
                    cue = new VisualEventCue(
                        SkillVisualTrigger.Bank,
                        big ? VisualImpactTier.Spectacle : VisualImpactTier.Readable,
                        big ? 0.64f : 0.42f,
                        big ? 1.20f : 0.82f,
                        big ? Clamp(0.10f + e.Value * 0.008f, 0.10f, 0.24f) : 0.03f,
                        big ? ClampInt(18 + e.Value, 18, 38) : 11);
                    return true;
                }
                case MatchEventType.CrateOpened:
                    cue = new VisualEventCue(SkillVisualTrigger.CrateOpen, VisualImpactTier.Readable, 0.46f, 0.82f, 0.04f, 14);
                    return true;
                case MatchEventType.ArrowUsed:
                    cue = new VisualEventCue(SkillVisualTrigger.Arrow, VisualImpactTier.Spectacle, 0.52f, 1.00f, 0.12f, 18);
                    return true;
                case MatchEventType.SpeedActivated:
                    cue = new VisualEventCue(SkillVisualTrigger.Speed, VisualImpactTier.Readable, 0.42f, 0.72f, 0.02f, 10);
                    return true;
                case MatchEventType.MissileFired:
                    cue = new VisualEventCue(SkillVisualTrigger.Missile, VisualImpactTier.Spectacle, 0.58f, 0.92f, 0.14f, 20);
                    return true;
                case MatchEventType.PadlockActivated:
                    cue = new VisualEventCue(SkillVisualTrigger.Padlock, VisualImpactTier.Readable, 0.50f, 0.90f, 0.03f, 12);
                    return true;
                case MatchEventType.PlayerStunned:
                    cue = new VisualEventCue(SkillVisualTrigger.Stun, VisualImpactTier.Readable, 0.52f, 0.66f, 0.05f, 11);
                    return true;
                case MatchEventType.HazardDetonated:
                    cue = new VisualEventCue(SkillVisualTrigger.HazardBlast, VisualImpactTier.Spectacle, 0.62f, 1.55f, 0.20f, 28);
                    return true;
                default:
                    cue = default;
                    return false;
            }
        }

        public static SkillVisualTrigger? TriggerFor(MatchEvent e)
        {
            VisualEventCue cue;
            return TryDescribe(e, out cue) ? cue.Trigger : (SkillVisualTrigger?)null;
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        private static int ClampInt(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
