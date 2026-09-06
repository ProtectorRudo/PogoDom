using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class SwipeGestureTrackerTests
    {
        [Test]
        public void TapAndShortDragDoNotCommit()
        {
            var gesture = new SwipeGestureTracker(45f);
            gesture.Begin(100f, 100f);

            Direction direction;
            Assert.IsFalse(gesture.TryUpdate(120f, 115f, out direction));
            Assert.AreEqual(Direction.None, direction);
            Assert.IsFalse(gesture.HasCommitted);
        }

        [Test]
        public void DirectionCommitsImmediatelyWhenThresholdIsCrossed()
        {
            var gesture = new SwipeGestureTracker(45f);
            gesture.Begin(100f, 100f);

            Direction direction;
            Assert.IsTrue(gesture.TryUpdate(146f, 105f, out direction));
            Assert.AreEqual(Direction.Right, direction);
            Assert.IsTrue(gesture.HasCommitted);
        }

        [Test]
        public void OneGestureCannotSpamMultipleDirectionChanges()
        {
            var gesture = new SwipeGestureTracker(45f);
            gesture.Begin(100f, 100f);

            Direction direction;
            Assert.IsTrue(gesture.TryUpdate(100f, 151f, out direction));
            Assert.AreEqual(Direction.Up, direction);
            Assert.IsFalse(gesture.TryUpdate(25f, 100f, out direction));
            Assert.AreEqual(Direction.None, direction);
        }

        [TestCase(60f, 10f, Direction.Right)]
        [TestCase(-60f, 10f, Direction.Left)]
        [TestCase(10f, 60f, Direction.Up)]
        [TestCase(10f, -60f, Direction.Down)]
        [TestCase(50f, 50f, Direction.Right)]
        [TestCase(-50f, 50f, Direction.Left)]
        public void DominantAxisAndDiagonalTieAreDeterministic(float dx, float dy, Direction expected)
        {
            Assert.AreEqual(expected, SwipeGestureTracker.ResolveCardinal(dx, dy));
        }

        [Test]
        public void ReleasingRearmsNextGesture()
        {
            var gesture = new SwipeGestureTracker(45f);
            Direction direction;

            gesture.Begin(0f, 0f);
            Assert.IsTrue(gesture.TryUpdate(60f, 0f, out direction));
            Assert.AreEqual(Direction.Right, direction);
            gesture.End();

            gesture.Begin(0f, 0f);
            Assert.IsTrue(gesture.TryUpdate(0f, -60f, out direction));
            Assert.AreEqual(Direction.Down, direction);
        }

        [Test]
        public void CommittedSwipeFeedsNextBounceAndPersistsUntilChanged()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                EnablePadlockPower = false,
                EnableMysteryCrates = false
            };
            var player = new PlayerState(0, "YOU", true, new GridPos(2, 2), Direction.Up);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 60f);
            var runner = new MatchRunner(config, new XorShiftRandom(17));
            var gesture = new SwipeGestureTracker(45f);

            gesture.Begin(0f, 0f);
            Direction direction;
            Assert.IsTrue(gesture.TryUpdate(60f, 0f, out direction));

            runner.Tick(state, new Dictionary<int, Direction> { [0] = direction });
            Assert.AreEqual(new GridPos(3, 2), player.Position);

            runner.Tick(state);
            Assert.AreEqual(new GridPos(4, 2), player.Position);
            Assert.AreEqual(Direction.Right, player.CurrentDirection);
        }
    }
}
