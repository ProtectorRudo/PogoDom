using PogoDom.Core;

namespace PogoDom.Verification
{
    public static class RulesetPresets
    {
        public static MatchConfig LaunchM02() => BaseM02();

        public static MatchConfig LoopV1()
        {
            var config = BaseM02();
            config.EnableEnclosureCapture = true;
            config.EnclosureCapturePolicy = EnclosureCapturePolicy.AllUnprotected;
            return config;
        }

        public static MatchConfig LoopV2()
        {
            var config = BaseM02();
            config.EnableEnclosureCapture = true;
            config.EnclosureCapturePolicy = EnclosureCapturePolicy.NeutralOnly;
            return config;
        }

        public static MatchConfig ChaosV1()
        {
            var config = BaseM02();
            config.EnableArenaChaos = true;
            return config;
        }

        public static MatchConfig ChaosV2()
        {
            var config = BaseM02();
            config.EnableArenaChaos = true;
            config.TntInitialDelayTicks = 20;
            config.TntSpawnIntervalTicks = 30;
            return config;
        }

        public static MatchConfig PadlockV1()
        {
            var config = BaseM02();
            config.EnablePadlockPower = true;
            return config;
        }

        public static MatchConfig PadlockV2()
        {
            var config = BaseM02();
            config.EnablePadlockPower = true;
            config.PadlockDurationTicks = 10;
            config.PadlockRespawnDelayTicks = 24;
            return config;
        }

        public static MatchConfig CratesV1()
        {
            var config = BaseM02();
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

        public static MatchConfig CratesV2()
        {
            var config = CratesV1();
            // Same power economy as V1. Only the shared spawn behavior changes.
            config.BehaviorVersion = RulesetBehaviorVersion.V2;
            return config;
        }

        public static MatchConfig RivalsV1()
        {
            var config = BaseM02();
            // Same battle economy as launch. Only Medium-bot decision semantics
            // change so we can A/B opponent entertainment without confounds.
            config.BehaviorVersion = RulesetBehaviorVersion.V3;
            return config;
        }

        private static MatchConfig BaseM02()
        {
            return new MatchConfig
            {
                BehaviorVersion = RulesetBehaviorVersion.V1,
                BoardWidth = 8,
                BoardHeight = 8,
                TickSeconds = 0.5f,
                MatchSeconds = 75f,
                TargetBankCrates = 3,
                TargetArrows = 1,
                TargetSpeedPickups = 1,
                TargetMissiles = 1,
                MinimumBankCrateChebyshevDistance = 2,
                ArrowRotationIntervalTicks = 2,
                BankThresholdForBots = 5,
                EnableMysteryCrates = false,
                TargetMysteryCrates = 2,
                MysteryCrateRespawnDelayTicks = 12,
                MysteryCrateTableId = MysteryCrateTableId.PowerMixV1,
                MysteryCrateMinimumPlayerManhattanDistance = 2,
                MysteryCrateMinimumCrateChebyshevDistance = 2,
                MysteryCrateMaximumClosestPlayerDistanceGap = 1,
                EnableEnclosureCapture = false,
                EnclosureCapturePolicy = EnclosureCapturePolicy.AllUnprotected,
                EnableArenaChaos = false,
                TntInitialDelayTicks = 12,
                TntSpawnIntervalTicks = 16,
                TntFuseTicks = 4,
                TntBlastRadius = 1,
                TntStunTicks = 1,
                MaxActiveTnt = 1,
                EnablePadlockPower = false,
                TargetPadlocks = 1,
                PadlockDurationTicks = 16,
                PadlockRespawnDelayTicks = 18,
                BankRespawnDelayTicks = 6,
                ArrowRespawnDelayTicks = 12,
                SpeedRespawnDelayTicks = 14,
                MissileRespawnDelayTicks = 16,
                SpeedDurationTicks = 16,
                MissileStunTicks = 4
            };
        }
    }
}
