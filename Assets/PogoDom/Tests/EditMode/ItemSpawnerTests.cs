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
    }
}
