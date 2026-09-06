using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Cosmetics;
using PogoDom.Core;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class PickupVisualIdentityPolicyTests
    {
        private static readonly PowerUpKind[] LaunchKinds =
        {
            PowerUpKind.BankCrate,
            PowerUpKind.MysteryCrate,
            PowerUpKind.Arrow,
            PowerUpKind.Speed,
            PowerUpKind.Missile,
            PowerUpKind.Padlock
        };

        [Test]
        public void EveryLaunchPickupHasUniqueSilhouetteArchetype()
        {
            var archetypes = new HashSet<PickupVisualArchetype>();
            for (var i = 0; i < LaunchKinds.Length; i++)
            {
                var identity = PickupVisualIdentityPolicy.Get(LaunchKinds[i]);
                Assert.AreEqual(LaunchKinds[i], identity.Kind);
                Assert.IsTrue(archetypes.Add(identity.Archetype), "Duplicate pickup silhouette: " + identity.Archetype);
            }
            Assert.AreEqual(LaunchKinds.Length, archetypes.Count);
        }

        [Test]
        public void MotionBudgetsStaySubtleEnoughToPreserveTileReadability()
        {
            for (var i = 0; i < LaunchKinds.Length; i++)
            {
                var identity = PickupVisualIdentityPolicy.Get(LaunchKinds[i]);
                Assert.That(identity.BobAmplitude, Is.InRange(0f, 0.08f));
                Assert.That(identity.SpinDegreesPerSecond, Is.InRange(0f, 100f));
                Assert.That(identity.PulseScale, Is.InRange(0f, 0.08f));
                Assert.That(identity.AccentPartBudget, Is.InRange(1, 4));
            }
        }

        [Test]
        public void DirectionArrowDoesNotRotateAwayFromGameplayDirection()
        {
            var arrow = PickupVisualIdentityPolicy.Get(PowerUpKind.Arrow);
            Assert.AreEqual(PickupVisualArchetype.DirectionArrow, arrow.Archetype);
            Assert.AreEqual(0f, arrow.SpinDegreesPerSecond);
        }

        [Test]
        public void MysteryAndSpeedHaveMostAnimatedIdentityWithoutExcess()
        {
            var bank = PickupVisualIdentityPolicy.Get(PowerUpKind.BankCrate);
            var mystery = PickupVisualIdentityPolicy.Get(PowerUpKind.MysteryCrate);
            var speed = PickupVisualIdentityPolicy.Get(PowerUpKind.Speed);

            Assert.Greater(mystery.SpinDegreesPerSecond, bank.SpinDegreesPerSecond);
            Assert.Greater(speed.SpinDegreesPerSecond, mystery.SpinDegreesPerSecond);
            Assert.Greater(speed.BobAmplitude, bank.BobAmplitude);
        }

        [Test]
        public void FutureUnsupportedPowersFailClosedInsteadOfBorrowingWrongIcon()
        {
            Assert.IsFalse(PickupVisualIdentityPolicy.IsLaunchPickup(PowerUpKind.Tnt));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => PickupVisualIdentityPolicy.Get(PowerUpKind.Tnt));
        }
    }
}
