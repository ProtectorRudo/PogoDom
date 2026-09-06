using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Meta;

namespace PogoDom.Tests.Meta
{
    public sealed class BattleSessionRecorderTests
    {
        [Test]
        public void RecorderCountsOnlyLocalHumanEventsAndBuildsPlacement()
        {
            var config = new MatchConfig
            {
                TargetBankCrates = 0,
                TargetArrows = 0,
                TargetSpeedPickups = 0,
                TargetMissiles = 0
            };
            var state = MatchFactory.CreateClassicPrototype(config);
            var recorder = new BattleSessionRecorder("abc", 0, new CityId("la-plata"), new NationId("argentina"));
            var tick = new TickResult();
            tick.Events.Add(new MatchEvent(MatchEventType.TilePainted, 0, value: 1));
            tick.Events.Add(new MatchEvent(MatchEventType.TileStolen, 0, value: 1));
            tick.Events.Add(new MatchEvent(MatchEventType.Banked, 0, value: 7));
            tick.Events.Add(new MatchEvent(MatchEventType.MissileFired, 0));
            tick.Events.Add(new MatchEvent(MatchEventType.TilePainted, 1, value: 50));
            recorder.Observe(tick);

            state.PlayerById(0).Score = 20;
            state.PlayerById(1).Score = 10;
            var result = recorder.Complete(state);

            Assert.IsTrue(result.IsHuman);
            Assert.IsTrue(result.Won);
            Assert.AreEqual(1, result.Placement);
            Assert.AreEqual(1, result.TilesPainted);
            Assert.AreEqual(1, result.TilesStolen);
            Assert.AreEqual(7, result.BankedPoints);
            Assert.AreEqual(1, result.Banks);
            Assert.AreEqual(1, result.Missiles);
        }
    }
}
