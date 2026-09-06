using System;
using PogoDom.Core;

namespace PogoDom.Meta
{
    public sealed class BattleSessionRecorder
    {
        private readonly string _matchId;
        private readonly int _localPlayerId;
        private readonly CityId _cityId;
        private readonly NationId _nationId;
        private int _painted;
        private int _stolen;
        private int _bankedPoints;
        private int _banks;
        private int _arrows;
        private int _speeds;
        private int _missiles;
        private bool _completed;

        public BattleSessionRecorder(string matchId, int localPlayerId, CityId cityId, NationId nationId)
        {
            if (string.IsNullOrWhiteSpace(matchId)) throw new ArgumentException("Match id cannot be empty.", nameof(matchId));
            _matchId = matchId;
            _localPlayerId = localPlayerId;
            _cityId = cityId;
            _nationId = nationId;
        }

        public void Observe(TickResult tick)
        {
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            if (_completed) throw new InvalidOperationException("Recorder is already completed.");

            for (var i = 0; i < tick.Events.Count; i++)
            {
                var e = tick.Events[i];
                if (e.PlayerId != _localPlayerId) continue;

                switch (e.Type)
                {
                    case MatchEventType.TilePainted: _painted += Math.Max(1, e.Value); break;
                    case MatchEventType.TileStolen: _stolen += Math.Max(1, e.Value); break;
                    case MatchEventType.Banked:
                        _banks++;
                        _bankedPoints += Math.Max(0, e.Value);
                        break;
                    case MatchEventType.ArrowUsed: _arrows++; break;
                    case MatchEventType.SpeedActivated: _speeds++; break;
                    case MatchEventType.MissileFired: _missiles++; break;
                }
            }
        }

        public BattleResult Complete(MatchState state, bool isValid = true)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_completed) throw new InvalidOperationException("Recorder is already completed.");
            _completed = true;

            var local = state.PlayerById(_localPlayerId);
            if (local == null) throw new InvalidOperationException("Local player does not exist in match state.");

            var standings = MatchOutcome.Standings(state);
            var placement = -1;
            for (var i = 0; i < standings.Count; i++)
            {
                if (standings[i].PlayerId == _localPlayerId)
                {
                    placement = i + 1;
                    break;
                }
            }
            if (placement < 1) throw new InvalidOperationException("Local player has no final standing.");

            return new BattleResult(
                _matchId,
                local.IsHuman,
                isValid,
                _cityId,
                _nationId,
                placement == 1,
                placement,
                local.Score,
                _painted,
                _stolen,
                _bankedPoints,
                _banks,
                _arrows,
                _speeds,
                _missiles);
        }
    }
}
