using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class RivalBotBrainTests
    {
        [Test]
        public void AggressiveRivalPrefersVisibleLeaderTerritory()
        {
            var bot = new PlayerState(1, "RIVAL", false, new GridPos(3, 3), Direction.Up, BotDifficulty.Medium, BotPersonality.Aggressive);
            var leader = new PlayerState(0, "LEADER", true, new GridPos(7, 7), Direction.Left);
            leader.Score = 20;
            var state = State(leader, bot);
            state.Board.SetOwner(new GridPos(4, 3), leader.Id);

            var direction = new RivalBotBrain().ChooseDirection(
                state,
                bot,
                new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V3 },
                new XorShiftRandom(123u));

            Assert.AreEqual(Direction.Right, direction);
        }

        [Test]
        public void BankerWithEnoughTerritoryRoutesTowardBank()
        {
            var leader = new PlayerState(0, "LEADER", true, new GridPos(7, 7), Direction.Left);
            leader.Score = 20;
            var bot = new PlayerState(1, "BANKER", false, new GridPos(3, 3), Direction.Up, BotDifficulty.Medium, BotPersonality.Banker);
            var state = State(leader, bot);
            state.Board.SetOwner(new GridPos(0, 0), bot.Id);
            state.Board.SetOwner(new GridPos(0, 1), bot.Id);
            state.Board.SetOwner(new GridPos(0, 2), bot.Id);
            state.Board.SetOwner(new GridPos(1, 0), bot.Id);
            state.Board.SetOwner(new GridPos(1, 1), bot.Id);
            state.Items.Add(new ItemState(1, PowerUpKind.BankCrate, new GridPos(4, 3)));

            var direction = new RivalBotBrain().ChooseDirection(
                state,
                bot,
                new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V3, BankThresholdForBots = 5 },
                new XorShiftRandom(456u));

            Assert.AreEqual(Direction.Right, direction);
        }

        [Test]
        public void RivalDoesNotTargetLeaderBecauseLeaderIsHuman()
        {
            var humanState = LeaderState(leaderIsHuman: true);
            var botState = LeaderState(leaderIsHuman: false);
            var humanBot = humanState.PlayerById(1);
            var botBot = botState.PlayerById(1);
            var config = new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V3 };

            var a = new RivalBotBrain().ChooseDirection(humanState, humanBot, config, new XorShiftRandom(999u));
            var b = new RivalBotBrain().ChooseDirection(botState, botBot, config, new XorShiftRandom(999u));

            Assert.AreEqual(a, b, "Opponent logic must react to public leader state, not the human flag.");
        }

        [Test]
        public void BotDirectorKeepsV1AndV2OnLegacyMediumPath()
        {
            var leader = new PlayerState(0, "LEADER", true, new GridPos(7, 7), Direction.Left);
            leader.Score = 30;
            var bot = new PlayerState(1, "BOT", false, new GridPos(3, 3), Direction.Up, BotDifficulty.Medium, BotPersonality.Aggressive);
            var state = State(leader, bot);
            state.Board.SetOwner(new GridPos(4, 3), leader.Id);
            var director = new BotDirector();

            var legacy = director.ChooseDirection(
                state,
                bot,
                new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V1 },
                new XorShiftRandom(77u));
            var v2 = director.ChooseDirection(
                state,
                bot,
                new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V2 },
                new XorShiftRandom(77u));
            var legacyBrain = new MediumBotBrain().ChooseDirection(
                state,
                bot,
                new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V1 },
                new XorShiftRandom(77u));

            Assert.AreEqual(legacyBrain, legacy);
            Assert.AreEqual(legacyBrain, v2);
        }

        [Test]
        public void RivalBrainIsDeterministicForSameStateAndSeed()
        {
            var first = LeaderState(true);
            var second = LeaderState(true);
            var config = new MatchConfig { BehaviorVersion = RulesetBehaviorVersion.V3 };

            var a = new RivalBotBrain().ChooseDirection(first, first.PlayerById(1), config, new XorShiftRandom(2026u));
            var b = new RivalBotBrain().ChooseDirection(second, second.PlayerById(1), config, new XorShiftRandom(2026u));

            Assert.AreEqual(a, b);
        }

        private static MatchState LeaderState(bool leaderIsHuman)
        {
            var leader = new PlayerState(0, "LEADER", leaderIsHuman, new GridPos(7, 7), Direction.Left);
            leader.Score = 25;
            var rival = new PlayerState(1, "RIVAL", false, new GridPos(3, 3), Direction.Up, BotDifficulty.Medium, BotPersonality.Aggressive);
            var state = State(leader, rival);
            state.Board.SetOwner(new GridPos(4, 3), leader.Id);
            state.Board.SetOwner(new GridPos(5, 3), leader.Id);
            state.Items.Add(new ItemState(1, PowerUpKind.Speed, new GridPos(3, 5)));
            return state;
        }

        private static MatchState State(PlayerState a, PlayerState b)
        {
            return new MatchState(new BoardState(8, 8), new List<PlayerState> { a, b }, 75f);
        }
    }
}
