using System.Reflection;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Verification;

namespace PogoDom.Tests.Verification
{
    public sealed class RivalRulesetFreezeTests
    {
        [Test]
        public void RivalsV1ChangesOnlyBehaviorVersionFromLaunch()
        {
            var launch = RulesetPresets.LaunchM02();
            var rivals = RulesetRegistry.CreateCurrent().Get("pogodom-rivals-v1").CreateConfig();
            var properties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var differences = 0;

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var before = property.GetValue(launch, null);
                var after = property.GetValue(rivals, null);
                if (Equals(before, after)) continue;

                differences++;
                Assert.AreEqual(nameof(MatchConfig.BehaviorVersion), property.Name, "Unexpected rival ruleset drift at " + property.Name);
                Assert.AreEqual(RulesetBehaviorVersion.V1, before);
                Assert.AreEqual(RulesetBehaviorVersion.V3, after);
            }

            Assert.AreEqual(1, differences);
        }

        [Test]
        public void UnityRivalPlaytestProfileMatchesFrozenRulesetExactly()
        {
            var expected = RulesetPresets.RivalsV1();
            var actual = PlaytestMatchProfiles.Create(PlaytestRulesetMode.RivalsV1);
            var properties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                Assert.AreEqual(
                    property.GetValue(expected, null),
                    property.GetValue(actual, null),
                    "Rival playtest profile drift at " + property.Name);
            }
        }

        [Test]
        public void ExistingPublishedRulesetsRemainOnTheirOriginalBehaviorVersions()
        {
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.LaunchM02().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.LoopV1().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.LoopV2().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.ChaosV1().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.ChaosV2().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.PadlockV1().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.PadlockV2().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V1, RulesetPresets.CratesV1().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V2, RulesetPresets.CratesV2().BehaviorVersion);
            Assert.AreEqual(RulesetBehaviorVersion.V3, RulesetPresets.RivalsV1().BehaviorVersion);
        }
    }
}
