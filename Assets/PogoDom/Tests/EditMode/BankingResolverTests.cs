using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class BankingResolverTests
    {
        [Test]
        public void BankConvertsOwnedTilesToScoreAndClearsThem()
        {
            var board = new BoardState(8, 8);
            var player = new PlayerState(0, "A", true, new GridPos(0, 0), Direction.Up);
            var state = new MatchState(board, new List<PlayerState> { player }, 90f);

            board.SetOwner(new GridPos(1, 1), 0);
            board.SetOwner(new GridPos(1, 2), 0);
            board.SetOwner(new GridPos(2, 2), 0);
            board.SetOwner(new GridPos(7, 7), 9);

            var banked = BankingResolver.Bank(state, player);

            Assert.AreEqual(3, banked);
            Assert.AreEqual(3, player.Score);
            Assert.AreEqual(TileState.NeutralOwner, board.OwnerAt(new GridPos(1, 1)));
            Assert.AreEqual(TileState.NeutralOwner, board.OwnerAt(new GridPos(1, 2)));
            Assert.AreEqual(TileState.NeutralOwner, board.OwnerAt(new GridPos(2, 2)));
            Assert.AreEqual(9, board.OwnerAt(new GridPos(7, 7)));
        }

        [Test]
        public void RunnerBanksBeforePaintingCurrentSquare()
        {
            var config = new MatchConfig { TargetBankCrates = 0, TargetArrows = 0 };
            var player = new PlayerState(0, "A", true, new GridPos(3, 3), Direction.None);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { player }, 90f);
            state.Board.SetOwner(new GridPos(1, 1), 0);
            state.Board.SetOwner(new GridPos(2, 1), 0);
            state.Items.Add(new ItemState(1, PowerUpKind.BankCrate, player.Position));

            var runner = new MatchRunner(config, new XorShiftRandom(2));
            var result = runner.Tick(state, new Dictionary<int, Direction> { [0] = Direction.None });

            Assert.AreEqual(2, player.Score);
            Assert.AreEqual(0, state.Board.OwnerAt(player.Position), "Current bank square should begin the new territory after banking.");
            Assert.AreEqual(TileState.NeutralOwner, state.Board.OwnerAt(new GridPos(1, 1)));
            Assert.IsTrue(result.Events.Exists(e => e.Type == MatchEventType.Banked && e.Value == 2));
        }
    }
}
