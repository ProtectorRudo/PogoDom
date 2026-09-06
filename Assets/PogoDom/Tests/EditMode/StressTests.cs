using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class StressTests
    {
        [Test]
        public void TwoHundredBotMatchesFinishWithoutDuplicatePlayers()
        {
            for (uint seed = 1; seed <= 200; seed++)
            {
                var config = new MatchConfig { MatchSeconds = 30f };
                var state = MatchFactory.CreateBotLab(config);
                var runner = new MatchRunner(config, new XorShiftRandom(seed));
                runner.Initialize(state);
                var previousScores = new int[state.Players.Count];
                var safety = 0;

                while (!state.IsFinished && safety++ < 1000)
                {
                    runner.Tick(state);
                    var occupied = new HashSet<GridPos>();
                    for (var p = 0; p < state.Players.Count; p++)
                    {
                        Assert.IsTrue(occupied.Add(state.Players[p].Position), "Duplicate player position at seed " + seed);
                        Assert.GreaterOrEqual(state.Players[p].Score, previousScores[p]);
                        previousScores[p] = state.Players[p].Score;
                    }
                }

                Assert.IsTrue(state.IsFinished, "Match failed to finish at seed " + seed);
            }
        }
    }
}
