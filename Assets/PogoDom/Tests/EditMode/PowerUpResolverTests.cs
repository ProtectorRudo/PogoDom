using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class PowerUpResolverTests
    {
        [Test]
        public void SpeedPickupActivatesAndIsConsumed()
        {
            var config = new MatchConfig { SpeedDurationTicks = 16 };
            var player = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.Right);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 60f);
            state.Items.Add(new ItemState(1, PowerUpKind.Speed, player.Position));
            var events = new List<MatchEvent>();

            var applied = PowerUpResolver.ApplyItemUnderPlayer(state, player, config, events);

            Assert.IsTrue(applied);
            Assert.AreEqual(16, player.SpeedTicksRemaining);
            Assert.AreEqual(0, state.Items.Count);
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.SpeedActivated));
        }

        [Test]
        public void MissileAutomaticallyTargetsCurrentLeader()
        {
            var config = new MatchConfig { MissileStunTicks = 4 };
            var attacker = new PlayerState(0, "A", true, new GridPos(0, 0), Direction.Right);
            var leader = new PlayerState(1, "B", false, new GridPos(7, 7), Direction.Left);
            var other = new PlayerState(2, "C", false, new GridPos(4, 4), Direction.Up);
            leader.Score = 20;
            other.Score = 5;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { attacker, leader, other }, 60f);
            state.Items.Add(new ItemState(1, PowerUpKind.Missile, attacker.Position));
            var events = new List<MatchEvent>();

            PowerUpResolver.ApplyItemUnderPlayer(state, attacker, config, events);

            Assert.AreEqual(4, leader.StunTicksRemaining);
            Assert.AreEqual(0, other.StunTicksRemaining);
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.MissileFired && e.SecondaryPlayerId == leader.Id));
        }

        [Test]
        public void SpeedProducesASecondMovementPhase()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                SpeedDurationTicks = 4
            };
            var player = new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right);
            player.SpeedTicksRemaining = 4;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 60f);
            var runner = new MatchRunner(config, new XorShiftRandom(7));

            var result = runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.Right });

            Assert.AreEqual(new GridPos(3, 1), player.Position);
            Assert.AreEqual(2, result.MovementSteps.FindAll(s => s.PlayerId == 0).Count);
            Assert.AreEqual(3, player.SpeedTicksRemaining);
        }

        [Test]
        public void StunnedPlayerCannotMoveUntilTimerExpires()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0
            };
            var player = new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right);
            player.StunTicksRemaining = 2;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 60f);
            var runner = new MatchRunner(config, new XorShiftRandom(9));

            runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.Right });
            Assert.AreEqual(new GridPos(1, 1), player.Position);
            Assert.AreEqual(1, player.StunTicksRemaining);

            runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.Right });
            Assert.AreEqual(new GridPos(1, 1), player.Position);
            Assert.AreEqual(0, player.StunTicksRemaining);

            runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.Right });
            Assert.AreEqual(new GridPos(2, 1), player.Position);
        }
    }
}
