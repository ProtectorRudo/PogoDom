using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class WinnerShowcasePlanTests
    {
        [Test]
        public void HumanWinnerFacesRunnerUpAndOwnsCelebration()
        {
            var state = CreateState();
            state.PlayerById(0).Score = 100;
            state.PlayerById(1).Score = 80;
            state.PlayerById(2).Score = 40;
            state.PlayerById(3).Score = 20;

            var plan = WinnerShowcaseResolver.Create(state, 0);

            Assert.IsTrue(plan.IsHumanWinner);
            Assert.AreEqual(1, plan.HumanPlacement);
            Assert.AreEqual(0, plan.WinnerPlayerId);
            Assert.AreEqual(1, plan.RunnerUpPlayerId);
            Assert.AreEqual(1, plan.FocalRivalPlayerId);
            Assert.AreEqual(0, plan.CelebrationPlayerId);
            Assert.AreEqual(ShowcaseOutcomeTone.Victory, plan.Tone);
        }

        [Test]
        public void HumanLossFacesActualWinnerNotAnArbitraryBot()
        {
            var state = CreateState();
            state.PlayerById(2).Score = 120;
            state.PlayerById(3).Score = 90;
            state.PlayerById(0).Score = 50;
            state.PlayerById(1).Score = 10;

            var plan = WinnerShowcaseResolver.Create(state, 0);

            Assert.IsFalse(plan.IsHumanWinner);
            Assert.AreEqual(3, plan.HumanPlacement);
            Assert.AreEqual(2, plan.WinnerPlayerId);
            Assert.AreEqual(3, plan.RunnerUpPlayerId);
            Assert.AreEqual(2, plan.FocalRivalPlayerId);
            Assert.AreEqual(2, plan.CelebrationPlayerId);
            Assert.AreEqual(ShowcaseOutcomeTone.Defeat, plan.Tone);
        }

        [Test]
        public void ShowcaseUsesExactDeterministicStandingTieBreaks()
        {
            var state = CreateState();
            for (var i = 0; i < state.Players.Count; i++) state.Players[i].Score = 10;

            // Same score and no owned tiles means MatchOutcome falls back to
            // player id; showcase must never invent a different ordering.
            var standings = MatchOutcome.Standings(state);
            var plan = WinnerShowcaseResolver.Create(state, 3);

            Assert.AreEqual(standings[0].PlayerId, plan.WinnerPlayerId);
            Assert.AreEqual(standings[1].PlayerId, plan.RunnerUpPlayerId);
            Assert.AreEqual(4, plan.HumanPlacement);
            Assert.AreEqual(standings[0].PlayerId, plan.FocalRivalPlayerId);
        }

        [Test]
        public void MobileTimelineIsOrderedAndOutsideBattleRules()
        {
            var timeline = WinnerShowcaseTimeline.FirstMobileHypothesis();
            Assert.LessOrEqual(timeline.ResultFreezeSeconds, timeline.VsRevealSeconds);
            Assert.LessOrEqual(timeline.VsRevealSeconds, timeline.CelebrationStartSeconds);
            Assert.LessOrEqual(timeline.CelebrationStartSeconds, timeline.PrimaryCtaSeconds);
            Assert.AreEqual(2.40f, timeline.CelebrationWindowSeconds, 0.0001f);
        }

        private static MatchState CreateState()
        {
            return MatchFactory.CreateClassicPrototype(new MatchConfig());
        }
    }
}
