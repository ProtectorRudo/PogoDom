using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class PadlockPowerTests
    {
        [Test]
        public void PickupAutomaticallyActivatesPadlockAndConsumesItem()
        {
            var config = EmptyConfig();
            config.EnablePadlockPower = true;
            config.PadlockDurationTicks = 16;
            var player = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.None);
            var state = new MatchState(new BoardState(5, 5), new List<PlayerState> { player }, 20f);
            state.Items.Add(new ItemState(99, PowerUpKind.Padlock, player.Position));
            var events = new List<MatchEvent>();

            Assert.IsTrue(PowerUpResolver.ApplyItemUnderPlayer(state, player, config, events));
            Assert.AreEqual(16, player.PadlockTicksRemaining);
            Assert.IsTrue(player.HasPadlock);
            Assert.AreEqual(0, state.Items.Count);
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.PadlockActivated));
        }

        [Test]
        public void PadlockBlocksDirectStealWithoutBlockingMovement()
        {
            var config = EmptyConfig();
            var attacker = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.None);
            var defender = new PlayerState(1, "B", false, new GridPos(4, 4), Direction.None);
            defender.PadlockTicksRemaining = 3;
            var state = new MatchState(new BoardState(5, 5), new List<PlayerState> { attacker, defender }, 20f);
            state.Board.SetOwner(attacker.Position, defender.Id);
            var runner = new MatchRunner(config, new XorShiftRandom(1));
            runner.Initialize(state);

            var tick = runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.None });

            Assert.AreEqual(defender.Id, state.Board.OwnerAt(attacker.Position));
            Assert.IsTrue(tick.Events.Exists(e => e.Type == MatchEventType.TileProtected && e.SecondaryPlayerId == defender.Id));
        }

        [Test]
        public void ArrowCannotRepaintProtectedEnemyTerritory()
        {
            var attacker = new PlayerState(0, "A", true, new GridPos(0, 2), Direction.Right);
            var defender = new PlayerState(1, "B", false, new GridPos(4, 4), Direction.Left);
            defender.PadlockTicksRemaining = 5;
            var state = new MatchState(new BoardState(5, 5), new List<PlayerState> { attacker, defender }, 20f);
            state.Board.SetOwner(new GridPos(2, 2), defender.Id);

            ArrowResolver.Apply(state, attacker, Direction.Right);

            Assert.AreEqual(defender.Id, state.Board.OwnerAt(new GridPos(2, 2)));
            Assert.AreEqual(attacker.Id, state.Board.OwnerAt(new GridPos(1, 2)));
            Assert.AreEqual(attacker.Id, state.Board.OwnerAt(new GridPos(3, 2)));
        }

        [Test]
        public void ExpiredPadlockAllowsStealOnNextPaintPhase()
        {
            var config = EmptyConfig();
            var attacker = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.None);
            var defender = new PlayerState(1, "B", false, new GridPos(4, 4), Direction.None);
            defender.PadlockTicksRemaining = 1;
            var state = new MatchState(new BoardState(5, 5), new List<PlayerState> { attacker, defender }, 20f);
            state.Board.SetOwner(attacker.Position, defender.Id);
            var runner = new MatchRunner(config, new XorShiftRandom(2));
            runner.Initialize(state);

            runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.None });
            Assert.AreEqual(defender.Id, state.Board.OwnerAt(attacker.Position));
            Assert.IsFalse(defender.HasPadlock);

            runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.None });
            Assert.AreEqual(attacker.Id, state.Board.OwnerAt(attacker.Position));
        }

        [Test]
        public void OneHundredPadlockMatchesFinishCleanly()
        {
            for (uint seed = 1; seed <= 100; seed++)
            {
                var config = new MatchConfig { MatchSeconds = 20f, EnablePadlockPower = true };
                var state = MatchFactory.CreateBotLab(config);
                var runner = new MatchRunner(config, new XorShiftRandom(seed));
                runner.Initialize(state);
                var safety = 0;
                while (!state.IsFinished && safety++ < 1000) runner.Tick(state);
                Assert.IsTrue(state.IsFinished);
            }
        }

        private static MatchConfig EmptyConfig()
        {
            return new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                TargetPadlocks = 0,
                EnableArenaChaos = false,
                EnableEnclosureCapture = false
            };
        }
    }
}
