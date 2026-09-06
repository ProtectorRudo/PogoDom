using System;
using System.Reflection;
using NUnit.Framework;
using PogoDom.Core;
using PogoDom.Verification;

namespace PogoDom.Tests.Verification
{
    public sealed class RulesetFreezeTests
    {
        [Test]
        public void EveryPublicMatchConfigPropertyIsSnapshottedByRulesetDefinition()
        {
            var configProperties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < configProperties.Length; i++)
            {
                var source = configProperties[i];
                var snapshot = typeof(RulesetDefinition).GetProperty(source.Name, BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(snapshot, "RulesetDefinition is missing MatchConfig property: " + source.Name);
                Assert.AreEqual(source.PropertyType, snapshot.PropertyType, "Ruleset property type mismatch: " + source.Name);
            }
        }

        [Test]
        public void LaunchM02IsExplicitlyFrozenToCertifiedValues()
        {
            var config = RulesetRegistry.CreateCurrent().Get("launch-m0-2").CreateConfig();
            Assert.AreEqual(RulesetBehaviorVersion.V1, config.BehaviorVersion);
            Assert.AreEqual(8, config.BoardWidth);
            Assert.AreEqual(8, config.BoardHeight);
            Assert.AreEqual(0.5f, config.TickSeconds);
            Assert.AreEqual(75f, config.MatchSeconds);
            Assert.AreEqual(3, config.TargetBankCrates);
            Assert.AreEqual(1, config.TargetArrows);
            Assert.AreEqual(1, config.TargetSpeedPickups);
            Assert.AreEqual(1, config.TargetMissiles);
            Assert.AreEqual(16, config.SpeedDurationTicks);
            Assert.AreEqual(4, config.MissileStunTicks);
            Assert.AreEqual(EnclosureCapturePolicy.AllUnprotected, config.EnclosureCapturePolicy);
            Assert.IsFalse(config.EnableMysteryCrates);
            Assert.IsFalse(config.EnableEnclosureCapture);
            Assert.IsFalse(config.EnableArenaChaos);
            Assert.IsFalse(config.EnablePadlockPower);
        }

        [Test]
        public void EveryPublishedLegacyRulesetPinsBehaviorV1()
        {
            var configs = new[]
            {
                RulesetPresets.LaunchM02(),
                RulesetPresets.LoopV1(),
                RulesetPresets.LoopV2(),
                RulesetPresets.ChaosV1(),
                RulesetPresets.ChaosV2(),
                RulesetPresets.PadlockV1(),
                RulesetPresets.PadlockV2(),
                RulesetPresets.CratesV1()
            };
            for (var i = 0; i < configs.Length; i++)
                Assert.AreEqual(RulesetBehaviorVersion.V1, configs[i].BehaviorVersion, "Published behavior changed at index " + i);
        }

        [Test]
        public void ExperimentalV1RulesChangeOnlyTheirNamedFeatureFromLaunch()
        {
            var launch = RulesetPresets.LaunchM02();
            AssertOnlyToggleDiffers(launch, RulesetPresets.LoopV1(), nameof(MatchConfig.EnableEnclosureCapture));
            AssertOnlyToggleDiffers(launch, RulesetPresets.ChaosV1(), nameof(MatchConfig.EnableArenaChaos));
            AssertOnlyToggleDiffers(launch, RulesetPresets.PadlockV1(), nameof(MatchConfig.EnablePadlockPower));
        }

        [Test]
        public void V2CandidatesHaveExplicitNonV1TuningOrBehavior()
        {
            Assert.AreEqual(EnclosureCapturePolicy.NeutralOnly, RulesetPresets.LoopV2().EnclosureCapturePolicy);
            Assert.AreEqual(20, RulesetPresets.ChaosV2().TntInitialDelayTicks);
            Assert.AreEqual(30, RulesetPresets.ChaosV2().TntSpawnIntervalTicks);
            Assert.AreEqual(10, RulesetPresets.PadlockV2().PadlockDurationTicks);
            Assert.AreEqual(24, RulesetPresets.PadlockV2().PadlockRespawnDelayTicks);
            Assert.AreEqual(RulesetBehaviorVersion.V2, RulesetPresets.CratesV2().BehaviorVersion);
        }

