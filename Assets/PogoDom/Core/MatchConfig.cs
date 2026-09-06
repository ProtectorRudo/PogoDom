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

        // 8 seconds at 0.5 s/tick. Speed grants one extra bounce per tick.
        public int SpeedDurationTicks { get; set; } = 16;

        // Start deliberately below the 3 s prototype reference to reduce frustration.
        // This is a tuning hypothesis, not a locked product value.
        public int MissileStunTicks { get; set; } = 4;
    }
}
