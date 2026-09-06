using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class DeterminismTests
    {
        [Test]
        public void SameSeedAndInputsProduceSameState()
        {
            var configA = new MatchConfig();
            var configB = new MatchConfig();
            var stateA = MatchFactory.CreateClassicPrototype(configA);
            var stateB = MatchFactory.CreateClassicPrototype(configB);
            var runnerA = new MatchRunner(configA, new XorShiftRandom(777));
            var runnerB = new MatchRunner(configB, new XorShiftRandom(777));
            runnerA.Initialize(stateA);
            runnerB.Initialize(stateB);

            var input = new Dictionary<int, Direction> { [0] = Direction.Right };
            for (var i = 0; i < 20; i++)
            {
                runnerA.Tick(stateA, input);
                runnerB.Tick(stateB, input);
            }

            Assert.AreEqual(stateA.Tick, stateB.Tick);
            Assert.AreEqual(stateA.RemainingSeconds, stateB.RemainingSeconds);
            Assert.AreEqual(stateA.Items.Count, stateB.Items.Count);

            for (var p = 0; p < stateA.Players.Count; p++)
            {
                Assert.AreEqual(stateA.Players[p].Position, stateB.Players[p].Position);
                Assert.AreEqual(stateA.Players[p].Score, stateB.Players[p].Score);
            }

            for (var y = 0; y < stateA.Board.Height; y++)
                for (var x = 0; x < stateA.Board.Width; x++)
                    Assert.AreEqual(stateA.Board.OwnerAt(new GridPos(x, y)), stateB.Board.OwnerAt(new GridPos(x, y)));
        }
    }
}
