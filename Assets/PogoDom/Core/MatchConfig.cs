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

        public bool EnableEnclosureCapture { get; set; } = false;

        // Experimental neutral chaos. TNT is telegraphed before it clears only UNBANKED
        // territory; confirmed score can never be lost. A caught player misses one bounce.
        public bool EnableArenaChaos { get; set; } = false;
        public int TntInitialDelayTicks { get; set; } = 12;       // 6 s before first warning
        public int TntSpawnIntervalTicks { get; set; } = 16;      // at most one new warning every 8 s
        public int TntFuseTicks { get; set; } = 4;                // 2 s visible warning
        public int TntBlastRadius { get; set; } = 1;              // 3x3 footprint
        public int TntStunTicks { get; set; } = 1;                // exactly one missed bounce
        public int MaxActiveTnt { get; set; } = 1;

        public int BankRespawnDelayTicks { get; set; } = 6;      // 3 s
        public int ArrowRespawnDelayTicks { get; set; } = 12;    // 6 s
        public int SpeedRespawnDelayTicks { get; set; } = 14;    // 7 s
        public int MissileRespawnDelayTicks { get; set; } = 16;  // 8 s

        public int SpeedDurationTicks { get; set; } = 16;
        public int MissileStunTicks { get; set; } = 4;
    }
}
