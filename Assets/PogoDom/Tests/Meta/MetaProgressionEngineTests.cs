using NUnit.Framework;
using PogoDom.Meta;

namespace PogoDom.Tests.Meta
{
    public sealed class MetaProgressionEngineTests
    {
        private static readonly NationId Argentina = new NationId("argentina");
        private static readonly CityId LaPlata = new CityId("la-plata");
        private static readonly CityId Berisso = new CityId("berisso");

        [Test]
        public void HumanVictoryAddsExactlyOneCityAndNationPoint()
        {
            var world = World();
            var result = Result("m1", true, true, true);

            var receipt = new MetaProgressionEngine().Apply(world, result, CampaignContext.None(LaPlata));

            Assert.IsTrue(receipt.Applied);
            Assert.AreEqual(1, world.City(LaPlata).GlobalPoints);
            Assert.AreEqual(1, world.Nation(Argentina).GlobalPoints);
            Assert.AreEqual(1, receipt.CityPointDelta);
            Assert.AreEqual(1, receipt.NationPointDelta);
        }

        [Test]
        public void SameMatchCannotContributeTwice()
        {
            var world = World();
            var result = Result("same", true, true, true);
            var engine = new MetaProgressionEngine();

            Assert.IsTrue(engine.Apply(world, result, CampaignContext.None(LaPlata)).Applied);
            var duplicate = engine.Apply(world, result, CampaignContext.None(LaPlata));

            Assert.IsFalse(duplicate.Applied);
            Assert.AreEqual(ProgressionRejection.DuplicateMatch, duplicate.Rejection);
            Assert.AreEqual(1, world.City(LaPlata).GlobalPoints);
        }

        [Test]
        public void BotResultNeverChangesPersistentWorld()
        {
            var world = World();
            var bot = Result("bot", false, true, true);

            var receipt = new MetaProgressionEngine().Apply(world, bot, CampaignContext.Attack(LaPlata, Berisso));

            Assert.IsFalse(receipt.Applied);
            Assert.AreEqual(ProgressionRejection.BotResult, receipt.Rejection);
            Assert.AreEqual(0, world.City(LaPlata).GlobalPoints);
            Assert.AreEqual(world.City(Berisso).MaxHp, world.City(Berisso).CurrentHp);
        }

        [Test]
        public void LossCanProgressActivityObjectivesButAddsNoGlobalPoint()
        {
            var world = World();
            world.Objectives.Add(new ObjectiveState("steal", ObjectiveScope.City, LaPlata.Value, ObjectiveMetric.TilesStolen, 10));
            var loss = new BattleResult("loss", true, true, LaPlata, Argentina, false, 2, 7, 4, 6, 7, 1, 0, 0, 0);

            var receipt = new MetaProgressionEngine().Apply(world, loss, CampaignContext.None(LaPlata));

            Assert.IsTrue(receipt.Applied);
            Assert.AreEqual(0, world.City(LaPlata).GlobalPoints);
            Assert.AreEqual(6, world.Objectives[0].Current);
        }

        [Test]
        public void AttackVictoryDealsOneDamageAndConquersAtZero()
        {
            var world = World();
            var target = world.City(Berisso);
            target.CurrentHp = 1;

            var receipt = new MetaProgressionEngine().Apply(world, Result("attack", true, true, true), CampaignContext.Attack(LaPlata, Berisso));

            Assert.AreEqual(0, target.CurrentHp);
            Assert.AreEqual(-1, receipt.TargetHpDelta);
            Assert.IsTrue(receipt.ConquestTriggered);
            Assert.AreEqual(LaPlata, target.ConqueredByCity.Value);
        }

        [Test]
        public void DefenseVictoryHealsTwoWithoutExceedingMax()
        {
            var world = World();
            var city = world.City(LaPlata);
            city.CurrentHp = city.MaxHp - 1;

            var receipt = new MetaProgressionEngine().Apply(world, Result("def", true, true, true), CampaignContext.Defense(LaPlata));

            Assert.AreEqual(city.MaxHp, city.CurrentHp);
            Assert.AreEqual(1, receipt.TargetHpDelta);
        }

        [Test]
        public void InvalidMatchDoesNotBurnIdempotencyKey()
        {
            var world = World();
            var engine = new MetaProgressionEngine();
            var invalid = Result("retryable", true, false, true);
            var valid = Result("retryable", true, true, true);

            Assert.AreEqual(ProgressionRejection.InvalidMatch, engine.Apply(world, invalid, CampaignContext.None(LaPlata)).Rejection);
            Assert.IsTrue(engine.Apply(world, valid, CampaignContext.None(LaPlata)).Applied);
        }

        private static WorldState World()
        {
            var world = new WorldState();
            world.AddNation(new NationState(Argentina));
            world.AddCity(new CityState(LaPlata, Argentina, 800_000));
            world.AddCity(new CityState(Berisso, Argentina, 100_000));
            return world;
        }

        private static BattleResult Result(string id, bool human, bool valid, bool won)
        {
            return new BattleResult(id, human, valid, LaPlata, Argentina, won, won ? 1 : 2, 12, 5, 4, 12, 2, 1, 1, 1);
        }
    }
}
