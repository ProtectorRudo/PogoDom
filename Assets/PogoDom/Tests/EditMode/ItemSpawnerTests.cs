using System;
using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class ItemSpawnerTests
    {
        [Test]
        public void InitialPopulationIsImmediateButReplacementWaitsForCooldown()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 1,
                MissileRespawnDelayTicks = 5
            };
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState>(), 60f);
            var spawner = new ItemSpawner();
            var random = new XorShiftRandom(12);
            var events = new List<MatchEvent>();

            spawner.EnsurePopulation(state, config, random, events);
            Assert.AreEqual(1, state.Items.Count);

            state.Items.Clear();
            spawner.EnsurePopulation(state, config, random, events);
            Assert.AreEqual(0, state.Items.Count, "Replacement should be scheduled, not immediate.");

            state.Tick = 4;
            spawner.EnsurePopulation(state, config, random, events);
            Assert.AreEqual(0, state.Items.Count);

            state.Tick = 5;
            spawner.EnsurePopulation(state, config, random, events);
            Assert.AreEqual(1, state.Items.Count);
        }

        [Test]
        public void V2MysteryCratesSpawnAwayFromPlayersSeparatedAndContestable()
        {
            var config = CrateOnlyConfig(RulesetBehaviorVersion.V2);
            var state = CornerPlayers();
            var spawner = new ItemSpawner();
            spawner.EnsurePopulation(state, config, new XorShiftRandom(20260906), new List<MatchEvent>());

            Assert.AreEqual(2, CountMysteryCrates(state));
            AssertFairCrates(state, config);
        }

        [Test]
        public void V2FairnessInvariantsHoldAcrossTwoHundredSeeds()
        {
            for (uint seed = 1; seed <= 200; seed++)
            {
                var config = CrateOnlyConfig(RulesetBehaviorVersion.V2);
                var state = CornerPlayers();
                var spawner = new ItemSpawner();
                spawner.EnsurePopulation(state, config, new XorShiftRandom(seed), new List<MatchEvent>());
                Assert.AreEqual(2, CountMysteryCrates(state), "seed=" + seed);
                AssertFairCrates(state, config);
            }
        }

        private static MatchConfig CrateOnlyConfig(RulesetBehaviorVersion behavior)
        {
            return new MatchConfig
            {
                BehaviorVersion = behavior,
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                EnablePadlockPower = true,
                TargetPadlocks = 0,
                EnableMysteryCrates = true,
                TargetMysteryCrates = 2,
                MysteryCrateMinimumPlayerManhattanDistance = 2,
                MysteryCrateMinimumCrateChebyshevDistance = 2,
                MysteryCrateMaximumClosestPlayerDistanceGap = 1
            };
        }

        private static MatchState CornerPlayers()
        {
            return new MatchState(
                new BoardState(8, 8),
                new List<PlayerState>
                {
                    new PlayerState(0, "P1", true, new GridPos(0, 0), Direction.Up),
                    new PlayerState(1, "P2", false, new GridPos(0, 7), Direction.Right),
                    new PlayerState(2, "P3", false, new GridPos(7, 7), Direction.Down),
                    new PlayerState(3, "P4", false, new GridPos(7, 0), Direction.Left)
                },
                75f);
        }

        private static void AssertFairCrates(MatchState state, MatchConfig config)
        {
            var crates = new List<ItemState>();
            for (var i = 0; i < state.Items.Count; i++)
                if (state.Items[i].Kind == PowerUpKind.MysteryCrate) crates.Add(state.Items[i]);

            for (var i = 0; i < crates.Count; i++)
            {
                var nearest = int.MaxValue;
                var second = int.MaxValue;
                for (var p = 0; p < state.Players.Count; p++)
                {
                    var distance = Manhattan(crates[i].Position, state.Players[p].Position);
                    Assert.GreaterOrEqual(distance, config.MysteryCrateMinimumPlayerManhattanDistance);
                    if (distance < nearest) { second = nearest; nearest = distance; }
                    else if (distance < second) second = distance;
                }
                Assert.LessOrEqual(second - nearest, config.MysteryCrateMaximumClosestPlayerDistanceGap);

                for (var j = i + 1; j < crates.Count; j++)
                    Assert.GreaterOrEqual(Chebyshev(crates[i].Position, crates[j].Position), config.MysteryCrateMinimumCrateChebyshevDistance);
            }
        }

        private static int CountMysteryCrates(MatchState state)
        {
            var count = 0;
            for (var i = 0; i < state.Items.Count; i++) if (state.Items[i].Kind == PowerUpKind.MysteryCrate) count++;
            return count;
        }

        private static int Manhattan(GridPos a, GridPos b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        private static int Chebyshev(GridPos a, GridPos b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
