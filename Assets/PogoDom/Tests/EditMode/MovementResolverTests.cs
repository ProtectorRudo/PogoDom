using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class MovementResolverTests
    {
        private static BoardState Board() => new BoardState(8, 8);

        [Test]
        public void StationaryOccupantHasPriority()
        {
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(1, 1), Direction.None),
                new PlayerState(1, "B", false, new GridPos(2, 1), Direction.Left)
            };
            var dirs = new Dictionary<int, Direction> { [0] = Direction.None, [1] = Direction.Left };

            var result = MovementResolver.Resolve(Board(), players, dirs, new XorShiftRandom(1));

            Assert.AreEqual(new GridPos(1, 1), result[0]);
            Assert.AreEqual(new GridPos(2, 1), result[1]);
        }

        [Test]
        public void TwoMoversSameTargetProduceExactlyOneWinner()
        {
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(0, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(2, 1), Direction.Left)
            };
            var dirs = new Dictionary<int, Direction> { [0] = Direction.Right, [1] = Direction.Left };

            var result = MovementResolver.Resolve(Board(), players, dirs, new XorShiftRandom(123));

            var winnerCount = 0;
            if (result[0] == new GridPos(1, 1)) winnerCount++;
            if (result[1] == new GridPos(1, 1)) winnerCount++;
            Assert.AreEqual(1, winnerCount);
            Assert.AreNotEqual(result[0], result[1]);
        }

        [Test]
        public void DirectSwapBlocksBothPlayers()
        {
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(2, 1), Direction.Left)
            };
            var dirs = new Dictionary<int, Direction> { [0] = Direction.Right, [1] = Direction.Left };

            var result = MovementResolver.Resolve(Board(), players, dirs, new XorShiftRandom(5));

            Assert.AreEqual(new GridPos(1, 1), result[0]);
            Assert.AreEqual(new GridPos(2, 1), result[1]);
        }

        [Test]
        public void CollisionChainStabilizesWithoutDuplicatePositions()
        {
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(0, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(1, 1), Direction.Right),
                new PlayerState(2, "C", false, new GridPos(2, 1), Direction.None)
            };
            var dirs = new Dictionary<int, Direction>
            {
                [0] = Direction.Right,
                [1] = Direction.Right,
                [2] = Direction.None
            };

            var result = MovementResolver.Resolve(Board(), players, dirs, new XorShiftRandom(8));

            Assert.AreEqual(new GridPos(0, 1), result[0]);
            Assert.AreEqual(new GridPos(1, 1), result[1]);
            Assert.AreEqual(new GridPos(2, 1), result[2]);
        }
    }
}
