using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class ItemSpawner
    {
        private int _nextItemId = 1;

        public void EnsurePopulation(MatchState state, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            EnsureKind(state, PowerUpKind.BankCrate, config.TargetBankCrates, config, random, events);
            EnsureKind(state, PowerUpKind.Arrow, config.TargetArrows, config, random, events);
            EnsureKind(state, PowerUpKind.Speed, config.TargetSpeedPickups, config, random, events);
            EnsureKind(state, PowerUpKind.Missile, config.TargetMissiles, config, random, events);
        }

        private void EnsureKind(
            MatchState state,
            PowerUpKind kind,
            int target,
            MatchConfig config,
            IRandomSource random,
            List<MatchEvent> events)
        {
            var count = 0;
            for (var i = 0; i < state.Items.Count; i++)
                if (state.Items[i].Kind == kind) count++;

            while (count < target)
            {
                GridPos pos;
                if (!TryFindSpawnPosition(state, kind, config, random, out pos))
                    return;

                var arrowDirection = (Direction)random.NextInt((int)Direction.Up, (int)Direction.Left + 1);
                var item = new ItemState(_nextItemId++, kind, pos, arrowDirection);
                state.Items.Add(item);
                if (events != null)
                    events.Add(new MatchEvent(MatchEventType.ItemSpawned, position: pos, itemKind: kind));
                count++;
            }
        }

        private static bool TryFindSpawnPosition(
            MatchState state,
            PowerUpKind kind,
            MatchConfig config,
            IRandomSource random,
            out GridPos position)
        {
            var attempts = Math.Max(64, state.Board.Count * 4);
            for (var i = 0; i < attempts; i++)
            {
                var candidate = new GridPos(
                    random.NextInt(0, state.Board.Width),
                    random.NextInt(0, state.Board.Height));

                if (state.ItemAt(candidate) != null || IsPlayerAt(state, candidate))
                    continue;

                if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance))
                    continue;

                position = candidate;
                return true;
            }

            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    if (state.ItemAt(candidate) != null || IsPlayerAt(state, candidate))
                        continue;
                    if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance))
                        continue;
                    position = candidate;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static bool IsPlayerAt(MatchState state, GridPos position)
        {
            for (var i = 0; i < state.Players.Count; i++)
                if (state.Players[i].Position == position) return true;
            return false;
        }

        private static bool TooCloseToBank(MatchState state, GridPos candidate, int minimumDistance)
        {
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                if (item.Kind != PowerUpKind.BankCrate)
                    continue;

                var dx = Math.Abs(item.Position.X - candidate.X);
                var dy = Math.Abs(item.Position.Y - candidate.Y);
                var chebyshev = Math.Max(dx, dy);
                if (chebyshev < minimumDistance)
                    return true;
            }
            return false;
        }
    }
}
