using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class VisualEventPolicyTests
    {
        [TestCase(MatchEventType.TileStolen, SkillVisualTrigger.TileSteal)]
        [TestCase(MatchEventType.TileProtected, SkillVisualTrigger.ShieldBlock)]
        [TestCase(MatchEventType.EnclosureCaptured, SkillVisualTrigger.AreaCapture)]
        [TestCase(MatchEventType.Banked, SkillVisualTrigger.Bank)]
        [TestCase(MatchEventType.CrateOpened, SkillVisualTrigger.CrateOpen)]
        [TestCase(MatchEventType.ArrowUsed, SkillVisualTrigger.Arrow)]
        [TestCase(MatchEventType.SpeedActivated, SkillVisualTrigger.Speed)]
        [TestCase(MatchEventType.MissileFired, SkillVisualTrigger.Missile)]
        [TestCase(MatchEventType.PadlockActivated, SkillVisualTrigger.Padlock)]
        [TestCase(MatchEventType.PlayerStunned, SkillVisualTrigger.Stun)]
        [TestCase(MatchEventType.HazardDetonated, SkillVisualTrigger.HazardBlast)]
        public void GameplayEventsMapToStableVisualTriggers(MatchEventType type, SkillVisualTrigger expected)
        {
            var e = new MatchEvent(type, playerId: 1, value: 5, itemKind: PowerUpKind.Missile);
            VisualEventCue cue;

            Assert.IsTrue(VisualEventPolicy.TryDescribe(e, out cue));
            Assert.AreEqual(expected, cue.Trigger);
            Assert.Greater(cue.LifetimeSeconds, 0f);
            Assert.Greater(cue.RadiusTiles, 0f);
            Assert.GreaterOrEqual(cue.ParticleBudget, 0);
            Assert.That(cue.CameraImpulse, Is.InRange(0f, 1f));
        }

        [Test]
        public void OrdinaryMovementDoesNotCreateSkillSpectacle()
        {
            var e = new MatchEvent(MatchEventType.PlayerMoved, playerId: 0);
            VisualEventCue cue;

            Assert.IsFalse(VisualEventPolicy.TryDescribe(e, out cue));
            Assert.IsNull(VisualEventPolicy.TriggerFor(e));
        }

        [Test]
        public void LargeBankGetsSpectacleTierWithoutChangingTrigger()
        {
            var small = new MatchEvent(MatchEventType.Banked, playerId: 0, value: 3);
            var large = new MatchEvent(MatchEventType.Banked, playerId: 0, value: 12);
            VisualEventCue smallCue;
            VisualEventCue largeCue;

            Assert.IsTrue(VisualEventPolicy.TryDescribe(small, out smallCue));
            Assert.IsTrue(VisualEventPolicy.TryDescribe(large, out largeCue));

            Assert.AreEqual(SkillVisualTrigger.Bank, smallCue.Trigger);
            Assert.AreEqual(SkillVisualTrigger.Bank, largeCue.Trigger);
            Assert.AreEqual(VisualImpactTier.Readable, smallCue.Impact);
            Assert.AreEqual(VisualImpactTier.Spectacle, largeCue.Impact);
            Assert.Greater(largeCue.ParticleBudget, smallCue.ParticleBudget);
            Assert.Greater(largeCue.CameraImpulse, smallCue.CameraImpulse);
        }

        [Test]
        public void CaptureAndBankBudgetsAreClampedForMobileSafety()
        {
            var hugeCapture = new MatchEvent(MatchEventType.EnclosureCaptured, playerId: 0, value: 10000);
            var hugeBank = new MatchEvent(MatchEventType.Banked, playerId: 0, value: 10000);
            VisualEventCue capture;
            VisualEventCue bank;

            VisualEventPolicy.TryDescribe(hugeCapture, out capture);
            VisualEventPolicy.TryDescribe(hugeBank, out bank);

            Assert.LessOrEqual(capture.ParticleBudget, 42);
            Assert.LessOrEqual(bank.ParticleBudget, 38);
            Assert.LessOrEqual(capture.CameraImpulse, 0.30f);
            Assert.LessOrEqual(bank.CameraImpulse, 0.24f);
            Assert.LessOrEqual(capture.RadiusTiles, 2.15f);
        }
    }
}
