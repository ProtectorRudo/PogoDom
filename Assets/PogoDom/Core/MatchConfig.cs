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

        public bool EnableArenaChaos { get; set; } = false;
        public int TntInitialDelayTicks { get; set; } = 12;
        public int TntSpawnIntervalTicks { get; set; } = 16;
        public int TntFuseTicks { get; set; } = 4;
        public int TntBlastRadius { get; set; } = 1;
        public int TntStunTicks { get; set; } = 1;
        public int MaxActiveTnt { get; set; } = 1;

        // Padlock is automatic on pickup. It protects the player's current unbanked
        // territory from rival paint/arrow/area capture without adding a second button.
        public bool EnablePadlockPower { get; set; } = false;
        public int TargetPadlocks { get; set; } = 1;
        public int PadlockDurationTicks { get; set; } = 16;        // 8 s
        public int PadlockRespawnDelayTicks { get; set; } = 18;   // 9 s

        public int BankRespawnDelayTicks { get; set; } = 6;
        public int ArrowRespawnDelayTicks { get; set; } = 12;
        public int SpeedRespawnDelayTicks { get; set; } = 14;
        public int MissileRespawnDelayTicks { get; set; } = 16;

        public int SpeedDurationTicks { get; set; } = 16;
        public int MissileStunTicks { get; set; } = 4;
    }
}
