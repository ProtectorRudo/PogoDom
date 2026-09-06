using System;
using PogoDom.Core;

namespace PogoDom.Cosmetics
{
    public enum AvatarExpressionKind
    {
        Neutral = 0,
        Excited = 1,
        Proud = 2,
        Attack = 3,
        Hurt = 4,
        Shielded = 5,
        Surprised = 6
    }

    public readonly struct AvatarExpressionCue
    {
        public int PlayerId { get; }
        public AvatarExpressionKind Kind { get; }
        public float DurationSeconds { get; }
        public float Strength { get; }

        public AvatarExpressionCue(int playerId, AvatarExpressionKind kind, float durationSeconds, float strength)
        {
            if (playerId < 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            if (durationSeconds <= 0f || durationSeconds > 0.85f) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            if (strength < 0f || strength > 1f) throw new ArgumentOutOfRangeException(nameof(strength));
            PlayerId = playerId;
            Kind = kind;
            DurationSeconds = durationSeconds;
            Strength = strength;
        }
    }

    /// <summary>
    /// Converts important gameplay events into short character reactions. Routine
    /// tile painting/stealing is intentionally ignored so faces remain meaningful.
    /// </summary>
    public static class AvatarExpressionPolicy
    {
        public static bool TryDescribe(MatchEvent e, out AvatarExpressionCue cue)
        {
            if (e == null)
            {
                cue = default;
                return false;
            }

            switch (e.Type)
            {
                case MatchEventType.Banked:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(
                        e.PlayerId,
                        e.Value >= 8 ? AvatarExpressionKind.Proud : AvatarExpressionKind.Excited,
                        e.Value >= 8 ? 0.72f : 0.48f,
                        e.Value >= 8 ? 1f : 0.72f);
                    return true;

                case MatchEventType.EnclosureCaptured:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Proud, 0.68f, Math.Min(1f, 0.55f + e.Value * 0.045f));
                    return true;

                case MatchEventType.CrateOpened:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Surprised, 0.46f, 0.82f);
                    return true;

                case MatchEventType.MissileFired:
                case MatchEventType.ArrowUsed:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Attack, 0.40f, 0.86f);
                    return true;

                case MatchEventType.PlayerStunned:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Hurt, 0.70f, 1f);
                    return true;

                case MatchEventType.TileProtected:
                    if (e.SecondaryPlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.SecondaryPlayerId, AvatarExpressionKind.Shielded, 0.52f, 0.88f);
                    return true;

                case MatchEventType.PadlockActivated:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Shielded, 0.48f, 0.72f);
                    return true;

                case MatchEventType.SpeedActivated:
                    if (e.PlayerId < 0) break;
                    cue = new AvatarExpressionCue(e.PlayerId, AvatarExpressionKind.Excited, 0.40f, 0.72f);
                    return true;
            }

            cue = default;
            return false;
        }
    }
}
