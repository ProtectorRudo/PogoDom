using System.Collections.Generic;
using NUnit.Framework;
using PogoDom.Core;

namespace PogoDom.Tests
{
    public sealed class PlaytestSeedSequenceTests
    {
        [Test]
        public void MatchZeroUsesExactInspectorSeed()
        {
            Assert.AreEqual(20260905u, PlaytestSeedSequence.SeedFor(20260905u, 0));
        }

        [Test]
        public void SequenceIsDeterministicAndDoesNotRepeatAcrossFirstTenThousandMatches()
        {
            var seen = new HashSet<uint>();
            for (var i = 0; i < 10000; i++)
            {
                var a = PlaytestSeedSequence.SeedFor(20260905u, i);
                var b = PlaytestSeedSequence.SeedFor(20260905u, i);
                Assert.AreEqual(a, b);
                Assert.IsTrue(seen.Add(a), "Repeated seed at match index " + i);
            }
        }

        [Test]
        public void SameSeedCanBeReusedForRulesetABComparison()
        {
            var seed = PlaytestSeedSequence.SeedFor(77u, 12);
            Assert.AreEqual(seed, PlaytestSeedSequence.SeedFor(77u, 12));
            Assert.AreNotEqual(seed, PlaytestSeedSequence.SeedFor(77u, 13));
        }
    }
}
