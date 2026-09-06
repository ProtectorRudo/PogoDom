using NUnit.Framework;
using PogoDom.Session;

namespace PogoDom.Tests.Session
{
    public sealed class ViralCameraFramingPolicyTests
    {
        [Test]
        public void PortraitNeedsMoreDistanceThanLandscapeForSameBoard()
        {
            var portrait = ViralCameraFramingPolicy.Fit(8.4f, 8.4f, 9f / 16f);
            var landscape = ViralCameraFramingPolicy.Fit(8.4f, 8.4f, 16f / 9f);

            Assert.Greater(portrait.Distance, landscape.Distance);
            Assert.Less(portrait.SafeHeightFraction, landscape.SafeHeightFraction);
        }

        [Test]
        public void BiggerArenaCanNeverRequestCloserCamera()
        {
            var small = ViralCameraFramingPolicy.Fit(8f, 8f, 9f / 16f);
            var large = ViralCameraFramingPolicy.Fit(12f, 12f, 9f / 16f);

            Assert.Greater(large.Distance, small.Distance);
        }

        [Test]
        public void MoreAvatarHeightCanNeverRequestCloserCamera()
        {
            var shortActors = ViralCameraFramingPolicy.Fit(8.4f, 8.4f, 9f / 16f, visualHeight: 1.2f);
            var tallActors = ViralCameraFramingPolicy.Fit(8.4f, 8.4f, 9f / 16f, visualHeight: 3.2f);

            Assert.Greater(tallActors.Distance, shortActors.Distance);
        }

        [TestCase(0f, 8f, 1f)]
        [TestCase(8f, 0f, 1f)]
        [TestCase(8f, 8f, 0f)]
        public void InvalidGeometryIsRejected(float width, float depth, float aspect)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ViralCameraFramingPolicy.Fit(width, depth, aspect));
        }
    }
}
