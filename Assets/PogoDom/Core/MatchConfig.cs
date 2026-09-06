namespace PogoDom.Core
{
    public sealed class MatchConfig
    {
        public int BoardWidth { get; set; } = 8;
        public int BoardHeight { get; set; } = 8;
        public float TickSeconds { get; set; } = 0.5f;
        public float MatchSeconds { get; set; } = 75f;

        public int TargetBankCrates { get; set; } = 3;
        public int TargetArrows { get; set; } = 1;
        public int TargetSpeedPickups { get; set; } = 1;
        public int TargetMissiles { get; set; } = 1;
        public int MinimumBankCrateChebyshevDistance { get; set; } = 2;
        public int ArrowRotationIntervalTicks { get; set; } = 2;
        public int BankThresholdForBots { get; set; } = 5;

        // Experimental crate shell: powers are pre-rolled at spawn, hidden until pickup.
        public bool EnableMysteryCrates { get; set; } = false;
        public int TargetMysteryCrates { get; set; } = 2;
        public int MysteryCrateRespawnDelayTicks { get; set; } = 12;
        public MysteryCrateTableId MysteryCrateTableId { get; set; } = MysteryCrateTableId.PowerMixV1;

        public bool EnableEnclosureCapture { get; set; } = false;
        public EnclosureCapturePolicy EnclosureCapturePolicy { get; set; } = EnclosureCapturePolicy.AllUnprotected;

        public bool EnableArenaChaos { get; set; } = false;
        public int TntInitialDelayTicks { get; set; } = 12;
        public int TntSpawnIntervalTicks { get; set; } = 16;
        public int TntFuseTicks { get; set; } = 4;
        public int TntBlastRadius { get; set; } = 1;
        public int TntStunTicks { get; set; } = 1;
        public int MaxActiveTnt { get; set; } = 1;

        public bool EnablePadlockPower { get; set; } = false;
        public int TargetPadlocks { get; set; } = 1;
        public int PadlockDurationTicks { get; set; } = 16;
        public int PadlockRespawnDelayTicks { get; set; } = 18;

        public int BankRespawnDelayTicks { get; set; } = 6;
        public int ArrowRespawnDelayTicks { get; set; } = 12;
        public int SpeedRespawnDelayTicks { get; set; } = 14;
        public int MissileRespawnDelayTicks { get; set; } = 16;

        public int SpeedDurationTicks { get; set; } = 16;
        public int MissileStunTicks { get; set; } = 4;
    }
}
