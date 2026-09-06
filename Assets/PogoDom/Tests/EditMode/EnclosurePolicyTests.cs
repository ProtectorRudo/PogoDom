using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class EnclosurePolicyTests
    {
        [Test]
        public void NeutralOnlyFillsEmptyInteriorButPreservesEnemyInterior()
        {
            var board = new BoardState(6, 6);
            PaintRing(board, 0, 1, 1, 4, 4);
            board.SetOwner(new GridPos(2, 2), 1);
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(5, 5), Direction.Left)
            };

            var result = EnclosureResolver.CaptureSimultaneous(board, players, EnclosureCapturePolicy.NeutralOnly);

            Assert.AreEqual(3, result[0].Count);
            Assert.AreEqual(0, result[0].StolenTiles);
            Assert.AreEqual(1, board.OwnerAt(new GridPos(2, 2)));
            Assert.AreEqual(0, board.OwnerAt(new GridPos(3, 3)));
        }

        [Test]
        public void V1PolicyStillCapturesEnemyInterior()
        {
            var board = new BoardState(6, 6);
            PaintRing(board, 0, 1, 1, 4, 4);
            board.SetOwner(new GridPos(2, 2), 1);
            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(5, 5), Direction.Left)
            };

            var result = EnclosureResolver.CaptureSimultaneous(board, players, EnclosureCapturePolicy.AllUnprotected);

            Assert.AreEqual(4, result[0].Count);
            Assert.AreEqual(1, result[0].StolenTiles);
            Assert.AreEqual(0, board.OwnerAt(new GridPos(2, 2)));
        }

        private static void PaintRing(BoardState board, int playerId, int minX, int minY, int maxX, int maxY)
        {
            for (var x = minX; x <= maxX; x++)
            {
                board.SetOwner(new GridPos(x, minY), playerId);
                board.SetOwner(new GridPos(x, maxY), playerId);
            }
            for (var y = minY; y <= maxY; y++)
            {
                board.SetOwner(new GridPos(minX, y), playerId);
                board.SetOwner(new GridPos(maxX, y), playerId);
            }
        }
    }
}
