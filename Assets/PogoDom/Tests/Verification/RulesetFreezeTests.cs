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
        public void ExperimentalV1RulesChangeOnlyTheirNamedFeatureFromLaunch()
        {
            var launch = RulesetPresets.LaunchM02();
            AssertOnlyToggleDiffers(launch, RulesetPresets.LoopV1(), nameof(MatchConfig.EnableEnclosureCapture));
            AssertOnlyToggleDiffers(launch, RulesetPresets.ChaosV1(), nameof(MatchConfig.EnableArenaChaos));
            AssertOnlyToggleDiffers(launch, RulesetPresets.PadlockV1(), nameof(MatchConfig.EnablePadlockPower));
        }

        [Test]
        public void V2CandidatesHaveExplicitNonV1Tuning()
        {
            Assert.AreEqual(EnclosureCapturePolicy.NeutralOnly, RulesetPresets.LoopV2().EnclosureCapturePolicy);
            Assert.AreEqual(20, RulesetPresets.ChaosV2().TntInitialDelayTicks);
            Assert.AreEqual(30, RulesetPresets.ChaosV2().TntSpawnIntervalTicks);
            Assert.AreEqual(10, RulesetPresets.PadlockV2().PadlockDurationTicks);
            Assert.AreEqual(24, RulesetPresets.PadlockV2().PadlockRespawnDelayTicks);
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
