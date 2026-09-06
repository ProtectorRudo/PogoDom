using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class ArenaHazardTests
    {
        [Test]
        public void TntWaitsForInitialDelayAndProvidesFullTelegraph()
        {
            var config = new MatchConfig
            {
                EnableArenaChaos = true,
                TntInitialDelayTicks = 2,
                TntFuseTicks = 4,
                MaxActiveTnt = 1,
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0
            };
            var state = MatchFactory.CreateBotLab(config);
            var director = new ArenaHazardDirector();
            director.Initialize(state, config);
            var events = new List<MatchEvent>();
            var rng = new XorShiftRandom(7);

            director.Update(state, config, rng, events);
            Assert.AreEqual(0, state.Hazards.Count);
            state.Tick = 1;
            director.Update(state, config, rng, events);
            Assert.AreEqual(0, state.Hazards.Count);
            state.Tick = 2;
            director.Update(state, config, rng, events);

            Assert.AreEqual(1, state.Hazards.Count);
            Assert.AreEqual(4, state.Hazards[0].TicksRemaining);
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.HazardTelegraphed));
        }

        [Test]
        public void DetonationClearsOnlyUnbankedTerritoryAndStunsCaughtPlayer()
        {
            var config = new MatchConfig
            {
                EnableArenaChaos = true,
                TntInitialDelayTicks = 999,
                TntBlastRadius = 1,
                TntStunTicks = 1
            };
            var player = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.Right);
            var state = new MatchState(new BoardState(5, 5), new List<PlayerState> { player }, 30f);
            player.Score = 12;
            for (var y = 1; y <= 3; y++)
                for (var x = 1; x <= 3; x++)
                    state.Board.SetOwner(new GridPos(x, y), 0);

            state.Hazards.Add(new ArenaHazard(1, ArenaHazardKind.Tnt, new GridPos(2, 2), 1, 1));
            var director = new ArenaHazardDirector();
            director.Initialize(state, config);
            var events = new List<MatchEvent>();
            director.Update(state, config, new XorShiftRandom(1), events);

            Assert.AreEqual(0, state.Hazards.Count);
            Assert.AreEqual(12, player.Score, "Confirmed/banked score must never be destroyed by arena chaos.");
            Assert.AreEqual(1, player.StunTicksRemaining);
            for (var y = 1; y <= 3; y++)
                for (var x = 1; x <= 3; x++)
                    Assert.AreEqual(TileState.NeutralOwner, state.Board.OwnerAt(new GridPos(x, y)));
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.HazardDetonated && e.Value == 9));
        }

        [Test]
        public void SameSeedChoosesSameHighImpactWarningCell()
        {
            var config = new MatchConfig { EnableArenaChaos = true, TntInitialDelayTicks = 0 };
            var a = MatchFactory.CreateBotLab(config);
            var b = MatchFactory.CreateBotLab(config);
            for (var x = 1; x <= 5; x++)
            {
                a.Board.SetOwner(new GridPos(x, 3), 0);
                b.Board.SetOwner(new GridPos(x, 3), 0);
            }

            var da = new ArenaHazardDirector();
            var db = new ArenaHazardDirector();
            da.Initialize(a, config);
            db.Initialize(b, config);
            da.Update(a, config, new XorShiftRandom(77), new List<MatchEvent>());
            db.Update(b, config, new XorShiftRandom(77), new List<MatchEvent>());

            Assert.AreEqual(1, a.Hazards.Count);
            Assert.AreEqual(a.Hazards[0].Position, b.Hazards[0].Position);
        }

        [Test]
        public void OneHundredChaosMatchesFinishWithoutInvalidPositions()
        {
            var detonations = 0;
            for (uint seed = 1; seed <= 100; seed++)
            {
                var config = new MatchConfig
                {
                    MatchSeconds = 30f,
                    EnableArenaChaos = true,
                    TntInitialDelayTicks = 4,
                    TntSpawnIntervalTicks = 8,
                    TntFuseTicks = 3
                };
                var state = MatchFactory.CreateBotLab(config);
                var runner = new MatchRunner(config, new XorShiftRandom(seed));
                runner.Initialize(state);
                var safety = 0;
                while (!state.IsFinished && safety++ < 1000)
                {
                    var tick = runner.Tick(state);
                    for (var e = 0; e < tick.Events.Count; e++)
                        if (tick.Events[e].Type == MatchEventType.HazardDetonated) detonations++;

                    var occupied = new HashSet<GridPos>();
                    for (var p = 0; p < state.Players.Count; p++)
                        Assert.IsTrue(occupied.Add(state.Players[p].Position));
                }
                Assert.IsTrue(state.IsFinished);
            }
            Assert.Greater(detonations, 0);
        }
    }
}
