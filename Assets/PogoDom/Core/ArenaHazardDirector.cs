using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class ArenaHazardDirector
    {
        private int _nextId = 1;
        private int _nextSpawnTick = -1;

        public void Initialize(MatchState state, MatchConfig config)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            _nextSpawnTick = Math.Max(0, config.TntInitialDelayTicks);
        }

        // Existing hazards advance first. A hazard spawned this tick always receives the
        // full telegraph duration. This makes the warning window deterministic and fair.
        public void Update(MatchState state, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (!config.EnableArenaChaos) return;

            for (var i = state.Hazards.Count - 1; i >= 0; i--)
            {
                var hazard = state.Hazards[i];
                hazard.TicksRemaining--;
                if (hazard.TicksRemaining <= 0)
                {
                    Detonate(state, config, hazard, events);
                    state.Hazards.RemoveAt(i);
                }
            }

            if (_nextSpawnTick < 0)
                _nextSpawnTick = Math.Max(0, config.TntInitialDelayTicks);

            if (state.Tick < _nextSpawnTick || state.Hazards.Count >= config.MaxActiveTnt)
                return;

            GridPos position;
            if (TryPickSpawn(state, config, random, out position))
            {
                var hazard = new ArenaHazard(
                    _nextId++,
                    ArenaHazardKind.Tnt,
                    position,
                    config.TntBlastRadius,
                    config.TntFuseTicks);
                state.Hazards.Add(hazard);
                events.Add(new MatchEvent(
                    MatchEventType.HazardTelegraphed,
                    -1,
                    position,
                    config.TntFuseTicks,
                    PowerUpKind.Tnt));
            }

            _nextSpawnTick = state.Tick + Math.Max(1, config.TntSpawnIntervalTicks);
        }

        private static bool TryPickSpawn(MatchState state, MatchConfig config, IRandomSource random, out GridPos position)
        {
            var bestImpact = int.MinValue;
            var candidates = new List<GridPos>();

            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    if (!IsSpawnable(state, candidate)) continue;

                    var impact = CountOwnedTilesInBlast(state.Board, candidate, config.TntBlastRadius);
                    if (impact > bestImpact)
                    {
                        bestImpact = impact;
                        candidates.Clear();
                        candidates.Add(candidate);
                    }
                    else if (impact == bestImpact)
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                position = default;
                return false;
            }

            position = candidates[random.NextInt(0, candidates.Count)];
            return true;
        }

        private static bool IsSpawnable(MatchState state, GridPos position)
        {
            if (state.ItemAt(position) != null) return false;
            if (state.HazardAt(position) != null) return false;

            for (var i = 0; i < state.Players.Count; i++)
                if (state.Players[i].Position == position) return false;

            return true;
        }

        private static int CountOwnedTilesInBlast(BoardState board, GridPos center, int radius)
        {
            var count = 0;
            for (var y = center.Y - radius; y <= center.Y + radius; y++)
            {
                for (var x = center.X - radius; x <= center.X + radius; x++)
                {
                    var pos = new GridPos(x, y);
                    if (board.Contains(pos) && board.OwnerAt(pos) >= 0) count++;
                }
            }
            return count;
        }

        private static void Detonate(MatchState state, MatchConfig config, ArenaHazard hazard, List<MatchEvent> events)
        {
            var destroyed = 0;
            for (var y = hazard.Position.Y - hazard.BlastRadius; y <= hazard.Position.Y + hazard.BlastRadius; y++)
            {
                for (var x = hazard.Position.X - hazard.BlastRadius; x <= hazard.Position.X + hazard.BlastRadius; x++)
                {
                    var pos = new GridPos(x, y);
                    if (!state.Board.Contains(pos)) continue;
                    if (state.Board.OwnerAt(pos) == TileState.NeutralOwner) continue;
                    state.Board.SetOwner(pos, TileState.NeutralOwner);
                    destroyed++;
                }
            }

            events.Add(new MatchEvent(
                MatchEventType.HazardDetonated,
                -1,
                hazard.Position,
                destroyed,
                PowerUpKind.Tnt));

            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (!hazard.Contains(player.Position)) continue;
                player.StunTicksRemaining = Math.Max(player.StunTicksRemaining, config.TntStunTicks);
                events.Add(new MatchEvent(
                    MatchEventType.PlayerStunned,
                    player.Id,
                    player.Position,
                    config.TntStunTicks,
                    PowerUpKind.Tnt));
            }
        }
    }
}
