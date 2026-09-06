using System;

namespace PogoDom.Core
{
    /// <summary>
    /// Medium-strength entertainment brain used only by behavior V3+.
    /// It never receives hidden information or stat bonuses. It reacts to the
    /// same public board/score state a player can read: leader, territory,
    /// visible pickups, hazards and remaining time.
    ///
    /// The goal is not maximum win rate. Different personalities create
    /// recognizable pressure: Aggressive hunts the leader's territory, Greedy
    /// contests visible pickups, Banker protects/banks a good run and Balanced
    /// switches between those jobs. Small deterministic jitter preserves human
    /// mistakes and prevents three bots from tracing identical routes.
    /// </summary>
    public sealed class RivalBotBrain : IBotBrain
    {
        private static readonly Direction[] Directions =
        {
            Direction.Up, Direction.Right, Direction.Down, Direction.Left
        };

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (bot == null) throw new ArgumentNullException(nameof(bot));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var owned = state.Board.CountOwnedBy(bot.Id);
            var leader = MatchOutcome.Leader(state);
            var late = state.RemainingSeconds <= 15f;
            var focus = PickStrategicFocus(state, bot, owned, leader, config, late);

            var best = Direction.None;
            var bestScore = float.NegativeInfinity;
            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                var next = state.Board.Step(bot.Position, direction);
                if (next == bot.Position) continue;

