using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class MatchOutcomeTests
    {
        [Test]
        public void ScoreWinsBeforeUnbankedTerritory()
        {
            var a = new PlayerState(0, "A", false, new GridPos(0, 0), Direction.Up);
            var b = new PlayerState(1, "B", false, new GridPos(7, 7), Direction.Down);
            a.Score = 10;
            b.Score = 9;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { a, b }, 0f);
            for (var x = 0; x < 8; x++) state.Board.SetOwner(new GridPos(x, 4), b.Id);

            var standings = MatchOutcome.Standings(state);

            Assert.AreEqual(a.Id, standings[0].PlayerId);
        }

        [Test]
        public void UnbankedTerritoryBreaksExactScoreTie()
        {
            var a = new PlayerState(0, "A", false, new GridPos(0, 0), Direction.Up);
            var b = new PlayerState(1, "B", false, new GridPos(7, 7), Direction.Down);
            a.Score = 10;
            b.Score = 10;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { a, b }, 0f);
            state.Board.SetOwner(new GridPos(1, 1), b.Id);
            state.Board.SetOwner(new GridPos(2, 1), b.Id);

            Assert.AreEqual(b.Id, MatchOutcome.Standings(state)[0].PlayerId);
        }
    }
}
