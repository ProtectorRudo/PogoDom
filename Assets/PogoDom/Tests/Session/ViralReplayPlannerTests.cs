using System;
using NUnit.Framework;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class ViralReplayPlannerTests
    {
        [Test]
        public void ComebackRecipeReplaysFromZeroButOnlyRendersFinalFourAndHalfSeconds()
        {
            var highlight = new HighlightMoment(HighlightKind.ComebackWin, 0, 2, 100, 150);
            var recipe = ViralReplayPlanner.Create(highlight, 150, 0.5f);

            Assert.AreEqual(0, recipe.SimulationStartTick);
            Assert.AreEqual(141, recipe.RenderStartTick);
            Assert.AreEqual(150, recipe.HighlightTick);
            Assert.AreEqual(150, recipe.RenderEndTick);
            Assert.AreEqual(141, recipe.WarmupTicks);
            Assert.AreEqual(4.5f, recipe.RenderDurationSeconds, 0.0001f);
            Assert.IsTrue(recipe.AppendWinnerShowcase);
            Assert.AreEqual(ViralReplayAspect.Vertical9x16, recipe.PreferredAspect);
            Assert.AreEqual("share.comeback_win", recipe.CaptionKey);
        }

        [Test]
        public void EarlyHighlightWindowClampsAtMatchStart()
        {
            var highlight = new HighlightMoment(HighlightKind.BigBank, 0, 12, 86, 2);
            var recipe = ViralReplayPlanner.Create(highlight, 150, 0.5f);

            Assert.AreEqual(0, recipe.RenderStartTick);
            Assert.AreEqual(2, recipe.HighlightTick);
            Assert.AreEqual(5, recipe.RenderEndTick);
            Assert.IsFalse(recipe.AppendWinnerShowcase);
        }

        [Test]
        public void EqualViralityPrefersLaterMomentDeterministically()
        {
            var earlier = new HighlightMoment(HighlightKind.AreaCapture, 0, 8, 90, 40);
            var later = new HighlightMoment(HighlightKind.BigBank, 0, 14, 90, 80);

            var best = ViralReplayPlanner.SelectBest(new[] { earlier, later });
            Assert.AreSame(later, best);

            var reverse = ViralReplayPlanner.SelectBest(new[] { later, earlier });
            Assert.AreSame(later, reverse);
        }

        [Test]
        public void UntimestampedLegacyMomentIsNotEligibleForReplayRecipe()
        {
            var legacy = new HighlightMoment(HighlightKind.BigBank, 0, 12, 90);
            Assert.IsNull(ViralReplayPlanner.SelectBest(new[] { legacy }));
            Assert.Throws<InvalidOperationException>(() => ViralReplayPlanner.Create(legacy, 150, 0.5f));
        }

        [Test]
        public void EveryHighlightKindKeepsCoreReplayWindowShort()
        {
            foreach (HighlightKind kind in Enum.GetValues(typeof(HighlightKind)))
            {
                var highlight = new HighlightMoment(kind, 0, 0, 80, 100);
                var recipe = ViralReplayPlanner.Create(highlight, 200, 0.5f);

                Assert.LessOrEqual(recipe.RenderDurationSeconds, 4.5f, kind + " produces a bloated share clip core.");
                Assert.LessOrEqual(recipe.RenderStartTick, recipe.HighlightTick);
                Assert.GreaterOrEqual(recipe.RenderEndTick, recipe.HighlightTick);
                Assert.AreEqual(ViralReplayAspect.Vertical9x16, recipe.PreferredAspect);
                Assert.IsTrue(recipe.CaptionKey.StartsWith("share.", StringComparison.Ordinal));
            }
        }

        [Test]
        public void PhotoFinishAndComebackAppendWinnerShowcaseButOrdinaryMomentsDoNot()
        {
            var photo = ViralReplayPlanner.Create(
                new HighlightMoment(HighlightKind.PhotoFinish, 0, 1, 92, 150), 150, 0.5f);
            var comeback = ViralReplayPlanner.Create(
                new HighlightMoment(HighlightKind.ComebackWin, 0, 1, 100, 150), 150, 0.5f);
            var bank = ViralReplayPlanner.Create(
                new HighlightMoment(HighlightKind.BigBank, 0, 12, 90, 80), 150, 0.5f);

            Assert.IsTrue(photo.AppendWinnerShowcase);
            Assert.IsTrue(comeback.AppendWinnerShowcase);
            Assert.IsFalse(bank.AppendWinnerShowcase);
        }
    }
}
