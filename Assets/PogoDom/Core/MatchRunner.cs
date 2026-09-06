using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class MatchRunner
    {
        private readonly MatchConfig _config;
        private readonly IRandomSource _random;
        private readonly ItemSpawner _spawner;
        private readonly EasyBotBrain _easyBot;
        private bool _finishEventSent;

        public MatchRunner(MatchConfig config, IRandomSource random)
        {
            _config = config;
            _random = random;
            _spawner = new ItemSpawner();
            _easyBot = new EasyBotBrain();
        }

        public void Initialize(MatchState state)
        {
            var ignored = new List<MatchEvent>();
            _spawner.EnsurePopulation(state, _config, _random, ignored);
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

            // 1) Effects on the square where the player landed during the previous tick.
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                ApplyItemUnderPlayer(state, player, result.Events);
            }

            // 2) Paint current positions. This deliberately happens AFTER banking.
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                if (state.Board.OwnerAt(player.Position) != player.Id)
                {
                    state.Board.SetOwner(player.Position, player.Id);
                    result.Events.Add(new MatchEvent(MatchEventType.TilePainted, player.Id, player.Position));
                }
            }

            // 3) Resolve desired directions.
            var desired = new Dictionary<int, Direction>(state.Players.Count);
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                result.FromPositions[player.Id] = player.Position;

                if (player.IsStunned)
                {
                    player.StunTicksRemaining--;
                    desired[player.Id] = Direction.None;
                    continue;
                }

                Direction direction;
                if (player.IsHuman)
                {
                    direction = externalDirections != null && externalDirections.TryGetValue(player.Id, out var supplied)
                        ? supplied
                        : player.CurrentDirection;
                }
                else
                {
                    direction = _easyBot.ChooseDirection(state, player, _config, _random);
                }

                if (direction != Direction.None)
                    player.CurrentDirection = direction;
                desired[player.Id] = direction;
            }

            // 4) Simultaneous movement.
            var resolved = MovementResolver.Resolve(state.Board, state.Players, desired, _random);
            for (var i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i];
                var from = player.Position;
                var to = resolved[player.Id];
                player.Position = to;
                result.ToPositions[player.Id] = to;

                result.Events.Add(new MatchEvent(
                    from == to ? MatchEventType.PlayerBlocked : MatchEventType.PlayerMoved,
                    player.Id,
                    to));
            }

            // 5) Remaining arrows rotate periodically.
            if (_config.ArrowRotationIntervalTicks > 0 && (state.Tick + 1) % _config.ArrowRotationIntervalTicks == 0)
            {
                for (var i = 0; i < state.Items.Count; i++)
                {
                    var item = state.Items[i];
                    if (item.Kind == PowerUpKind.Arrow)
                        item.ArrowDirection = ArrowResolver.RotateClockwise(item.ArrowDirection);
                }
            }

            // 6) Immediately refill consumed slots; newly spawned items cannot trigger until next tick.
            _spawner.EnsurePopulation(state, _config, _random, result.Events);

            state.Tick++;
            state.RemainingSeconds -= _config.TickSeconds;
            if (state.RemainingSeconds < 0f)
                state.RemainingSeconds = 0f;

            if (state.IsFinished)
                EmitFinishOnce(result);

            return result;
        }

        private static void RemoveItem(MatchState state, ItemState item)
        {
            state.Items.Remove(item);
        }

        private void ApplyItemUnderPlayer(MatchState state, PlayerState player, List<MatchEvent> events)
        {
            var item = state.ItemAt(player.Position);
            if (item == null)
                return;

            switch (item.Kind)
            {
                case PowerUpKind.BankCrate:
                {
                    var banked = BankingResolver.Bank(state, player);
                    events.Add(new MatchEvent(MatchEventType.Banked, player.Id, player.Position, banked, item.Kind));
                    RemoveItem(state, item);
                    events.Add(new MatchEvent(MatchEventType.ItemConsumed, player.Id, player.Position, banked, item.Kind));
                    break;
                }
                case PowerUpKind.Arrow:
                {
                    var painted = ArrowResolver.Apply(state, player, item.ArrowDirection);
                    events.Add(new MatchEvent(MatchEventType.ArrowUsed, player.Id, player.Position, painted, item.Kind));
                    RemoveItem(state, item);
                    events.Add(new MatchEvent(MatchEventType.ItemConsumed, player.Id, player.Position, painted, item.Kind));
                    break;
                }
            }
        }

        private void EmitFinishOnce(TickResult result)
        {
            if (_finishEventSent)
                return;
            _finishEventSent = true;
            result.Events.Add(new MatchEvent(MatchEventType.MatchFinished));
        }
    }
}
