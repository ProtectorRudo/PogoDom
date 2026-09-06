using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarExpressionPolicyTests
    {
        [Test]
        public void RoutineTileNoiseDoesNotSpamFaces()
        {
            AvatarExpressionCue cue;
            Assert.IsFalse(AvatarExpressionPolicy.TryDescribe(new MatchEvent(MatchEventType.TilePainted, 0), out cue));
            Assert.IsFalse(AvatarExpressionPolicy.TryDescribe(new MatchEvent(MatchEventType.TileStolen, 0, secondaryPlayerId: 1), out cue));
            Assert.IsFalse(AvatarExpressionPolicy.TryDescribe(new MatchEvent(MatchEventType.PlayerMoved, 0), out cue));
        }

        [Test]
        public void LargeBankGetsProudReactionAndSmallBankStaysExcited()
        {
            AvatarExpressionCue small;
            AvatarExpressionCue large;
            Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(new MatchEvent(MatchEventType.Banked, 0, value: 4), out small));
            Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(new MatchEvent(MatchEventType.Banked, 0, value: 12), out large));
            Assert.AreEqual(AvatarExpressionKind.Excited, small.Kind);
            Assert.AreEqual(AvatarExpressionKind.Proud, large.Kind);
            Assert.Greater(large.Strength, small.Strength);
        }

        [Test]
        public void MissileReactionBelongsToAttackerButStunBelongsToVictim()
        {
            AvatarExpressionCue fire;
            AvatarExpressionCue hit;
            Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(
                new MatchEvent(MatchEventType.MissileFired, playerId: 1, itemKind: PowerUpKind.Missile, secondaryPlayerId: 3), out fire));
            Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(
                new MatchEvent(MatchEventType.PlayerStunned, playerId: 3, itemKind: PowerUpKind.Missile, secondaryPlayerId: 1), out hit));

            Assert.AreEqual(1, fire.PlayerId);
            Assert.AreEqual(AvatarExpressionKind.Attack, fire.Kind);
            Assert.AreEqual(3, hit.PlayerId);
            Assert.AreEqual(AvatarExpressionKind.Hurt, hit.Kind);
        }

        [Test]
        public void ProtectedTileReactionBelongsToDefenderNotThief()
        {
            AvatarExpressionCue cue;
            Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(
                new MatchEvent(MatchEventType.TileProtected, playerId: 0, itemKind: PowerUpKind.Padlock, secondaryPlayerId: 2), out cue));
            Assert.AreEqual(2, cue.PlayerId);
            Assert.AreEqual(AvatarExpressionKind.Shielded, cue.Kind);
        }

        [Test]
        public void AllExpressionCuesAreShortAndNonBlockingByConstruction()
        {
            var events = new[]
            {
                new MatchEvent(MatchEventType.Banked, 0, value: 15),
                new MatchEvent(MatchEventType.CrateOpened, 0, itemKind: PowerUpKind.Speed),
                new MatchEvent(MatchEventType.MissileFired, 0, secondaryPlayerId: 1),
                new MatchEvent(MatchEventType.PlayerStunned, 1),
                new MatchEvent(MatchEventType.PadlockActivated, 0),
                new MatchEvent(MatchEventType.EnclosureCaptured, 0, value: 20)
            };

            for (var i = 0; i < events.Length; i++)
            {
                AvatarExpressionCue cue;
                Assert.IsTrue(AvatarExpressionPolicy.TryDescribe(events[i], out cue));
                Assert.That(cue.DurationSeconds, Is.GreaterThan(0f).And.LessThanOrEqualTo(0.85f));
                Assert.That(cue.Strength, Is.InRange(0f, 1f));
            }
        }
    }
}
