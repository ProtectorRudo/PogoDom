using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class FirstRunCoachTests
    {
        [Test]
        public void CoachIsNeverBlockingAndTimesOutByTwentySeconds()
        {
            var coach = new FirstRunCoach(0);
            Assert.AreEqual(CoachHint.SwipeToTurn, coach.Current.Hint);
            Assert.IsFalse(coach.Current.IsBlocking);

            coach.AdvanceWithoutTick(20f);

            Assert.IsTrue(coach.IsComplete);
            Assert.AreEqual(CoachHint.None, coach.Current.Hint);
            Assert.IsFalse(coach.Current.IsBlocking);
        }

        [Test]
        public void CoachProgressesFromMoveToPaintToBank()
        {
            var coach = new FirstRunCoach(0);
            var move = new TickResult();
            move.Events.Add(new MatchEvent(MatchEventType.PlayerMoved, 0));
            coach.Observe(move, 0.5f);
            Assert.AreEqual(CoachHint.PaintAndSteal, coach.Current.Hint);

            var paint = new TickResult();
            paint.Events.Add(new MatchEvent(MatchEventType.TilePainted, 0));
            coach.Observe(paint, 0.5f);
            Assert.AreEqual(CoachHint.BankToScore, coach.Current.Hint);

            var bank = new TickResult();
            bank.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 5));
            coach.Observe(bank, 0.5f);
            Assert.IsTrue(coach.IsComplete);
        }
    }
}
