using System;
using System.Collections.Generic;
using PogoDom.Core;

namespace PogoDom.Session
{
    public enum CoachHint
    {
        None = 0,
        SwipeToTurn = 1,
        PaintAndSteal = 2,
        BankToScore = 3
    }

    public readonly struct CoachDirective
    {
        public CoachHint Hint { get; }
        public bool IsBlocking { get; }

        public CoachDirective(CoachHint hint)
        {
            Hint = hint;
            IsBlocking = false;
        }
    }

    public sealed class FirstRunCoach
    {
        public const float MaximumCoachSeconds = 20f;

        private readonly int _localPlayerId;
        private float _elapsed;
        private bool _moved;
        private bool _painted;
        private bool _banked;

        public FirstRunCoach(int localPlayerId)
        {
            _localPlayerId = localPlayerId;
        }

        public bool IsComplete => _banked || _elapsed >= MaximumCoachSeconds;
        public float ElapsedSeconds => _elapsed;

        public CoachDirective Current
        {
            get
            {
                if (IsComplete) return new CoachDirective(CoachHint.None);
                if (!_moved) return new CoachDirective(CoachHint.SwipeToTurn);
                if (!_painted) return new CoachDirective(CoachHint.PaintAndSteal);
                return new CoachDirective(CoachHint.BankToScore);
            }
        }

        public void Observe(TickResult tick, float deltaSeconds)
        {
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            ObserveEvents(tick.Events, deltaSeconds);
        }

        public void ObserveEvents(IReadOnlyList<MatchEvent> events, float deltaSeconds)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            _elapsed += deltaSeconds;

            for (var i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.PlayerId != _localPlayerId) continue;

                if (e.Type == MatchEventType.PlayerMoved) _moved = true;
                if (e.Type == MatchEventType.TilePainted || e.Type == MatchEventType.TileStolen) _painted = true;
                if (e.Type == MatchEventType.Banked) _banked = true;
            }
        }

        public void AdvanceWithoutTick(float deltaSeconds)
        {
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            _elapsed += deltaSeconds;
        }
    }
}
