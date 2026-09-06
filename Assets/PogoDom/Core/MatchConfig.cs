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

        // Experimental PogoDom evolution of the classic paint/bank loop. Closing a
        // boundary automatically paints the unreachable interior, but those tiles stay
        // unbanked until the player reaches a Bank Crate. No extra button is introduced.
        public bool EnableEnclosureCapture { get; set; } = false;

        // Initial items are present immediately. Replacement items have deliberate
        // scarcity so a pickup creates a hotspot instead of constant visual noise.
        public int BankRespawnDelayTicks { get; set; } = 6;      // 3 s
        public int ArrowRespawnDelayTicks { get; set; } = 12;    // 6 s
        public int SpeedRespawnDelayTicks { get; set; } = 14;    // 7 s
        public int MissileRespawnDelayTicks { get; set; } = 16;  // 8 s

        // 8 seconds at 0.5 s/tick. Speed grants one extra real bounce per tick.
        public int SpeedDurationTicks { get; set; } = 16;

        // Deliberately below the 3 s prototype reference to reduce frustration.
        public int MissileStunTicks { get; set; } = 4;
    }
}