                var score = ScoreMove(state, bot, next, direction, owned, leader, focus, config, late, random);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = direction;
                }
            }

            return best == Direction.None ? bot.CurrentDirection : best;
        }

        private static float ScoreMove(
            MatchState state,
            PlayerState bot,
            GridPos next,
            Direction direction,
            int owned,
            PlayerState leader,
            StrategicFocus focus,
            MatchConfig config,
            bool late,
            IRandomSource random)
        {
            var owner = state.Board.OwnerAt(next);
            var protectedByRival = IsProtectedByRival(state, bot, owner);
            var score = 0f;

            if (owner == TileState.NeutralOwner) score += 7.2f;
            else if (owner != bot.Id && !protectedByRival) score += 9.3f;
            else if (protectedByRival) score -= 5.5f;
            else score += 0.7f;

            if (owner >= 0 && owner != bot.Id && !protectedByRival)
            {
                if (leader != null && owner == leader.Id)
                    score += LeaderPressureBonus(bot, leader, late);
                if (bot.BotPersonality == BotPersonality.Aggressive)
                    score += 3.8f;
            }

            if (config.EnableEnclosureCapture && owner != bot.Id && !protectedByRival)
            {
                var enclosure = EnclosureResolver.PreviewCaptureCount(
                    state.Board,
                    bot.Id,
                    next,
                    config.EnclosureCapturePolicy);
                if (enclosure > 0)
                {
                    var multiplier = bot.BotPersonality == BotPersonality.Banker ? 4.0f : 3.3f;
                    score += Math.Min(30f, enclosure * multiplier);
                }
            }

            score += HazardSafetyScore(state, bot, next);
            score += OccupancyScore(state, bot, next);

            var item = state.ItemAt(next);
            if (item != null)
            {
                var itemValue = ItemValue(item.Kind, owned, bot, leader, state, config);
                score += itemValue;
            }

            if (focus.HasTarget)
            {
                var before = Manhattan(bot.Position, focus.Target);
                var after = Manhattan(next, focus.Target);
                score += (before - after) * focus.Weight;
            }

            // A leader with meaningful unbanked territory becomes a little more
            // conservative late. A trailer does the opposite and contests the
            // current leader. This changes decisions only; no score/stat rubber band.
            if (late)
            {
                if (leader != null && leader.Id == bot.Id && owned >= config.BankThresholdForBots)
                    score += MovesTowardNearestBank(state, bot.Position, next) * 3.4f;
                else if (leader != null && leader.Id != bot.Id)
                    score += MovesTowardLeaderTerritory(state, bot, leader, next) * 1.8f;
            }

            if (direction == bot.CurrentDirection) score += 0.65f;
            if (IsReverse(bot.CurrentDirection, direction)) score -= 0.5f;

            // Personality-specific imperfection. All randomness comes from the
            // match RNG, so signed replay remains deterministic.
            var jitter = bot.BotPersonality == BotPersonality.Chaotic ? 4.0f : 1.25f;
            if (bot.BotPersonality == BotPersonality.Banker) jitter = 0.85f;
            score += random.NextFloat01() * jitter;
            return score;
        }

        private static StrategicFocus PickStrategicFocus(
            MatchState state,
            PlayerState bot,
            int owned,
            PlayerState leader,
            MatchConfig config,
            bool late)
        {
            if (bot.BotPersonality == BotPersonality.Banker &&
                (owned >= config.BankThresholdForBots || (late && owned > 0)))
            {
                var bank = NearestItem(state, bot.Position, PowerUpKind.BankCrate);
                if (bank != null) return new StrategicFocus(bank.Position, 4.8f);
            }

            if (bot.BotPersonality == BotPersonality.Aggressive && leader != null && leader.Id != bot.Id)
            {
                GridPos target;
                if (TryNearestOwnedTile(state, bot.Position, leader.Id, out target))
                    return new StrategicFocus(target, late ? 4.1f : 3.2f);
                return new StrategicFocus(leader.Position, late ? 3.5f : 2.5f);
            }

            if (bot.BotPersonality == BotPersonality.Greedy)
            {
                var item = BestVisibleItem(state, bot, owned, leader, config);
                if (item != null) return new StrategicFocus(item.Position, 3.1f);
            }

            if (bot.BotPersonality == BotPersonality.Balanced)
            {
                if (leader != null && leader.Id != bot.Id && late)
                {
                    GridPos target;
                    if (TryNearestOwnedTile(state, bot.Position, leader.Id, out target))
                        return new StrategicFocus(target, 2.6f);
                }
                var item = BestVisibleItem(state, bot, owned, leader, config);
                if (item != null) return new StrategicFocus(item.Position, 2.0f);
            }

            if (bot.BotPersonality == BotPersonality.Chaotic)
            {
                var item = BestVisibleItem(state, bot, owned, leader, config);
                if (item != null) return new StrategicFocus(item.Position, 1.2f);
            }

            // Banker below threshold still values a useful visible pickup.
            var fallback = BestVisibleItem(state, bot, owned, leader, config);
            return fallback == null
                ? StrategicFocus.None
                : new StrategicFocus(fallback.Position, 1.7f);
        }

        private static ItemState BestVisibleItem(
            MatchState state,
            PlayerState bot,
            int owned,
            PlayerState leader,
            MatchConfig config)
        {
            ItemState best = null;
            var bestUtility = float.NegativeInfinity;
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                // Mystery payload is intentionally never inspected.
                var utility = ItemValue(item.Kind, owned, bot, leader, state, config)
                    - Manhattan(bot.Position, item.Position) * 1.25f;
                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    best = item;
                }
            }
            return best;
        }

        private static float ItemValue(
            PowerUpKind kind,
            int owned,
            PlayerState bot,
            PlayerState leader,
            MatchState state,
            MatchConfig config)
        {
            float value;
            switch (kind)
            {
                case PowerUpKind.BankCrate:
                    value = owned >= config.BankThresholdForBots ? 9f + owned * 1.7f : -1f;
                    if (state.RemainingSeconds <= 15f && owned > 0) value += 5f;
                    break;
                case PowerUpKind.MysteryCrate:
                    value = 10.5f;
                    break;
                case PowerUpKind.Missile:
                    value = leader != null && leader.Id != bot.Id ? 13f : 9f;
                    break;
                case PowerUpKind.Arrow:
                    value = 9.5f;
                    break;
                case PowerUpKind.Speed:
                    value = bot.HasSpeed ? 2f : 8.5f;
                    break;
                case PowerUpKind.Padlock:
                    value = bot.HasPadlock ? 1f : 7f + Math.Min(7f, owned * 0.65f);
                    break;
                default:
                    value = 0f;
                    break;
            }

            if (bot.BotPersonality == BotPersonality.Greedy) value += 3.5f;
            if (bot.BotPersonality == BotPersonality.Banker && kind == PowerUpKind.BankCrate) value += 4.5f;
            if (bot.BotPersonality == BotPersonality.Aggressive && kind == PowerUpKind.Missile) value += 3f;
            return value;
        }

        private static float LeaderPressureBonus(PlayerState bot, PlayerState leader, bool late)
        {
            if (leader == null || leader.Id == bot.Id) return 0f;
            var baseBonus = bot.BotPersonality == BotPersonality.Aggressive ? 5.5f : 2.5f;
            return late ? baseBonus + 2f : baseBonus;
        }

        private static float HazardSafetyScore(MatchState state, PlayerState bot, GridPos next)
        {
            var score = 0f;
            for (var i = 0; i < state.Hazards.Count; i++)
            {
                var hazard = state.Hazards[i];
                if (!hazard.Contains(next)) continue;
                if (hazard.TicksRemaining <= 1) score -= bot.BotPersonality == BotPersonality.Chaotic ? 10f : 42f;
                else if (hazard.TicksRemaining <= 2) score -= bot.BotPersonality == BotPersonality.Chaotic ? 4f : 18f;
                else score -= 2f;
            }
            return score;
        }

        private static float OccupancyScore(MatchState state, PlayerState bot, GridPos next)
        {
            for (var i = 0; i < state.Players.Count; i++)
            {
                if (state.Players[i].Id != bot.Id && state.Players[i].Position == next)
                    return bot.BotPersonality == BotPersonality.Chaotic ? -5f : -12f;
            }
            return 0f;
        }

        private static int MovesTowardNearestBank(MatchState state, GridPos from, GridPos next)
        {
            var bank = NearestItem(state, from, PowerUpKind.BankCrate);
            if (bank == null) return 0;
            return Manhattan(from, bank.Position) - Manhattan(next, bank.Position);
        }

        private static int MovesTowardLeaderTerritory(MatchState state, PlayerState bot, PlayerState leader, GridPos next)
        {
            if (leader == null || leader.Id == bot.Id) return 0;
            GridPos target;
            if (!TryNearestOwnedTile(state, bot.Position, leader.Id, out target))
                target = leader.Position;
            return Manhattan(bot.Position, target) - Manhattan(next, target);
        }

        private static bool TryNearestOwnedTile(MatchState state, GridPos from, int ownerId, out GridPos target)
        {
            var found = false;
            var bestDistance = int.MaxValue;
            target = default(GridPos);
            foreach (var position in state.Board.PositionsOwnedBy(ownerId))
            {
                var distance = Manhattan(from, position);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                target = position;
                found = true;
            }
            return found;
        }

        private static ItemState NearestItem(MatchState state, GridPos from, PowerUpKind kind)
        {
            ItemState best = null;
            var bestDistance = int.MaxValue;
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                if (item.Kind != kind) continue;
                var distance = Manhattan(from, item.Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = item;
                }
            }
            return best;
        }

        private static bool IsProtectedByRival(MatchState state, PlayerState bot, int owner)
        {
            if (owner < 0 || owner == bot.Id) return false;
            var defender = state.PlayerById(owner);
            return defender != null && defender.HasPadlock;
        }

        private static int Manhattan(GridPos a, GridPos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static bool IsReverse(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down) ||
                   (a == Direction.Down && b == Direction.Up) ||
                   (a == Direction.Left && b == Direction.Right) ||
                   (a == Direction.Right && b == Direction.Left);
        }

        private readonly struct StrategicFocus
        {
            public static StrategicFocus None => new StrategicFocus(default(GridPos), 0f, false);
            public GridPos Target { get; }
            public float Weight { get; }
            public bool HasTarget { get; }

            public StrategicFocus(GridPos target, float weight)
                : this(target, weight, true)
            {
            }

            private StrategicFocus(GridPos target, float weight, bool hasTarget)
            {
                Target = target;
                Weight = weight;
                HasTarget = hasTarget;
            }
        }
    }
}
