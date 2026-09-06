using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class MatchHighlightTrackerTests
    {
        [Test]
        public void BigBankAndLeaderMissileCreateShareableMoments()
        {
            var state = State();
            var tracker = new MatchHighlightTracker(0);
            var tick = new TickResult();
            tick.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 12));
            tick.Events.Add(new MatchEvent(MatchEventType.MissileFired, 0, secondaryPlayerId: 1));

            tracker.Observe(state, tick);

            Assert.IsTrue(Contains(tracker.Moments, HighlightKind.BigBank));
            Assert.IsTrue(Contains(tracker.Moments, HighlightKind.LeaderMissile));
        }

        [Test]
        public void LocalLateComebackWinBecomesTopHighlight()
        {
            var state = State();
            state.RemainingSeconds = 14f;
            state.PlayerById(1).Score = 10;
            state.PlayerById(0).Score = 5;
            var tracker = new MatchHighlightTracker(0);
            tracker.Observe(state, new TickResult());

            state.PlayerById(0).Score = 12;
            state.PlayerById(1).Score = 10;
            tracker.Observe(state, new TickResult());
            tracker.Complete(state);

            Assert.AreEqual(HighlightKind.ComebackWin, tracker.Best().Kind);
            Assert.AreEqual(100, tracker.Best().ViralityScore);
        }

        private static MatchState State()
        {
            var players = new List<PlayerState>
            {
                new PlayerState(0, "YOU", true, new GridPos(0,0), Direction.Up),
                new PlayerState(1, "BOT", false, new GridPos(7,7), Direction.Down)
            };
            return new MatchState(new BoardState(8,8), players, 75f);
        }

        private static bool Contains(IReadOnlyList<HighlightMoment> moments, HighlightKind kind)
        {
            for (var i = 0; i < moments.Count; i++) if (moments[i].Kind == kind) return true;
            return false;
        }
    }
}
