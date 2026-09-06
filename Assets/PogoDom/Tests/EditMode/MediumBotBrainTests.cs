using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class MediumBotBrainTests
    {
        [Test]
        public void GreedyBotTakesAdjacentMissileOverOwnTile()
        {
            var config = new MatchConfig();
            var bot = new PlayerState(0, "BOT", false, new GridPos(3, 3), Direction.Up, BotDifficulty.Medium, BotPersonality.Greedy);
            var other = new PlayerState(1, "OTHER", false, new GridPos(7, 7), Direction.Left);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { bot, other }, 60f);
            state.Board.SetOwner(new GridPos(3, 4), bot.Id);
            state.Items.Add(new ItemState(1, PowerUpKind.Missile, new GridPos(4, 3)));

            var direction = new MediumBotBrain().ChooseDirection(state, bot, config, new XorShiftRandom(3));

            Assert.AreEqual(Direction.Right, direction);
        }

        [Test]
        public void BotNeverChoosesAImmediateWallStepWhenAlternativesExist()
        {
            var config = new MatchConfig();
            var bot = new PlayerState(0, "BOT", false, new GridPos(0, 0), Direction.Left, BotDifficulty.Medium, BotPersonality.Balanced);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { bot }, 60f);

            var direction = new MediumBotBrain().ChooseDirection(state, bot, config, new XorShiftRandom(4));

            Assert.IsTrue(direction == Direction.Up || direction == Direction.Right);
        }
    }
}
