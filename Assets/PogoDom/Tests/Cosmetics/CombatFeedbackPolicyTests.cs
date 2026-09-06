using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class CombatFeedbackPolicyTests
    {
        [Test]
        public void MissileTracePointsFromAttackerToResolvedTarget()
        {
            var e = new MatchEvent(
                MatchEventType.MissileFired,
                playerId: 1,
                value: 3,
                itemKind: PowerUpKind.Missile,
                secondaryPlayerId: 3);

            CombatFeedbackCue cue;
            Assert.IsTrue(CombatFeedbackPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(CombatFeedbackKind.MissileTrace, cue.Kind);
            Assert.AreEqual(1, cue.SourcePlayerId);
            Assert.AreEqual(3, cue.TargetPlayerId);
            Assert.That(cue.LifetimeSeconds, Is.InRange(0.10f, 0.40f));
            Assert.Greater(cue.WidthScale, 0f);
        }

        [Test]
        public void StunImpactPreservesCoreAttackerTargetSemantics()
        {
            var e = new MatchEvent(
                MatchEventType.PlayerStunned,
                playerId: 2,
                value: 3,
                itemKind: PowerUpKind.Missile,
                secondaryPlayerId: 0);

            CombatFeedbackCue cue;
            Assert.IsTrue(CombatFeedbackPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(CombatFeedbackKind.StunImpact, cue.Kind);
            Assert.AreEqual(0, cue.SourcePlayerId);
            Assert.AreEqual(2, cue.TargetPlayerId);
            Assert.AreEqual(1.0f, cue.PulseStrength);
        }

        [Test]
        public void ShieldBlockHighlightsDefenderNotThief()
        {
            var e = new MatchEvent(
                MatchEventType.TileProtected,
                playerId: 3,
                value: 1,
                itemKind: PowerUpKind.Padlock,
                secondaryPlayerId: 1);

            CombatFeedbackCue cue;
            Assert.IsTrue(CombatFeedbackPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(CombatFeedbackKind.ShieldBlock, cue.Kind);
            Assert.AreEqual(3, cue.SourcePlayerId);
            Assert.AreEqual(1, cue.TargetPlayerId);
            Assert.Greater(cue.PulseStrength, 0.5f);
        }

        [Test]
        public void InvalidOrSelfTargetedCombatEventProducesNoCue()
        {
            CombatFeedbackCue cue;
            Assert.IsFalse(CombatFeedbackPolicy.TryDescribe(
                new MatchEvent(MatchEventType.MissileFired, playerId: 1, secondaryPlayerId: 1), out cue));
            Assert.IsFalse(CombatFeedbackPolicy.TryDescribe(
                new MatchEvent(MatchEventType.PlayerStunned, playerId: -1, secondaryPlayerId: 0), out cue));
            Assert.IsFalse(CombatFeedbackPolicy.TryDescribe(
                new MatchEvent(MatchEventType.PlayerMoved, playerId: 0), out cue));
        }

        [Test]
        public void ReadabilityPolicyNeverCreatesLongBlockingEffects()
        {
            var events = new[]
            {
                new MatchEvent(MatchEventType.MissileFired, playerId: 0, secondaryPlayerId: 2),
                new MatchEvent(MatchEventType.PlayerStunned, playerId: 2, secondaryPlayerId: 0),
                new MatchEvent(MatchEventType.TileProtected, playerId: 0, secondaryPlayerId: 1)
            };

            for (var i = 0; i < events.Length; i++)
            {
                CombatFeedbackCue cue;
                Assert.IsTrue(CombatFeedbackPolicy.TryDescribe(events[i], out cue));
                Assert.LessOrEqual(cue.LifetimeSeconds, 0.40f);
                Assert.That(cue.PulseStrength, Is.InRange(0f, 1f));
            }
        }
    }
}
