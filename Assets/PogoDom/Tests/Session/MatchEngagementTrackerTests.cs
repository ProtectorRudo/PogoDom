using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class MatchEngagementTrackerTests
    {
        [Test]
        public void OrdinaryStealsKeepActivityAliveButDoNotHideSpectacleDeadZone()
        {
            var state = State();
            var tracker = new MatchEngagementTracker(0.5f);

            for (var tickIndex = 0; tickIndex < 10; tickIndex++)
            {
                var tick = new TickResult();
                tick.Events.Add(new MatchEvent(MatchEventType.TileStolen, 0, value: 1, secondaryPlayerId: 1));
                tracker.Observe(tickIndex, state, tick);
            }

            var report = tracker.Complete(10);
            Assert.AreEqual(0, report.LongestActivityQuietTicks);
            Assert.AreEqual(10, report.LongestSpectacleQuietTicks);
            Assert.AreEqual(5f, report.LongestSpectacleQuietSeconds);
        }

        [Test]
        public void BigBankAndLeadChangeBreakSpectacleSilence()
        {
            var state = State();
            state.PlayerById(0).Score = 8;
            state.PlayerById(1).Score = 3;
            var tracker = new MatchEngagementTracker(0.5f);

            for (var tickIndex = 0; tickIndex < 12; tickIndex++)
            {
                var tick = new TickResult();
                if (tickIndex == 3)
                    tick.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 11));
                if (tickIndex == 7)
                {
                    state.PlayerById(1).Score = 15;
                    state.PlayerById(0).Score = 8;
                }
                tracker.Observe(tickIndex, state, tick);
            }

            var report = tracker.Complete(12);
            Assert.AreEqual(2, report.SpectacleBeatTicks);
            Assert.Less(report.LongestSpectacleQuietSeconds, 5f);
            Assert.IsTrue(Contains(tracker.Beats, EngagementBeatKind.BigBank));
            Assert.IsTrue(Contains(tracker.Beats, EngagementBeatKind.LeadChange));
        }

        [Test]
        public void FiveSecondThresholdCountsOnlyActualLongQuietSpan()
        {
            var state = State();
            var tracker = new MatchEngagementTracker(0.5f);

            for (var tickIndex = 0; tickIndex < 20; tickIndex++)
            {
                var tick = new TickResult();
                if (tickIndex == 4 || tickIndex == 15)
                    tick.Events.Add(new MatchEvent(MatchEventType.MissileFired, 0, secondaryPlayerId: 1));
                tracker.Observe(tickIndex, state, tick);
            }

            var report = tracker.Complete(20);
            Assert.AreEqual(1, report.CountQuietSpansAtLeast(EngagementTier.Spectacle, 5f));
            Assert.AreEqual(10, report.QuietTicksInsideSpansAtLeast(EngagementTier.Spectacle, 5f));
        }

        [Test]
        public void SmallBankIsActivityButOnlyLargeBankIsSpectacle()
        {
            var state = State();
            var tracker = new MatchEngagementTracker(0.5f);
            var small = new TickResult();
            small.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 4));
            tracker.Observe(0, state, small);
            var large = new TickResult();
            large.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 8));
            tracker.Observe(1, state, large);

            var report = tracker.Complete(2);
            Assert.AreEqual(2, report.ActivityBeatTicks);
            Assert.AreEqual(1, report.SpectacleBeatTicks);
        }

        private static MatchState State()
        {
            return new MatchState(
                new BoardState(8, 8),
                new List<PlayerState>
                {
                    new PlayerState(0, "YOU", true, new GridPos(0, 0), Direction.Up),
                    new PlayerState(1, "BOT", false, new GridPos(7, 7), Direction.Down)
                },
                75f);
        }

        private static bool Contains(IReadOnlyList<EngagementBeat> beats, EngagementBeatKind kind)
        {
            for (var i = 0; i < beats.Count; i++) if (beats[i].Kind == kind) return true;
            return false;
        }
    }
}
