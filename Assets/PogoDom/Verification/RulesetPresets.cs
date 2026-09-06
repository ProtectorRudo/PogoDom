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
            config.TntInitialDelayTicks = 20;      // first warning at 10 s
            config.TntSpawnIntervalTicks = 30;    // at most every 15 s
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
            config.PadlockDurationTicks = 10;      // 5 s instead of 8 s
            config.PadlockRespawnDelayTicks = 24; // 12 s instead of 9 s
            return config;
        }

        private static MatchConfig BaseM02()
        {
            return new MatchConfig
            {
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
