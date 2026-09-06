using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class MatchRunner
    {
        private readonly MatchConfig _config;
        private readonly IRandomSource _random;
        private readonly ItemSpawner _spawner;
        private readonly BotDirector _bots;
        private readonly ArenaHazardDirector _hazards;
        private bool _finishEventSent;

        public MatchRunner(MatchConfig config, IRandomSource random)
        {
            _config = config;
            _random = random;
            _spawner = new ItemSpawner();
            _bots = new BotDirector();
            _hazards = new ArenaHazardDirector();
        }

        public void Initialize(MatchState state)
        {
            var ignored = new List<MatchEvent>();
            _spawner.EnsurePopulation(state, _config, _random, ignored);
            _hazards.Initialize(state, _config);
        }

        public TickResult Tick(MatchState state, IReadOnlyDictionary<int, Direction> externalDirections = null)
        {
            var result = new TickResult();
            if (state.IsFinished)
            {
                EmitFinishOnce(result);
                return result;
            }

            _spawner.EnsurePopulation(state, _config, _random, result.Events);
            for (var i = 0; i < state.Players.Count; i++)
                PowerUpResolver.ApplyItemUnderPlayer(state, state.Players[i], _config, result.Events);

            PaintPlayers(state, state.Players, result.Events, _config);
            var desired = ResolveDirections(state, externalDirections);
            RunMovementPhase(state, desired, result, 1);

            var speedPlayers = new List<PlayerState>();
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (player.HasSpeed && !player.IsStunned) speedPlayers.Add(player);
            }

            if (speedPlayers.Count > 0)
            {
                for (var i = 0; i < speedPlayers.Count; i++)
                    PowerUpResolver.ApplyItemUnderPlayer(state, speedPlayers[i], _config, result.Events);
                PaintPlayers(state, speedPlayers, result.Events, _config);

                var speedDirections = new Dictionary<int, Direction>();
                for (var i = 0; i < state.Players.Count; i++) speedDirections[state.Players[i].Id] = Direction.None;
                for (var i = 0; i < speedPlayers.Count; i++) speedDirections[speedPlayers[i].Id] = speedPlayers[i].CurrentDirection;
                RunMovementPhase(state, speedDirections, result, 2);
            }

            if (_config.ArrowRotationIntervalTicks > 0 && (state.Tick + 1) % _config.ArrowRotationIntervalTicks == 0)
            {
                for (var i = 0; i < state.Items.Count; i++)
                {
                    var item = state.Items[i];
                    if (item.Kind == PowerUpKind.Arrow) item.ArrowDirection = ArrowResolver.RotateClockwise(item.ArrowDirection);
                }
            }

            _spawner.EnsurePopulation(state, _config, _random, result.Events);
            AdvanceStatusTimers(state);
            _hazards.Update(state, _config, _random, result.Events);

            state.Tick++;
            state.RemainingSeconds -= _config.TickSeconds;
            if (state.RemainingSeconds < 0f) state.RemainingSeconds = 0f;
            if (state.IsFinished) EmitFinishOnce(result);
            return result;
        }

        private Dictionary<int, Direction> ResolveDirections(MatchState state, IReadOnlyDictionary<int, Direction> externalDirections)
        {
            var desired = new Dictionary<int, Direction>(state.Players.Count);
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (player.IsStunned)
                {
                    desired[player.Id] = Direction.None;
                    continue;
                }

                Direction direction;
                if (player.IsHuman)
                {
                    Direction supplied;
                    direction = externalDirections != null && externalDirections.TryGetValue(player.Id, out supplied) ? supplied : player.CurrentDirection;
                }
                else direction = _bots.ChooseDirection(state, player, _config, _random);

                if (direction != Direction.None) player.CurrentDirection = direction;
                desired[player.Id] = direction;
            }
            return desired;
        }

        private void RunMovementPhase(MatchState state, IReadOnlyDictionary<int, Direction> directions, TickResult result, int phase)
        {
            var resolved = MovementResolver.Resolve(state.Board, state.Players, directions, _random);
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                Direction intent;
                var hasIntent = directions.TryGetValue(player.Id, out intent) && intent != Direction.None;
                if (!hasIntent) continue;

                var from = player.Position;
                var to = resolved[player.Id];
                if (!result.FromPositions.ContainsKey(player.Id)) result.FromPositions[player.Id] = from;
                player.Position = to;
                result.ToPositions[player.Id] = to;
                result.MovementSteps.Add(new MovementStep(player.Id, from, to, phase));
                result.Events.Add(new MatchEvent(from == to ? MatchEventType.PlayerBlocked : MatchEventType.PlayerMoved, player.Id, to, phase));
            }

            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (!result.ToPositions.ContainsKey(player.Id)) result.ToPositions[player.Id] = player.Position;
                if (!result.FromPositions.ContainsKey(player.Id)) result.FromPositions[player.Id] = player.Position;
            }
        }

        private static void PaintPlayers(MatchState state, IReadOnlyList<PlayerState> players, List<MatchEvent> events, MatchConfig config)
        {
            var anyDirectPaint = false;
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var previousOwner = state.Board.OwnerAt(player.Position);
                if (previousOwner == player.Id) continue;

                if (previousOwner >= 0)
                {
                    var defender = state.PlayerById(previousOwner);
                    if (defender != null && defender.HasPadlock)
                    {
                        events.Add(new MatchEvent(MatchEventType.TileProtected, player.Id, player.Position, 1, PowerUpKind.Padlock, defender.Id));
                        continue;
                    }
                }

                anyDirectPaint = true;
                state.Board.SetOwner(player.Position, player.Id);
                if (previousOwner >= 0)
                    events.Add(new MatchEvent(MatchEventType.TileStolen, player.Id, player.Position, 1, PowerUpKind.None, previousOwner));
                else
                    events.Add(new MatchEvent(MatchEventType.TilePainted, player.Id, player.Position, 1));
            }

            if (!config.EnableEnclosureCapture || !anyDirectPaint) return;
            var captures = EnclosureResolver.CaptureSimultaneous(state.Board, state.Players, config.EnclosureCapturePolicy);
            for (var i = 0; i < captures.Count; i++)
            {
                var capture = captures[i];
                if (capture.Count == 0) continue;
                var player = state.PlayerById(capture.PlayerId);
                events.Add(new MatchEvent(MatchEventType.EnclosureCaptured, capture.PlayerId, player == null ? default : player.Position, capture.Count));
            }
        }

        private static void AdvanceStatusTimers(MatchState state)
        {
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (player.StunTicksRemaining > 0) player.StunTicksRemaining--;
                if (player.SpeedTicksRemaining > 0) player.SpeedTicksRemaining--;
                if (player.PadlockTicksRemaining > 0) player.PadlockTicksRemaining--;
            }
        }

        private void EmitFinishOnce(TickResult result)
        {
            if (_finishEventSent) return;
            _finishEventSent = true;
            result.Events.Add(new MatchEvent(MatchEventType.MatchFinished));
        }
    }
}
