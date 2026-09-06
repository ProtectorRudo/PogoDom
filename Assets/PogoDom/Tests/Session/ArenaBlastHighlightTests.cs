using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class ArenaBlastHighlightTests
    {
        [Test]
        public void LargeNeutralBlastCanBecomeViralMoment()
        {
            var state = new MatchState(
                new BoardState(8, 8),
                new List<PlayerState>
                {
                    new PlayerState(0, "YOU", true, new GridPos(0, 0), Direction.Right),
                    new PlayerState(1, "BOT", false, new GridPos(7, 7), Direction.Left)
                },
                30f);
            var tick = new TickResult();
            tick.Events.Add(new MatchEvent(MatchEventType.HazardDetonated, -1, new GridPos(4, 4), 7, PowerUpKind.Tnt));
            var tracker = new MatchHighlightTracker(0);

            tracker.Observe(state, tick);

            Assert.AreEqual(1, tracker.Moments.Count);
            Assert.AreEqual(HighlightKind.ArenaBlast, tracker.Moments[0].Kind);
            Assert.AreEqual(7, tracker.Moments[0].Value);
        }
    }
}
