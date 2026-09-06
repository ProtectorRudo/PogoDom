using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class EnclosureResolverTests
    {
        [Test]
        public void ClosedRectangleCapturesInteriorIncludingEnemyTile()
        {
            var board = new BoardState(6, 6);
            PaintRing(board, 0, 1, 1, 4, 4);
            board.SetOwner(new GridPos(2, 2), 1);

            var players = new List<PlayerState>
            {
                new PlayerState(0, "A", true, new GridPos(1, 1), Direction.Right),
                new PlayerState(1, "B", false, new GridPos(5, 5), Direction.Left)
            };

            var results = EnclosureResolver.CaptureSimultaneous(board, players);

            Assert.AreEqual(4, results[0].Count);
            Assert.AreEqual(1, results[0].StolenTiles);
            Assert.AreEqual(0, board.OwnerAt(new GridPos(2, 2)));
            Assert.AreEqual(0, board.OwnerAt(new GridPos(3, 3)));
            Assert.AreEqual(TileState.NeutralOwner, board.OwnerAt(new GridPos(0, 0)));
        }

        [Test]
        public void OpenShapeDoesNotCaptureAnything()
        {
            var board = new BoardState(6, 6);
            // Three sides of a box, deliberately open at the top.
            for (var y = 1; y <= 4; y++)
            {
                board.SetOwner(new GridPos(1, y), 0);
                board.SetOwner(new GridPos(4, y), 0);
            }
            for (var x = 1; x <= 4; x++)
                board.SetOwner(new GridPos(x, 1), 0);

            Assert.AreEqual(0, EnclosureResolver.FindEnclosed(board, 0).Count);
        }

        [Test]
        public void TwoIndependentLoopsAreBothDetected()
        {
            var board = new BoardState(8, 8);
            PaintRing(board, 0, 0, 0, 2, 2);
            PaintRing(board, 0, 5, 5, 7, 7);

            var enclosed = EnclosureResolver.FindEnclosed(board, 0);

            Assert.AreEqual(2, enclosed.Count);
            CollectionAssert.Contains(enclosed, new GridPos(1, 1));
            CollectionAssert.Contains(enclosed, new GridPos(6, 6));
        }

        [Test]
        public void PreviewDoesNotMutateBoard()
        {
            var board = new BoardState(5, 5);
            // Ring missing one top tile. Candidate closes it.
            PaintRing(board, 0, 0, 0, 4, 4);
            board.SetOwner(new GridPos(2, 0), TileState.NeutralOwner);

            var before = board.OwnerAt(new GridPos(2, 0));
            var preview = EnclosureResolver.PreviewCaptureCount(board, 0, new GridPos(2, 0));

            Assert.AreEqual(9, preview);
            Assert.AreEqual(before, board.OwnerAt(new GridPos(2, 0)));
        }

        [Test]
        public void OneHundredLoopEnabledBotMatchesStayValid()
        {
            var enclosureEvents = 0;
            for (uint seed = 1; seed <= 100; seed++)
            {
                var config = new MatchConfig
                {
                    MatchSeconds = 30f,
                    EnableEnclosureCapture = true
                };
                var state = MatchFactory.CreateBotLab(config);
                var runner = new MatchRunner(config, new XorShiftRandom(seed));
                runner.Initialize(state);
                var safety = 0;

                while (!state.IsFinished && safety++ < 1000)
                {
                    var tick = runner.Tick(state);
                    for (var e = 0; e < tick.Events.Count; e++)
                        if (tick.Events[e].Type == MatchEventType.EnclosureCaptured) enclosureEvents++;

                    var occupied = new HashSet<GridPos>();
                    for (var p = 0; p < state.Players.Count; p++)
                        Assert.IsTrue(occupied.Add(state.Players[p].Position));
                }

                Assert.IsTrue(state.IsFinished);
            }

            Assert.Greater(enclosureEvents, 0, "Loop mode never produced an enclosure in the deterministic bot lab.");
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
