using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class EnclosureHighlightTests
    {
        [Test]
        public void LargeAreaCaptureBecomesShareableMoment()
        {
            var state = new MatchState(
                new BoardState(8, 8),
                new List<PlayerState>
                {
                    new PlayerState(0, "YOU", true, new GridPos(1, 1), Direction.Right),
                    new PlayerState(1, "BOT", false, new GridPos(6, 6), Direction.Left)
                },
                40f);
            var tick = new TickResult();
            tick.Events.Add(new MatchEvent(MatchEventType.EnclosureCaptured, 0, new GridPos(1, 1), 6));
            var tracker = new MatchHighlightTracker(0);

            tracker.Observe(state, tick);

            Assert.AreEqual(1, tracker.Moments.Count);
            Assert.AreEqual(HighlightKind.AreaCapture, tracker.Moments[0].Kind);
            Assert.AreEqual(6, tracker.Moments[0].Value);
            Assert.GreaterOrEqual(tracker.Moments[0].ViralityScore, 90);
        }
    }
}
