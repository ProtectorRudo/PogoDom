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
                if (config.EnableMysteryCrates)
                    FillImmediately(state, PowerUpKind.MysteryCrate, config.TargetMysteryCrates, config, random, events);
                _primed = true;
                return;
            }

            ReplenishWithDelay(state, PowerUpKind.BankCrate, config.TargetBankCrates, config.BankRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Arrow, config.TargetArrows, config.ArrowRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Speed, config.TargetSpeedPickups, config.SpeedRespawnDelayTicks, config, random, events);
            ReplenishWithDelay(state, PowerUpKind.Missile, config.TargetMissiles, config.MissileRespawnDelayTicks, config, random, events);
            if (config.EnablePadlockPower)
                ReplenishWithDelay(state, PowerUpKind.Padlock, config.TargetPadlocks, config.PadlockRespawnDelayTicks, config, random, events);
            if (config.EnableMysteryCrates)
                ReplenishWithDelay(state, PowerUpKind.MysteryCrate, config.TargetMysteryCrates, config.MysteryCrateRespawnDelayTicks, config, random, events);
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
            var containedPower = kind == PowerUpKind.MysteryCrate
                ? MysteryCrateTable.Roll(config.MysteryCrateTableId, random)
                : PowerUpKind.None;

            var item = new ItemState(_nextItemId++, kind, pos, arrowDirection, containedPower);
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
            if (kind == PowerUpKind.MysteryCrate && config.BehaviorVersion == RulesetBehaviorVersion.V2)
                return TryFindFairMysteryCratePosition(state, config, random, out position);

            // V1 historical contract: rejection sampling first, row-major fallback.
            // Do not alter this branch; signed V1 replays depend on its RNG consumption.
            var attempts = Math.Max(64, state.Board.Count * 4);
            for (var i = 0; i < attempts; i++)
            {
                var candidate = new GridPos(random.NextInt(0, state.Board.Width), random.NextInt(0, state.Board.Height));
                if (!IsBaseSpawnCellFree(state, candidate)) continue;
                if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance)) continue;
                position = candidate;
                return true;
            }

            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    if (!IsBaseSpawnCellFree(state, candidate)) continue;
                    if (kind == PowerUpKind.BankCrate && TooCloseToBank(state, candidate, config.MinimumBankCrateChebyshevDistance)) continue;
                    position = candidate;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static bool TryFindFairMysteryCratePosition(MatchState state, MatchConfig config, IRandomSource random, out GridPos position)
        {
            // V2 never relaxes player safety. It first asks for a separated crate
            // that is contestable by at least two players, then relaxes contestability,
            // then crate-to-crate spacing only if the board is too congested.
            var candidates = new List<GridPos>();
            CollectMysteryCandidates(state, config, requireContestability: true, requireCrateSeparation: true, candidates);
            if (candidates.Count == 0)
                CollectMysteryCandidates(state, config, requireContestability: false, requireCrateSeparation: true, candidates);
            if (candidates.Count == 0)
                CollectMysteryCandidates(state, config, requireContestability: false, requireCrateSeparation: false, candidates);

            if (candidates.Count == 0)
            {
                position = default;
                return false;
            }

            position = candidates[random.NextInt(0, candidates.Count)];
            return true;
        }

        private static void CollectMysteryCandidates(
            MatchState state,
            MatchConfig config,
            bool requireContestability,
            bool requireCrateSeparation,
            List<GridPos> candidates)
        {
            candidates.Clear();
            for (var y = 0; y < state.Board.Height; y++)
            {
                for (var x = 0; x < state.Board.Width; x++)
                {
                    var candidate = new GridPos(x, y);
                    if (!IsBaseSpawnCellFree(state, candidate)) continue;
                    if (!IsSafeFromPlayers(state, candidate, config.MysteryCrateMinimumPlayerManhattanDistance)) continue;
                    if (requireCrateSeparation && !IsSeparatedFromMysteryCrates(state, candidate, config.MysteryCrateMinimumCrateChebyshevDistance)) continue;
                    if (requireContestability && !IsContestableByPlayers(state, candidate, config.MysteryCrateMaximumClosestPlayerDistanceGap)) continue;
                    candidates.Add(candidate);
                }
            }
        }

        private static bool IsBaseSpawnCellFree(MatchState state, GridPos candidate)
        {
            return state.ItemAt(candidate) == null && state.HazardAt(candidate) == null && !IsPlayerAt(state, candidate);
        }

        private static bool IsSafeFromPlayers(MatchState state, GridPos candidate, int minimumDistance)
        {
            var required = Math.Max(1, minimumDistance);
            for (var i = 0; i < state.Players.Count; i++)
            {
                if (Manhattan(state.Players[i].Position, candidate) < required)
                    return false;
            }
            return true;
        }

        private static bool IsSeparatedFromMysteryCrates(MatchState state, GridPos candidate, int minimumDistance)
        {
            var required = Math.Max(1, minimumDistance);
            for (var i = 0; i < state.Items.Count; i++)
            {
                var item = state.Items[i];
                if (item.Kind != PowerUpKind.MysteryCrate) continue;
                if (Chebyshev(item.Position, candidate) < required)
                    return false;
            }
            return true;
        }

        private static bool IsContestableByPlayers(MatchState state, GridPos candidate, int maximumClosestGap)
        {
            if (state.Players.Count < 2) return true;

            var closest = int.MaxValue;
            var second = int.MaxValue;
            for (var i = 0; i < state.Players.Count; i++)
            {
                var distance = Manhattan(state.Players[i].Position, candidate);
                if (distance < closest)
                {
                    second = closest;
                    closest = distance;
                }
                else if (distance < second)
                {
                    second = distance;
                }
            }

            return second == int.MaxValue || second - closest <= Math.Max(0, maximumClosestGap);
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
                if (Chebyshev(item.Position, candidate) < minimumDistance) return true;
            }
            return false;
        }

        private static int Manhattan(GridPos a, GridPos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static int Chebyshev(GridPos a, GridPos b)
        {
            return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
    }
}