        [Test]
        public void CratesV1ReplacesLooseCombatPickupsWithoutChangingBankLoop()
        {
            var config = RulesetRegistry.CreateCurrent().Get("pogodom-crates-v1").CreateConfig();
            Assert.IsTrue(config.EnableMysteryCrates);
            Assert.AreEqual(2, config.TargetMysteryCrates);
            Assert.AreEqual(MysteryCrateTableId.PowerMixV1, config.MysteryCrateTableId);
            Assert.AreEqual(3, config.TargetBankCrates);
            Assert.AreEqual(0, config.TargetArrows);
            Assert.AreEqual(0, config.TargetSpeedPickups);
            Assert.AreEqual(0, config.TargetMissiles);
            Assert.AreEqual(0, config.TargetPadlocks);
        }

        [Test]
        public void CratesV2ChangesOnlySharedBehaviorVersionFromCratesV1()
        {
            var v1 = RulesetPresets.CratesV1();
            var v2 = RulesetRegistry.CreateCurrent().Get("pogodom-crates-v2").CreateConfig();
            var properties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var differences = 0;
            for (var i = 0; i < properties.Length; i++)
            {
                var p = properties[i];
                var a = p.GetValue(v1, null);
                var b = p.GetValue(v2, null);
                if (Equals(a, b)) continue;
                differences++;
                Assert.AreEqual(nameof(MatchConfig.BehaviorVersion), p.Name);
                Assert.AreEqual(RulesetBehaviorVersion.V1, a);
                Assert.AreEqual(RulesetBehaviorVersion.V2, b);
            }
            Assert.AreEqual(1, differences);
        }

        [Test]
        public void UnityPlaytestProfilesExactlyMatchFrozenCandidateRulesets()
        {
            AssertConfigsEqual(RulesetPresets.LaunchM02(), PlaytestMatchProfiles.Create(PlaytestRulesetMode.Base));
            AssertConfigsEqual(RulesetPresets.LoopV2(), PlaytestMatchProfiles.Create(PlaytestRulesetMode.LoopV2));
            AssertConfigsEqual(RulesetPresets.PadlockV2(), PlaytestMatchProfiles.Create(PlaytestRulesetMode.PadlockV2));
            AssertConfigsEqual(RulesetPresets.ChaosV2(), PlaytestMatchProfiles.Create(PlaytestRulesetMode.ChaosV2));
            AssertConfigsEqual(RulesetPresets.CratesV1(), PlaytestMatchProfiles.Create(PlaytestRulesetMode.CratesV1));
        }

        private static void AssertConfigsEqual(MatchConfig expected, MatchConfig actual)
        {
            var properties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < properties.Length; i++)
            {
                var p = properties[i];
                Assert.AreEqual(p.GetValue(expected, null), p.GetValue(actual, null), "Playtest profile drift at " + p.Name);
            }
        }

        private static void AssertOnlyToggleDiffers(MatchConfig baseline, MatchConfig candidate, string expectedProperty)
        {
            var properties = typeof(MatchConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var differences = 0;
            for (var i = 0; i < properties.Length; i++)
            {
                var p = properties[i];
                var a = p.GetValue(baseline, null);
                var b = p.GetValue(candidate, null);
                if (Equals(a, b)) continue;
                differences++;
                Assert.AreEqual(expectedProperty, p.Name, "Unexpected ruleset drift at " + p.Name);
                Assert.AreEqual(false, a);
                Assert.AreEqual(true, b);
            }
            Assert.AreEqual(1, differences);
        }
    }
}
