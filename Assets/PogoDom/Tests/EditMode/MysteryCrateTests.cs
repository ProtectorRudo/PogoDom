using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class MysteryCrateTests
    {
        [Test]
        public void SameSeedPreRollsSameHiddenPayloadsAndDirections()
        {
            var config = CrateOnlyConfig();
            var stateA = OnePlayerState();
            var stateB = OnePlayerState();
            var spawnerA = new ItemSpawner();
            var spawnerB = new ItemSpawner();

            spawnerA.EnsurePopulation(stateA, config, new XorShiftRandom(12345), new List<MatchEvent>());
            spawnerB.EnsurePopulation(stateB, config, new XorShiftRandom(12345), new List<MatchEvent>());

            Assert.AreEqual(stateA.Items.Count, stateB.Items.Count);
            for (var i = 0; i < stateA.Items.Count; i++)
            {
                Assert.AreEqual(PowerUpKind.MysteryCrate, stateA.Items[i].Kind);
                Assert.AreEqual(stateA.Items[i].Position, stateB.Items[i].Position);
                Assert.AreEqual(stateA.Items[i].ContainedPower, stateB.Items[i].ContainedPower);
                Assert.AreEqual(stateA.Items[i].ArrowDirection, stateB.Items[i].ArrowDirection);
                Assert.IsTrue(MysteryCrateTable.IsValidPayload(stateA.Items[i].ContainedPower));
            }
        }

        [Test]
        public void OpeningSpeedCrateRevealsAndAppliesPayloadAutomatically()
        {
            var config = new MatchConfig { SpeedDurationTicks = 9 };
            var player = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.Right);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 60f);
            state.Items.Add(new ItemState(1, PowerUpKind.MysteryCrate, player.Position, Direction.Left, PowerUpKind.Speed));
            var events = new List<MatchEvent>();

            var applied = PowerUpResolver.ApplyItemUnderPlayer(state, player, config, events);

            Assert.IsTrue(applied);
            Assert.AreEqual(9, player.SpeedTicksRemaining);
            Assert.AreEqual(0, state.Items.Count);
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.CrateOpened && e.ItemKind == PowerUpKind.Speed));
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.SpeedActivated));
            Assert.IsTrue(events.Exists(e => e.Type == MatchEventType.ItemConsumed && e.ItemKind == PowerUpKind.MysteryCrate));
        }

        [Test]
        public void BotsCannotReadHiddenPayloadThroughPickupKind()
        {
            var a = new ItemState(1, PowerUpKind.MysteryCrate, new GridPos(1, 1), Direction.Up, PowerUpKind.Arrow);
            var b = new ItemState(2, PowerUpKind.MysteryCrate, new GridPos(2, 2), Direction.Up, PowerUpKind.Missile);
            Assert.AreEqual(a.Kind, b.Kind);
            Assert.AreEqual(PowerUpKind.MysteryCrate, a.Kind);
            Assert.AreNotEqual(a.ContainedPower, b.ContainedPower);
        }

        [Test]
        public void InvalidCratePayloadIsRejectedAtConstruction()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new ItemState(1, PowerUpKind.MysteryCrate, new GridPos(1, 1), Direction.Up, PowerUpKind.BankCrate));
        }

        private static MatchConfig CrateOnlyConfig()
        {
            return new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                EnablePadlockPower = false,
                EnableMysteryCrates = true,
                TargetMysteryCrates = 3,
                MysteryCrateTableId = MysteryCrateTableId.PowerMixV1
            };
        }

        private static MatchState OnePlayerState()
        {
            return new MatchState(
                new BoardState(8, 8),
                new List<PlayerState> { new PlayerState(0, "A", true, new GridPos(0, 0), Direction.Right) },
                60f);
        }
    }
}
