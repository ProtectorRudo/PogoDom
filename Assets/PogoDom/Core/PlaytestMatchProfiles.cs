using System;

namespace PogoDom.Core
{
    public enum PlaytestRulesetMode
    {
        Base = 0,
        LoopV2 = 1,
        PadlockV2 = 2,
        ChaosV2 = 3,
        CratesV1 = 4,
        CratesV2 = 5
    }

    /// <summary>
    /// Client-side construction of the exact candidates selected for the first
    /// Unity feel test. Verification tests pin these profiles to the immutable
    /// server rulesets, so the prototype cannot silently drift from replay rules.
    /// </summary>
    public static class PlaytestMatchProfiles
    {
        public static MatchConfig Create(PlaytestRulesetMode mode)
        {
            var config = new MatchConfig();
            switch (mode)
            {
                case PlaytestRulesetMode.Base:
                    return config;

                case PlaytestRulesetMode.LoopV2:
                    config.EnableEnclosureCapture = true;
                    config.EnclosureCapturePolicy = EnclosureCapturePolicy.NeutralOnly;
                    return config;

                case PlaytestRulesetMode.PadlockV2:
                    config.EnablePadlockPower = true;
                    config.PadlockDurationTicks = 10;
                    config.PadlockRespawnDelayTicks = 24;
                    return config;

                case PlaytestRulesetMode.ChaosV2:
                    config.EnableArenaChaos = true;
                    config.TntInitialDelayTicks = 20;
                    config.TntSpawnIntervalTicks = 30;
                    return config;

                case PlaytestRulesetMode.CratesV1:
                    return ConfigureCrates(config, RulesetBehaviorVersion.V1);

                case PlaytestRulesetMode.CratesV2:
                    return ConfigureCrates(config, RulesetBehaviorVersion.V2);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown playtest ruleset.");
            }
        }

        private static MatchConfig ConfigureCrates(MatchConfig config, RulesetBehaviorVersion behaviorVersion)
        {
            config.BehaviorVersion = behaviorVersion;
            config.TargetArrows = 0;
            config.TargetSpeedPickups = 0;
            config.TargetMissiles = 0;
            config.EnablePadlockPower = true;
            config.TargetPadlocks = 0;
            config.EnableMysteryCrates = true;
            config.TargetMysteryCrates = 2;
            config.MysteryCrateRespawnDelayTicks = 12;
            config.MysteryCrateTableId = MysteryCrateTableId.PowerMixV1;
            return config;
        }
    }
}
