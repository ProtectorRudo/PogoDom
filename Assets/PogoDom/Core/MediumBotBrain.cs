using System;

namespace PogoDom.Core
{
    public sealed class MediumBotBrain : IBotBrain
    {
        private static readonly Direction[] Directions =
        {
            Direction.Up, Direction.Right, Direction.Down, Direction.Left
        };

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            var bestDirection = Direction.None;
            var bestScore = float.NegativeInfinity;
            var owned = state.Board.CountOwnedBy(bot.Id);
            var leader = MatchOutcome.Leader(state);
            var focus = PickFocusItem(state, bot, owned, config);

            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                var next = state.Board.Step(bot.Position, direction);
                if (next == bot.Position)
                    continue;

                var score = ScoreMove(state, bot, next, direction, owned, leader, focus, config, random);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = direction;
                }
            }

            return bestDirection == Direction.None ? bot.CurrentDirection : bestDirection;
        }

        private static float ScoreMove(
            MatchState state,
            PlayerState bot,
            GridPos next,
            Direction direction,
            int owned,
            PlayerState leader,
            ItemState focus,
            MatchConfig config,
            IRandomSource random)
        {
            var owner = state.Board.OwnerAt(next);
            var score = 0f;

            if (owner == TileState.NeutralOwner)
                score += 7f;
            else if (owner != bot.Id)
                score += 9f;
            else
                score += 0.8f;

            if (owner >= 0 && owner != bot.Id)
            {
                if (leader != null && owner == leader.Id)
                    score += 3.5f;
                if (bot.BotPersonality == BotPersonality.Aggressive)
                    score += 4f;
            }

            var item = state.ItemAt(next);
            if (item != null)
            {
                var itemValue = ItemValue(item.Kind, owned, config);
                if (bot.BotPersonality == BotPersonality.Greedy)
                    itemValue += 4f;
                if (bot.BotPersonality == BotPersonality.Banker && item.Kind == PowerUpKind.BankCrate)
                    itemValue += 5f;
                score += itemValue;
            }

            if (focus != null)
            {
                var before = Manhattan(bot.Position, focus.Position);
                var after = Manhattan(next, focus.Position);
                var focusWeight = bot.BotPersonality == BotPersonality.Greedy ? 2.7f : 1.8f;
                score += (before - after) * focusWeight;
            }

            // Occupied squares are legal candidates at the rules layer but usually waste a bounce.
            for (var i = 0; i < state.Players.Count; i++)
            {
                var other = state.Players[i];
                if (other.Id == bot.Id)
                    continue;
                if (other.Position == next)
                    score -= bot.BotPersonality == BotPersonality.Chaotic ? 5f : 12f;
            }

            if (direction == bot.CurrentDirection)
                score += 0.7f;
            if (IsReverse(bot.CurrentDirection, direction))
                score -= 0.45f;

            var centerX = (state.Board.Width - 1) * 0.5f;
            var centerY = (state.Board.Height - 1) * 0.5f;
            var centerDistance = Math.Abs(next.X - centerX) + Math.Abs(next.Y - centerY);
            score -= centerDistance * 0.05f;

            var noise = bot.BotPersonality == BotPersonality.Chaotic ? 4.5f : 1.1f;
            score += random.NextFloat01() * noise;
            return score;
        }

        private static ItemState PickFocusItem(MatchState state, PlayerState bot, int owned, MatchConfig config)
        {
            ItemState best = null;
            var bestUtility = float.NegativeInfinity;
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                var distance = Manhattan(bot.Position, item.Position);
                var utility = ItemValue(item.Kind, owned, config) - distance * 1.35f;
                if (bot.BotPersonality == BotPersonality.Greedy)
                    utility += 2.5f;
                if (bot.BotPersonality == BotPersonality.Banker && item.Kind == PowerUpKind.BankCrate)
                    utility += 5f;

                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    best = item;
                }
            }
            return best;
        }

        private static float ItemValue(PowerUpKind kind, int owned, MatchConfig config)
        {
            switch (kind)
            {
                case PowerUpKind.BankCrate:
                    return owned >= config.BankThresholdForBots ? 8f + owned * 1.8f : -1.5f;
                case PowerUpKind.Missile:
                    return 12f;
                case PowerUpKind.Arrow:
                    return 9.5f;
                case PowerUpKind.Speed:
                    return 8f;
                default:
                    return 0f;
            }
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
    }
}
