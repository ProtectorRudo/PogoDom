using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    /// <summary>
    /// Deterministic local-lookahead opponent. Distills useful observation/reward
    /// ideas from the audited Squares RL environment without a network/ML runtime.
    /// </summary>
    public sealed class HardBotBrain : IBotBrain
    {
        private static readonly Direction[] Directions = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        private const int SearchDepth = 3;
        private const float FutureDiscount = 0.58f;

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (bot == null) throw new ArgumentNullException(nameof(bot));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var owned = state.Board.CountOwnedBy(bot.Id);
            var leader = MatchOutcome.Leader(state);
            var bestDirection = Direction.None;
            var bestScore = float.NegativeInfinity;

            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                var next = state.Board.Step(bot.Position, direction);
                if (next == bot.Position) continue;

                var path = new HashSet<GridPos> { bot.Position, next };
                var score = ScoreNode(state, bot, bot.Position, next, direction, owned, leader, config, true);
                score += FutureDiscount * SearchFuture(state, bot, next, direction, owned, leader, config, SearchDepth - 1, path);
                if (score > bestScore + 0.0001f)
                {
                    bestScore = score;
                    bestDirection = direction;
                }
            }
            return bestDirection == Direction.None ? bot.CurrentDirection : bestDirection;
        }

        private static float SearchFuture(MatchState state, PlayerState bot, GridPos position, Direction incoming, int owned, PlayerState leader, MatchConfig config, int depth, HashSet<GridPos> path)
        {
            if (depth <= 0) return 0f;
            var best = float.NegativeInfinity;
            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                var next = state.Board.Step(position, direction);
                if (next == position) continue;
                var revisiting = path.Contains(next);
                var score = ScoreNode(state, bot, position, next, direction, owned, leader, config, false);
                if (revisiting) score -= 9f;
                var added = !revisiting && path.Add(next);
                score += FutureDiscount * SearchFuture(state, bot, next, direction, owned, leader, config, depth - 1, path);
                if (added) path.Remove(next);
                if (score > best) best = score;
            }
            return float.IsNegativeInfinity(best) ? 0f : best;
        }

        private static float ScoreNode(MatchState state, PlayerState bot, GridPos from, GridPos next, Direction direction, int owned, PlayerState leader, MatchConfig config, bool root)
        {
            var owner = state.Board.OwnerAt(next);
            var protectedByRival = IsProtectedByRival(state, bot, owner);
            var score = 0f;
            if (owner == TileState.NeutralOwner) score += 10f;
            else if (owner != bot.Id && !protectedByRival) score += 12.5f;
            else if (protectedByRival) score -= 8f;
            else score += 0.35f;

            if (owner >= 0 && owner != bot.Id && !protectedByRival && leader != null && owner == leader.Id) score += 4.5f;
            if (root && config.EnableEnclosureCapture && owner != bot.Id && !protectedByRival)
            {
                var enclosure = EnclosureResolver.PreviewCaptureCount(state.Board, bot.Id, next, config.EnclosureCapturePolicy);
                if (enclosure > 0) score += Math.Min(42f, enclosure * 4.4f);
            }

            score += HazardScore(state, next);
            score += OccupancyScore(state, bot, next);
            score += FrontierScore(state, bot, next);
            score += BankProgressScore(state, bot, from, next, owned, config);

            var item = state.ItemAt(next);
            if (item != null) score += ItemValue(item.Kind, owned, bot, leader, state, config);
            if (direction == bot.CurrentDirection) score += 0.9f;
            if (IsReverse(bot.CurrentDirection, direction)) score -= 0.75f;
            if (state.RemainingSeconds <= 15f && owned > 0) score += BankProgressScore(state, bot, from, next, owned + 3, config);
            return score;
        }

        private static float HazardScore(MatchState state, GridPos next)
        {
            var score = 0f;
            for (var i = 0; i < state.Hazards.Count; i++)
            {
                var hazard = state.Hazards[i];
                if (!hazard.Contains(next)) continue;
                if (hazard.TicksRemaining <= 1) score -= 90f;
                else if (hazard.TicksRemaining <= 2) score -= 42f;
                else score -= 12f;
            }
            return score;
        }

        private static float OccupancyScore(MatchState state, PlayerState bot, GridPos next)
        {
            for (var i = 0; i < state.Players.Count; i++)
                if (state.Players[i].Id != bot.Id && state.Players[i].Position == next) return -24f;
            return 0f;
        }

        private static float FrontierScore(MatchState state, PlayerState bot, GridPos next)
        {
            var frontier = 0;
            for (var i = 0; i < Directions.Length; i++)
            {
                var adjacent = state.Board.Step(next, Directions[i]);
                if (adjacent != next && state.Board.OwnerAt(adjacent) != bot.Id) frontier++;
            }
            return frontier * 0.8f;
        }

        private static float BankProgressScore(MatchState state, PlayerState bot, GridPos from, GridPos next, int owned, MatchConfig config)
        {
            if (owned <= 0) return 0f;
            ItemState nearest = null;
            var nearestDistance = int.MaxValue;
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                if (item.Kind != PowerUpKind.BankCrate) continue;
                var distance = Manhattan(from, item.Position);
                if (distance < nearestDistance) { nearest = item; nearestDistance = distance; }
            }
            if (nearest == null) return 0f;
            var before = Manhattan(from, nearest.Position);
            var after = Manhattan(next, nearest.Position);
            if (before == after) return 0f;
            var urgent = owned >= config.BankThresholdForBots;
            var weight = urgent ? 4.2f + Math.Min(4f, owned * 0.25f) : 0.65f;
            return (before - after) * weight;
        }

        private static float ItemValue(PowerUpKind kind, int owned, PlayerState bot, PlayerState leader, MatchState state, MatchConfig config)
        {
            switch (kind)
            {
                case PowerUpKind.BankCrate:
                {
                    var late = state.RemainingSeconds <= 15f;
                    if (owned >= config.BankThresholdForBots || late) return 18f + owned * 2.1f;
                    return owned > 0 ? 3f + owned * 0.7f : -1f;
                }
                case PowerUpKind.MysteryCrate: return 13f; // never inspect ContainedPower
                case PowerUpKind.Missile: return leader != null && leader.Id != bot.Id ? 17f : 11f;
                case PowerUpKind.Arrow: return 12f;
                case PowerUpKind.Speed: return bot.HasSpeed ? 2f : 11f;
                case PowerUpKind.Padlock: return bot.HasPadlock ? 1f : 8f + Math.Min(8f, owned * 0.6f);
                default: return 0f;
            }
        }

        private static bool IsProtectedByRival(MatchState state, PlayerState bot, int owner)
        {
            if (owner < 0 || owner == bot.Id) return false;
            var defender = state.PlayerById(owner);
            return defender != null && defender.HasPadlock;
        }

        private static int Manhattan(GridPos a, GridPos b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        private static bool IsReverse(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down) || (a == Direction.Down && b == Direction.Up) ||
                   (a == Direction.Left && b == Direction.Right) || (a == Direction.Right && b == Direction.Left);
        }
    }
}
