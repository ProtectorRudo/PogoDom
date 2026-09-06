using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Session
{
    public enum EngagementBeatKind
    {
        Steal = 0,
        BigBank = 1,
        Arrow = 2,
        PowerPickup = 3,
        MissileAttack = 4,
        Stun = 5,
        Enclosure = 6,
        HazardWarning = 7,
        HazardBlast = 8,
        Defense = 9,
        LeadChange = 10,
        MysteryCrate = 11
    }

    public enum EngagementTier
    {
        Activity = 0,
        Spectacle = 1
    }

    public sealed class EngagementBeat
    {
        public int Tick { get; }
        public EngagementBeatKind Kind { get; }
        public int PlayerId { get; }
        public int Value { get; }
        public bool IsSpectacle { get; }

        public EngagementBeat(int tick, EngagementBeatKind kind, int playerId, int value, bool isSpectacle)
        {
            Tick = tick;
            Kind = kind;
            PlayerId = playerId;
            Value = value;
            IsSpectacle = isSpectacle;
        }
    }

    public sealed class QuietSpan
    {
        public int StartTick { get; }
        public int EndTickExclusive { get; }
        public int DurationTicks => EndTickExclusive - StartTick;

        public QuietSpan(int startTick, int endTickExclusive)
        {
            StartTick = startTick;
            EndTickExclusive = endTickExclusive;
        }

        public float DurationSeconds(float tickSeconds) => DurationTicks * tickSeconds;
    }

    public sealed class MatchEngagementReport
    {
        private readonly List<QuietSpan> _activityQuiet;
        private readonly List<QuietSpan> _spectacleQuiet;

        public int TotalTicks { get; }
        public float TickSeconds { get; }
        public int ActivityBeatTicks { get; }
        public int SpectacleBeatTicks { get; }
        public IReadOnlyList<QuietSpan> ActivityQuietSpans => _activityQuiet;
        public IReadOnlyList<QuietSpan> SpectacleQuietSpans => _spectacleQuiet;
        public int LongestActivityQuietTicks { get; }
        public int LongestSpectacleQuietTicks { get; }
        public float LongestActivityQuietSeconds => LongestActivityQuietTicks * TickSeconds;
        public float LongestSpectacleQuietSeconds => LongestSpectacleQuietTicks * TickSeconds;

        internal MatchEngagementReport(
            int totalTicks,
            float tickSeconds,
            int activityBeatTicks,
            int spectacleBeatTicks,
            List<QuietSpan> activityQuiet,
            List<QuietSpan> spectacleQuiet)
        {
            TotalTicks = totalTicks;
            TickSeconds = tickSeconds;
            ActivityBeatTicks = activityBeatTicks;
            SpectacleBeatTicks = spectacleBeatTicks;
            _activityQuiet = activityQuiet;
            _spectacleQuiet = spectacleQuiet;
            LongestActivityQuietTicks = Longest(activityQuiet);
            LongestSpectacleQuietTicks = Longest(spectacleQuiet);
        }

        public int CountQuietSpansAtLeast(EngagementTier tier, float seconds)
        {
            var minimumTicks = MinimumTicks(seconds);
            var spans = tier == EngagementTier.Activity ? _activityQuiet : _spectacleQuiet;
            var count = 0;
            for (var i = 0; i < spans.Count; i++)
                if (spans[i].DurationTicks >= minimumTicks) count++;
            return count;
        }

        public int QuietTicksInsideSpansAtLeast(EngagementTier tier, float seconds)
        {
            var minimumTicks = MinimumTicks(seconds);
            var spans = tier == EngagementTier.Activity ? _activityQuiet : _spectacleQuiet;
            var ticks = 0;
            for (var i = 0; i < spans.Count; i++)
                if (spans[i].DurationTicks >= minimumTicks) ticks += spans[i].DurationTicks;
            return ticks;
        }

        private int MinimumTicks(float seconds)
        {
            if (seconds <= 0f) return 1;
            return Math.Max(1, (int)Math.Ceiling(seconds / TickSeconds));
        }

        private static int Longest(List<QuietSpan> spans)
        {
            var longest = 0;
            for (var i = 0; i < spans.Count; i++)
                if (spans[i].DurationTicks > longest) longest = spans[i].DurationTicks;
            return longest;
        }
    }

    /// <summary>
    /// Presentation/analytics-only detector for dead zones. It never feeds back
    /// into battle rules or bot decisions, so measuring a match cannot alter it.
    /// Activity includes ordinary territorial conflict; Spectacle is deliberately
    /// stricter and tracks beats worth noticing in a short mobile session.
    /// </summary>
    public sealed class MatchEngagementTracker
    {
        private readonly float _tickSeconds;
        private readonly HashSet<int> _activityTicks = new HashSet<int>();
        private readonly HashSet<int> _spectacleTicks = new HashSet<int>();
        private readonly List<EngagementBeat> _beats = new List<EngagementBeat>();
        private int _previousLeaderId = -1;
        private bool _hasLeaderSample;
        private int _lastObservedTick = -1;
        private MatchEngagementReport _completed;

        public MatchEngagementTracker(float tickSeconds)
        {
            if (tickSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(tickSeconds));
            _tickSeconds = tickSeconds;
        }

        public IReadOnlyList<EngagementBeat> Beats => _beats;

        public void Observe(int tickIndex, MatchState state, TickResult tick)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            if (_completed != null) throw new InvalidOperationException("Engagement tracker is complete.");
            if (tickIndex < 0) throw new ArgumentOutOfRangeException(nameof(tickIndex));
            if (tickIndex <= _lastObservedTick) throw new InvalidOperationException("Engagement ticks must be strictly increasing.");
            _lastObservedTick = tickIndex;

            for (var i = 0; i < tick.Events.Count; i++)
                ObserveEvent(tickIndex, tick.Events[i]);

            var leader = MatchOutcome.Leader(state);
            var leaderId = leader == null ? -1 : leader.Id;
            if (_hasLeaderSample && leaderId >= 0 && _previousLeaderId >= 0 && leaderId != _previousLeaderId)
                Add(tickIndex, EngagementBeatKind.LeadChange, leaderId, 0, true);
            _previousLeaderId = leaderId;
            _hasLeaderSample = true;
        }

        public MatchEngagementReport Complete(int totalTicks)
        {
            if (totalTicks < 0) throw new ArgumentOutOfRangeException(nameof(totalTicks));
            if (_completed != null) return _completed;
            if (_lastObservedTick >= totalTicks && totalTicks > 0)
                throw new ArgumentException("totalTicks must include every observed tick.", nameof(totalTicks));

            var activityQuiet = BuildQuietSpans(totalTicks, _activityTicks);
            var spectacleQuiet = BuildQuietSpans(totalTicks, _spectacleTicks);
            _completed = new MatchEngagementReport(
                totalTicks,
                _tickSeconds,
                _activityTicks.Count,
                _spectacleTicks.Count,
                activityQuiet,
                spectacleQuiet);
            return _completed;
        }

        private void ObserveEvent(int tickIndex, MatchEvent e)
        {
            switch (e.Type)
            {
                case MatchEventType.TileStolen:
                    Add(tickIndex, EngagementBeatKind.Steal, e.PlayerId, e.Value, false);
                    break;
                case MatchEventType.Banked:
                    Add(tickIndex, EngagementBeatKind.BigBank, e.PlayerId, e.Value, e.Value >= 8);
                    break;
                case MatchEventType.ArrowUsed:
                    Add(tickIndex, EngagementBeatKind.Arrow, e.PlayerId, e.Value, true);
                    break;
                case MatchEventType.SpeedActivated:
                    Add(tickIndex, EngagementBeatKind.PowerPickup, e.PlayerId, e.Value, false);
                    break;
                case MatchEventType.MissileFired:
                    Add(tickIndex, EngagementBeatKind.MissileAttack, e.PlayerId, e.SecondaryPlayerId, true);
                    break;
                case MatchEventType.PlayerStunned:
                    Add(tickIndex, EngagementBeatKind.Stun, e.PlayerId, e.Value, true);
                    break;
                case MatchEventType.EnclosureCaptured:
                    Add(tickIndex, EngagementBeatKind.Enclosure, e.PlayerId, e.Value, e.Value >= 3);
                    break;
                case MatchEventType.HazardTelegraphed:
                    Add(tickIndex, EngagementBeatKind.HazardWarning, e.PlayerId, e.Value, true);
                    break;
                case MatchEventType.HazardDetonated:
                    Add(tickIndex, EngagementBeatKind.HazardBlast, e.PlayerId, e.Value, true);
                    break;
                case MatchEventType.TileProtected:
                case MatchEventType.PadlockActivated:
                    Add(tickIndex, EngagementBeatKind.Defense, e.PlayerId, e.Value, e.Type == MatchEventType.PadlockActivated);
                    break;
                case MatchEventType.CrateOpened:
                    Add(tickIndex, EngagementBeatKind.MysteryCrate, e.PlayerId, e.Value, true);
                    break;
            }
        }

        private void Add(int tickIndex, EngagementBeatKind kind, int playerId, int value, bool spectacle)
        {
            _activityTicks.Add(tickIndex);
            if (spectacle) _spectacleTicks.Add(tickIndex);
            _beats.Add(new EngagementBeat(tickIndex, kind, playerId, value, spectacle));
        }

        private static List<QuietSpan> BuildQuietSpans(int totalTicks, HashSet<int> beatTicks)
        {
            var spans = new List<QuietSpan>();
            var start = -1;
            for (var tick = 0; tick < totalTicks; tick++)
            {
                var quiet = !beatTicks.Contains(tick);
                if (quiet && start < 0) start = tick;
                if (!quiet && start >= 0)
                {
                    spans.Add(new QuietSpan(start, tick));
                    start = -1;
                }
            }
            if (start >= 0) spans.Add(new QuietSpan(start, totalTicks));
            return spans;
        }
    }
}
