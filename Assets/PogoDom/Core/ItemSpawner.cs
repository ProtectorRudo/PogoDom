using System;
using System.Collections.Generic;

namespace PogoDom.Core
{
    public sealed class ItemSpawner
    {
        private readonly Dictionary<PowerUpKind, int> _nextSpawnTick = new Dictionary<PowerUpKind, int>();
        private int _nextItemId = 1;
        private bool _primed;

        public void EnsurePopulation(MatchState state, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            if (!_primed)
            {
                FillImmediately(state, PowerUpKind.BankCrate, config.TargetBankCrates, config, random, events);
                FillImmediately(state, PowerUpKind.Arrow, config.TargetArrows, config, random, events);
                FillImmediately(state, PowerUpKind.Speed, config.TargetSpeedPickups, config, random, events);
                FillImmediately(state, PowerUpKind.Missile, config.TargetMissiles, config, random, events);
                if (config.EnablePadlockPower)
                    FillImmediately(state, PowerUpKind.Padlock, config.TargetPadlocks, config, random, events);
                _primed = true;
                return;
            }

            ReplenishWithDelay(state, PowerUpKind.BankCrate, config.TargetBankCrates, config.BankRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Arrow, config.TargetArrows, config.ArrowRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Speed, config.TargetSpeedPickups, config.SpeedRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Missile, config.TargetMissiles, config.MissileRespawnDelayTicks, config, random, events);
            if (config.EnablePadlockPower)
                ReplenishWithDelay(state, PowerUpKind.Padlock, config.TargetPadlocks, config.PadlockRespawnDelayTicks, config, random, events);
        }

        private void FillImmediately(MatchState state, PowerUpKind kind, int target, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            var count = Count(state, kind);
            while (count < target)
            {
                if (!TrySpawnOne(state, kind, config, random, events)) return;
                count++;
            }
        }

        private void ReplenishWithDelay(MatchState state, PowerUpKind kind, int target, int delayTicks, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            if (target <= 0) return;
            var count = Count(state, kind);
            if (count >= target)
            {
                _nextSpawnTick.Remove(kind);
                return;
            }

            int dueTick;
            if (!_nextSpawnTick.TryGetValue(kind, out dueTick))
            {
                _nextSpawnTick[kind] = state.Tick + Math.Max(1, delayTicks);
                return;
            }
            if (state.Tick < dueTick) return;

            if (TrySpawnOne(state, kind, config, random, events))
            {
                count++;
                if (count < target) _nextSpawnTick[kind] = state.Tick + Math.Max(1, delayTicks);
                else _nextSpawnTick.Remove(kind);
            }
            else
            {
                _nextSpawnTick[kind] = state.Tick + 1;
            }
        }

        private bool TrySpawnOne(MatchState state, PowerUpKind kind, MatchConfig config, IRandomSource random, List<MatchEvent> events)
        {
            GridPos pos;
            if (!TryFindSpawnPosition(state, kind, config, random, out pos)) return false;

            var arrowDirection = (Direction)random.NextInt((int)Direction.Up, (int)Direction.Left + 1);
            var item = new ItemState(_nextItemId++, kind, pos, arrowDirection);
            state.Items.Add(item);
            if (events != null)
                events.Add(new MatchEvent(MatchEventType.ItemSpawned, position: pos, itemKind: kind));
            return true;
        }

        private static int Count(MatchState state, PowerUpKind kind)
        {
            var count = 0;
            for (var i = 0; i < state.Items.Count; i++)
                if (state.Items[i].Kind == kind) count++;
            return count;
        }

        private static bool TryFindSpawnPosition(MatchState state, PowerUpKind kind, MatchConfig config, IRandomSource random, out GridPos position)
        {
            var attempts = Math.Max(64, state.Board.Count * 4);
            for (var i = 0; i < attempts; i++)
            {
                var candidate = new GridPos(random.NextInt(0, state.Board.Width), random.NextInt(0, state.Board.Height));
                if (state.ItemAt(candidate) != null || state.HazardAt(candidate) != null || IsPlayerAt(state, candidate)) continue;
                if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance)) continue;
                position = candidate;
                return true;
            }

            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    if (state.ItemAt(candidate) != null || state.HazardAt(candidate) != null || IsPlayerAt(state, candidate)) continue;
                    if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance)) continue;
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
                if (item.Kind != PowerUpKind.BankCrate) continue;
                var dx = Math.Abs(item.Position.X - candidate.X);
                var dy = Math.Abs(item.Position.Y - candidate.Y);
                if (Math.Max(dx, dy) < minimumDistance) return true;
            }
            return false;
        }
    }
}
