using NUnit.Framework;
using PogoDom.Meta;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class ObjectiveSpotlightTests
    {
        [Test]
        public void SelectorReturnsOnlyOneRelevantIncompleteObjective()
        {
            var world = new WorldState();
            var ar = new NationId("argentina");
            var city = new CityId("la-plata");
            world.AddNation(new NationState(ar));
            world.AddCity(new CityState(city, ar, 800000));

            var a = new ObjectiveState("fresh", ObjectiveScope.City, city.Value, ObjectiveMetric.Wins, 10);
            var b = new ObjectiveState("almost", ObjectiveScope.Nation, ar.Value, ObjectiveMetric.TilesStolen, 100);
            b.Current = 80;
            world.Objectives.Add(a);
            world.Objectives.Add(b);

            var selected = ObjectiveSpotlightSelector.Select(world, city, ar);

            Assert.IsNotNull(selected);
            Assert.AreEqual("almost", selected.ObjectiveId);
        }
    }
}
