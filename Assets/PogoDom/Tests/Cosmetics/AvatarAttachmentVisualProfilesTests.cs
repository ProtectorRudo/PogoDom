using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarAttachmentVisualProfilesTests
    {
        [Test]
        public void LaunchProfilesHaveUniqueCompleteSignatures()
        {
            var profiles = AvatarAttachmentVisualProfiles.Launch;
            Assert.AreEqual(6, profiles.Count);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var signatures = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < profiles.Count; i++)
            {
                Assert.IsTrue(ids.Add(profiles[i].Id), "Duplicate id: " + profiles[i].Id);
                Assert.IsTrue(signatures.Add(profiles[i].Signature), "Duplicate visual signature: " + profiles[i].Signature);
            }
        }

        [Test]
        public void LaunchProfilesStayInsideMobileReadabilityBudgets()
        {
            var profiles = AvatarAttachmentVisualProfiles.Launch;
            for (var i = 0; i < profiles.Count; i++)
            {
                var p = profiles[i];
                Assert.That(p.HeadwearScale, Is.InRange(0.75f, 1.20f));
                Assert.That(p.BackScale, Is.InRange(0.75f, 1.20f));
                Assert.That(p.AuraRadius, Is.InRange(0.45f, 0.75f));
                Assert.That(p.TrailWidth, Is.InRange(0.08f, 0.16f));
                Assert.That(p.LandingRadius, Is.InRange(0.55f, 0.90f));
            }
        }

        [Test]
        public void PresentationProfileCannotContainCompetitiveStatProperties()
        {
            var forbidden = new[] { "speed", "jump", "score", "damage", "stun", "hitbox", "collision", "power", "cooldown" };
            var properties = typeof(AvatarAttachmentVisualProfile).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < properties.Length; i++)
            {
                var name = properties[i].Name.ToLowerInvariant();
                for (var j = 0; j < forbidden.Length; j++)
                    Assert.IsFalse(name.Contains(forbidden[j]), "Competitive-looking property leaked into cosmetic profile: " + properties[i].Name);
            }
        }

        [TestCase(-1, "captain")]
        [TestCase(0, "hero")]
        [TestCase(5, "captain")]
        [TestCase(6, "hero")]
        [TestCase(7, "trickster")]
        public void StyleLookupWrapsDeterministically(int styleIndex, string expectedId)
        {
            Assert.AreEqual(expectedId, AvatarAttachmentVisualProfiles.ForStyle(styleIndex).Id);
        }
    }
}
