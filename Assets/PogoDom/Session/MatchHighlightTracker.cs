using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Session
{
    public enum HighlightKind
    {
        BigBank = 0,
        LeaderMissile = 1,
        LateLeadChange = 2,
        PhotoFinish = 3,
        ComebackWin = 4,
        AreaCapture = 5,
        ArenaBlast = 6
    }

    public sealed class HighlightMoment
    {
        public HighlightKind Kind { get; }
        public int PlayerId { get; }
        public int Value { get; }
        public int ViralityScore { get; }

        public HighlightMoment(HighlightKind kind, int playerId, int value, int viralityScore)
        {
            Kind = kind;
            PlayerId = playerId;
            Value = value;
            ViralityScore = viralityScore;
        }
    }

    public sealed class MatchHighlightTracker
    {
        private readonly int _localPlayerId;
        private readonly List<HighlightMoment> _moments = new List<HighlightMoment>();
        private int _previousLeaderId = -1;
        private int _leaderAtLateWindow = -1;
        private bool _enteredLateWindow;
        private bool _completed;

        public MatchHighlightTracker(int localPlayerId)
        {
            _localPlayerId = localPlayerId;
        }

        public IReadOnlyList<HighlightMoment> Moments => _moments;

        public void Observe(MatchState state, TickResult tick)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            if (_completed) throw new InvalidOperationException("Highlight tracker is complete.");

            for (var i = 0; i < tick.Events.Count; i++)
            {
                var e = tick.Events[i];

                if (e.Type == MatchEventType.HazardDetonated && e.Value >= 5)
                    AddOnce(HighlightKind.ArenaBlast, -1, e.Value, Math.Min(100, 65 + e.Value * 4));

                if (e.PlayerId != _localPlayerId) continue;

                if (e.Type == MatchEventType.Banked && e.Value >= 8)
                    AddOnce(HighlightKind.BigBank, _localPlayerId, e.Value, Math.Min(100, 50 + e.Value * 3));
                else if (e.Type == MatchEventType.MissileFired)
                    AddOnce(HighlightKind.LeaderMissile, _localPlayerId, e.SecondaryPlayerId, 72);
                else if (e.Type == MatchEventType.EnclosureCaptured && e.Value >= 3)
                    AddOnce(HighlightKind.AreaCapture, _localPlayerId, e.Value, Math.Min(100, 68 + e.Value * 4));
            }

            var leader = MatchOutcome.Leader(state);
            var leaderId = leader == null ? -1 : leader.Id;

            if (!_enteredLateWindow && state.RemainingSeconds <= 15f)
            {
                _enteredLateWindow = true;
                _leaderAtLateWindow = leaderId;
            }

            if (_enteredLateWindow && _previousLeaderId >= 0 && leaderId >= 0 && leaderId != _previousLeaderId)
                AddOnce(HighlightKind.LateLeadChange, leaderId, 0, 85);

            _previousLeaderId = leaderId;
        }

        public IReadOnlyList<HighlightMoment> Complete(MatchState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_completed) return _moments;
            _completed = true;

            var standings = MatchOutcome.Standings(state);
            if (standings.Count >= 2)
            {
                var margin = standings[0].Score - standings[1].Score;
                if (margin <= 5)
                    AddOnce(HighlightKind.PhotoFinish, standings[0].PlayerId, margin, 92);

                if (standings[0].PlayerId == _localPlayerId && _enteredLateWindow && _leaderAtLateWindow >= 0 && _leaderAtLateWindow != _localPlayerId)
                    AddOnce(HighlightKind.ComebackWin, _localPlayerId, margin, 100);
            }

            _moments.Sort((a, b) => b.ViralityScore.CompareTo(a.ViralityScore));
            return _moments;
        }

        public HighlightMoment Best()
        {
            HighlightMoment best = null;
            for (var i = 0; i < _moments.Count; i++)
                if (best == null || _moments[i].ViralityScore > best.ViralityScore) best = _moments[i];
            return best;
        }

        private void AddOnce(HighlightKind kind, int playerId, int value, int score)
        {
            for (var i = 0; i < _moments.Count; i++)
                if (_moments[i].Kind == kind) return;
            _moments.Add(new HighlightMoment(kind, playerId, value, score));
        }
    }
}
