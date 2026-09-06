using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class HardBotBrainTests
    {
        [Test]
        public void HardBotPlansTwoStepsAheadTowardValuableBank()
        {
            var config = EmptyConfig();
            config.BankThresholdForBots = 5;
            var bot = new PlayerState(0, "HARD", false, new GridPos(1, 1), Direction.Up, BotDifficulty.Hard, BotPersonality.Balanced);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { bot }, 60f);
            for (var x = 0; x < 6; x++) state.Board.SetOwner(new GridPos(x, 0), bot.Id);
            state.Items.Add(new ItemState(1, PowerUpKind.BankCrate, new GridPos(3, 1)));

            var direction = new BotDirector().ChooseDirection(state, bot, config, new XorShiftRandom(1));

            Assert.AreEqual(Direction.Right, direction);
        }

        [Test]
        public void HardBotRejectsImminentTntEvenWhenMissileIsInsideBlast()
        {
            var config = EmptyConfig();
            var bot = new PlayerState(0, "HARD", false, new GridPos(1, 1), Direction.Right, BotDifficulty.Hard, BotPersonality.Balanced);
            var rival = new PlayerState(1, "RIVAL", false, new GridPos(7, 7), Direction.Left);
            rival.Score = 20;
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { bot, rival }, 60f);
            state.Items.Add(new ItemState(1, PowerUpKind.Missile, new GridPos(2, 1)));
            state.Hazards.Add(new ArenaHazard(1, ArenaHazardKind.Tnt, new GridPos(2, 1), 0, 1));

            var direction = new BotDirector().ChooseDirection(state, bot, config, new XorShiftRandom(2));

            Assert.AreNotEqual(Direction.Right, direction);
        }

        [Test]
        public void HardBotAvoidsOccupiedLandingWhenOpenRoutesExist()
        {
            var config = EmptyConfig();
            var bot = new PlayerState(0, "HARD", false, new GridPos(1, 1), Direction.Right, BotDifficulty.Hard, BotPersonality.Balanced);
            var blocker = new PlayerState(1, "BLOCKER", false, new GridPos(2, 1), Direction.Left);
            var state = new MatchState(new BoardState(8, 8), new List<PlayerState> { bot, blocker }, 60f);

            var direction = new BotDirector().ChooseDirection(state, bot, config, new XorShiftRandom(3));

            Assert.AreNotEqual(Direction.Right, direction);
        }

        [Test]
        public void OneHundredMixedDifficultyMatchesStayValid()
        {
            var config = new MatchConfig();
            for (var m = 0; m < 100; m++)
            {
                var hardSlot = m % 4;
                var state = MatchFactory.CreateDifficultyLab(config, hardSlot);
                var runner = new MatchRunner(config, new XorShiftRandom((uint)(9000 + m * 31)));
                runner.Initialize(state);
                var safety = 0;
                while (!state.IsFinished && safety++ < 1000)
                    runner.Tick(state);

                Assert.IsTrue(state.IsFinished, "match did not finish: " + m);
                var seen = new HashSet<GridPos>();
                for (var i = 0; i < state.Players.Count; i++)
                    Assert.IsTrue(seen.Add(state.Players[i].Position), "duplicate player position in match " + m);
            }
        }

        private static MatchConfig EmptyConfig()
        {
            return new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0,
                EnablePadlockPower = false,
                EnableArenaChaos = false,
                EnableEnclosureCapture = false
            };
        }
    }
}
