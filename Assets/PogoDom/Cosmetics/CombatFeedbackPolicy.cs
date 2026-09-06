using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public enum CombatFeedbackKind
    {
        None = 0,
        MissileTrace = 1,
        StunImpact = 2,
        ShieldBlock = 3
    }

    public readonly struct CombatFeedbackCue
    {
        public CombatFeedbackKind Kind { get; }
        public int SourcePlayerId { get; }
        public int TargetPlayerId { get; }
        public float LifetimeSeconds { get; }
        public float WidthScale { get; }
        public float PulseStrength { get; }

        public CombatFeedbackCue(
            CombatFeedbackKind kind,
            int sourcePlayerId,
            int targetPlayerId,
            float lifetimeSeconds,
            float widthScale,
            float pulseStrength)
        {
            Kind = kind;
            SourcePlayerId = sourcePlayerId;
            TargetPlayerId = targetPlayerId;
            LifetimeSeconds = lifetimeSeconds;
            WidthScale = widthScale;
            PulseStrength = pulseStrength;
        }
    }

    /// <summary>
    /// Pure presentation policy for competitive cause/effect readability.
    /// It consumes already-resolved MatchEvent values and never alters Core state.
    /// </summary>
    public static class CombatFeedbackPolicy
    {
        public static bool TryDescribe(MatchEvent e, out CombatFeedbackCue cue)
        {
            switch (e.Type)
            {
                case MatchEventType.MissileFired:
                    if (!ValidPair(e.PlayerId, e.SecondaryPlayerId)) break;
                    cue = new CombatFeedbackCue(
                        CombatFeedbackKind.MissileTrace,
                        e.PlayerId,
                        e.SecondaryPlayerId,
                        0.24f,
                        0.075f,
                        0.72f);
                    return true;

                case MatchEventType.PlayerStunned:
                    // Core emits PlayerId = stunned target and SecondaryPlayerId = attacker.
                    if (!ValidPair(e.SecondaryPlayerId, e.PlayerId)) break;
                    cue = new CombatFeedbackCue(
                        CombatFeedbackKind.StunImpact,
                        e.SecondaryPlayerId,
                        e.PlayerId,
                        0.34f,
                        0.0f,
                        1.0f);
                    return true;

                case MatchEventType.TileProtected:
                    // Core emits PlayerId = thief and SecondaryPlayerId = padlock owner.
                    if (!ValidPair(e.PlayerId, e.SecondaryPlayerId)) break;
                    cue = new CombatFeedbackCue(
                        CombatFeedbackKind.ShieldBlock,
                        e.PlayerId,
                        e.SecondaryPlayerId,
                        0.30f,
                        0.0f,
                        0.82f);
                    return true;
            }

            cue = default;
            return false;
        }

        private static bool ValidPair(int source, int target)
        {
            return source >= 0 && target >= 0 && source != target;
        }
    }
}
