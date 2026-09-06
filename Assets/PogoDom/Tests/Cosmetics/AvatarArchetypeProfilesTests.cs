using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarArchetypeProfilesTests
    {
        [Test]
        public void LaunchArchetypesHaveSixDistinctSilhouetteSignatures()
        {
            Assert.AreEqual(6, AvatarArchetypeProfiles.All.Count);
            var ids = new HashSet<string>();
            var signatures = new HashSet<string>();
            var families = new HashSet<AvatarSilhouetteFamily>();

            for (var i = 0; i < AvatarArchetypeProfiles.All.Count; i++)
            {
                var profile = AvatarArchetypeProfiles.All[i];
                Assert.IsTrue(ids.Add(profile.Id), "Duplicate avatar profile id: " + profile.Id);
                Assert.IsTrue(signatures.Add(profile.SilhouetteSignature), "Duplicate silhouette signature: " + profile.Id);
                Assert.IsTrue(families.Add(profile.Silhouette), "Duplicate silhouette family: " + profile.Id);
            }
        }

        [Test]
        public void MascotIsAllowedToBeNonHumanoidWhileStillSupportingCosmetics()
        {
            var mascot = AvatarArchetypeProfiles.Get("mascot");

            Assert.IsFalse(AvatarRigCapabilities.Supports(mascot.Capabilities, AvatarRigCapability.HumanoidRetargeting));
            Assert.IsTrue(AvatarRigCapabilities.Supports(mascot.Capabilities, AvatarRigCapability.HeadwearSocket));
            Assert.IsTrue(AvatarRigCapabilities.Supports(mascot.Capabilities, AvatarRigCapability.BackAccessorySocket));
            Assert.IsTrue(AvatarRigCapabilities.Supports(mascot.Capabilities, AvatarRigCapability.AuraSocket));
            Assert.IsTrue(AvatarRigCapabilities.Supports(mascot.Capabilities, AvatarRigCapability.FullBodyEmote));
        }

        [Test]
        public void EveryHumanoidLaunchArchetypeSupportsPogoHandleIkAndEmotes()
        {
            for (var i = 0; i < AvatarArchetypeProfiles.All.Count; i++)
            {
                var profile = AvatarArchetypeProfiles.All[i];
                if (profile.Silhouette == AvatarSilhouetteFamily.Mascot) continue;

                Assert.IsTrue(AvatarRigCapabilities.Supports(profile.Capabilities, AvatarRigCapability.PogoHandleIk), profile.Id);
                Assert.IsTrue(AvatarRigCapabilities.Supports(profile.Capabilities, AvatarRigCapability.FullBodyEmote), profile.Id);
            }
        }
    }
}
