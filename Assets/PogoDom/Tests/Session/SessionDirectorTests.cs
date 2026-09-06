using NUnit.Framework;
using PogoDom.Meta;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class SessionDirectorTests
    {
        [Test]
        public void ResultsAlwaysMakeRematchThePrimaryAction()
        {
            var director = new SessionDirector();
            director.StartImmediate("m1");
            var battle = new BattleResult("m1", true, true, new CityId("la-plata"), new NationId("argentina"), true, 1, 10, 0, 0, 10, 1, 0, 0, 0);
            var model = director.Complete(battle, new ProgressionReceipt(), null, null);

            Assert.AreEqual(ResultPrimaryAction.Rematch, model.PrimaryAction);
            Assert.AreEqual(SessionPhase.Results, director.Phase);

            director.Rematch("m2");
            Assert.AreEqual(SessionPhase.Playing, director.Phase);
            Assert.AreEqual(2, director.MatchesStarted);
            Assert.AreEqual(1, director.RematchesRequested);
        }

        [Test]
        public void RematchCannotReuseMatchId()
        {
            var director = new SessionDirector();
            director.StartImmediate("m1");
            var battle = new BattleResult("m1", true, true, new CityId("la-plata"), new NationId("argentina"), false, 2, 8, 0, 0, 8, 1, 0, 0, 0);
            director.Complete(battle, new ProgressionReceipt(), null, null);

            Assert.Throws<System.InvalidOperationException>(() => director.Rematch("m1"));
        }
    }
}
