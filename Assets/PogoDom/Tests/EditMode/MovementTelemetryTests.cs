using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class MovementTelemetryTests
    {
        [Test]
        public void NoIntentDoesNotCountAsBlockedMovement()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0
            };
            var player = new PlayerState(0, "A", true, new GridPos(2, 2), Direction.None);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 30f);
            var runner = new MatchRunner(config, new XorShiftRandom(2));

            var result = runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.None });

            Assert.IsFalse(result.Events.Exists(e => e.Type == MatchEventType.PlayerBlocked));
            Assert.AreEqual(0, result.MovementSteps.Count);
        }
    }
}
