using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class PogoSilhouetteProfilesTests
    {
        [Test]
        public void LaunchPogoFamiliesAreSixAndVisuallyUnique()
        {
            Assert.AreEqual(6, PogoSilhouetteProfiles.All.Count);
            var signatures = new HashSet<string>();
            for (var i = 0; i < PogoSilhouetteProfiles.All.Count; i++)
            {
                var profile = PogoSilhouetteProfiles.All[i];
                Assert.IsTrue(signatures.Add(profile.Signature), "Duplicate pogo silhouette signature: " + profile.Signature);
            }
        }

        [Test]
        public void PogoPresentationProportionsStayInsideReadableBounds()
        {
            for (var i = 0; i < PogoSilhouetteProfiles.All.Count; i++)
            {
                var p = PogoSilhouetteProfiles.All[i];
                Assert.That(p.FootWidthScale, Is.InRange(0.80f, 1.35f));
                Assert.That(p.SpringWidthScale, Is.InRange(0.75f, 1.30f));
                Assert.That(p.HandleWidthScale, Is.InRange(0.85f, 1.30f));
                Assert.That(p.AccentPartBudget, Is.InRange(1, 5));
            }
        }

        [Test]
        public void PogoProfileCannotCarryCompetitiveStats()
        {
            var forbiddenFragments = new[] { "speed", "jumpheight", "duration", "score", "stun", "damage", "tick", "collision", "hitbox", "range" };
            var properties = typeof(PogoSilhouetteProfile).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (var i = 0; i < properties.Length; i++)
            {
                var name = properties[i].Name.ToLowerInvariant();
                for (var j = 0; j < forbiddenFragments.Length; j++)
                    Assert.IsFalse(name.Contains(forbiddenFragments[j]), "Competitive-looking property leaked into pogo cosmetic profile: " + properties[i].Name);
            }
        }

        [Test]
        public void FirstFourBattleSlotsUseFourDifferentPogos()
        {
            var used = new HashSet<PogoSilhouetteFamily>();
            for (var playerId = 0; playerId < 4; playerId++)
                Assert.IsTrue(used.Add(PogoSilhouetteProfiles.ForPlayerSlot(playerId)));
            Assert.AreEqual(4, used.Count);
        }
    }
}
