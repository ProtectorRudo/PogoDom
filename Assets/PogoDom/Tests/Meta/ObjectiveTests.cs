using NUnit.Framework;
using PogoDom.Meta;

namespace PogoDom.Tests.Meta
{
    public sealed class ObjectiveTests
    {
        [Test]
        public void ObjectiveCapsAtTarget()
        {
            var objective = new ObjectiveState("o", ObjectiveScope.City, "la-plata", ObjectiveMetric.TilesPainted, 5);
            Assert.AreEqual(5, objective.Apply(99));
            Assert.AreEqual(5, objective.Current);
            Assert.IsTrue(objective.IsComplete);
            Assert.AreEqual(0, objective.Apply(1));
        }

        [Test]
        public void NationObjectiveAcceptsOnlyMatchingNation()
        {
            var world = new WorldState();
            var ar = new NationId("argentina");
            var uy = new NationId("uruguay");
            var lp = new CityId("la-plata");
            world.AddNation(new NationState(ar));
            world.AddNation(new NationState(uy));
            world.AddCity(new CityState(lp, ar, 800_000));
            world.Objectives.Add(new ObjectiveState("nation-wins", ObjectiveScope.Nation, ar.Value, ObjectiveMetric.Wins, 3));

            var result = new BattleResult("m", true, true, lp, ar, true, 1, 10, 0, 0, 10, 1, 0, 0, 0);
            new MetaProgressionEngine().Apply(world, result, CampaignContext.None(lp));

            Assert.AreEqual(1, world.Objectives[0].Current);
        }
    }
}
