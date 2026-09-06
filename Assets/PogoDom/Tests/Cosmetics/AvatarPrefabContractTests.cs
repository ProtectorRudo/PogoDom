using System.Linq;
using NUnit.Framework;
using PogoDom.Cosmetics;

namespace PogoDom.Tests.Cosmetics
{
    public sealed class AvatarPrefabContractTests
    {
        [Test]
        public void StandardHumanoidRequiresAllCosmeticAndGripSockets()
        {
            var required = AvatarPrefabContract.RequiredSockets(AvatarRigCapabilities.StandardHumanoid);

            CollectionAssert.Contains(required, AvatarPrefabContract.PogoSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.SkillSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.TrailSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.LandingFxSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.EmoteRoot);
            CollectionAssert.Contains(required, AvatarPrefabContract.HeadwearSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.BackAccessorySocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.AuraSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.LeftGripTarget);
            CollectionAssert.Contains(required, AvatarPrefabContract.RightGripTarget);
            Assert.AreEqual(required.Count, required.Distinct().Count());
        }

        [Test]
        public void MascotDoesNotPretendToHavePogoHandIkTargets()
        {
            var required = AvatarPrefabContract.RequiredSockets(AvatarRigCapabilities.Mascot);

            CollectionAssert.DoesNotContain(required, AvatarPrefabContract.LeftGripTarget);
            CollectionAssert.DoesNotContain(required, AvatarPrefabContract.RightGripTarget);
            CollectionAssert.Contains(required, AvatarPrefabContract.AuraSocket);
            CollectionAssert.Contains(required, AvatarPrefabContract.EmoteRoot);
        }

        [Test]
        public void MissingSocketDetectionIsExactAndDeterministic()
        {
            var existing = new[]
            {
                AvatarPrefabContract.PogoSocket,
                AvatarPrefabContract.SkillSocket,
                AvatarPrefabContract.TrailSocket,
                AvatarPrefabContract.LandingFxSocket,
                AvatarPrefabContract.EmoteRoot,
                AvatarPrefabContract.HeadwearSocket,
                AvatarPrefabContract.BackAccessorySocket
            };

            var missingA = AvatarPrefabContract.MissingSockets(existing, AvatarRigCapabilities.StandardHumanoid);
            var missingB = AvatarPrefabContract.MissingSockets(existing.Reverse(), AvatarRigCapabilities.StandardHumanoid);

            CollectionAssert.AreEqual(missingA, missingB);
            CollectionAssert.Contains(missingA, AvatarPrefabContract.AuraSocket);
            CollectionAssert.Contains(missingA, AvatarPrefabContract.LeftGripTarget);
            CollectionAssert.Contains(missingA, AvatarPrefabContract.RightGripTarget);
        }
    }
}
