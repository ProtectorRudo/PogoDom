using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class EasyBotBrain
    {
        private static readonly Direction[] Directions =
        {
            Direction.Up, Direction.Right, Direction.Down, Direction.Left
        };

        public Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random)
        {
            var owned = state.Board.CountOwnedBy(bot.Id);
            var targetKind = owned >= config.BankThresholdForBots ? PowerUpKind.BankCrate : PowerUpKind.Arrow;
            var target = FindNearestItem(state, bot.Position, targetKind);

            if (target == null)
                target = FindNearestItem(state, bot.Position, PowerUpKind.BankCrate);

            var occupied = new HashSet<GridPos>();
            for (var i = 0; i < state.Players.Count; i++)
            {
                if (state.Players[i].Id != bot.Id)
                    occupied.Add(state.Players[i].Position);
            }

            var best = bot.CurrentDirection;
            var bestScore = float.NegativeInfinity;

            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                var next = state.Board.Step(bot.Position, direction);
                if (next == bot.Position || occupied.Contains(next))
                    continue;

                var score = 0f;
                if (state.Board.OwnerAt(next) != bot.Id) score += 2.5f;
                if (state.ItemAt(next) != null) score += 4f;
                if (direction == bot.CurrentDirection) score += 0.35f;

                if (target != null)
                {
                    var before = Manhattan(bot.Position, target.Position);
                    var after = Manhattan(next, target.Position);
                    score += (before - after) * 3f;
                }

                score += random.NextFloat01() * 0.2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = direction;
                }
            }

            return best;
        }

        private static ItemState FindNearestItem(MatchState state, GridPos from, PowerUpKind kind)
        {
            ItemState best = null;
            var bestDistance = int.MaxValue;
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                if (item.Kind != kind)
                    continue;

                var distance = Manhattan(from, item.Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = item;
                }
            }
            return best;
        }

        private static int Manhattan(GridPos a, GridPos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
