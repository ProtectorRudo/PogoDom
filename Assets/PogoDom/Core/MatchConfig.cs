namespace PogoDom.Core
{
    public sealed class MatchConfig
    {
        public int BoardWidth { get; set; } = 8;
        public int BoardHeight { get; set; } = 8;
        public float TickSeconds { get; set; } = 0.5f;
        public float MatchSeconds { get; set; } = 90f;

        public int TargetBankCrates { get; set; } = 3;
        public int TargetArrows { get; set; } = 3;
        public int MinimumBankCrateChebyshevDistance { get; set; } = 2;
        public int ArrowRotationIntervalTicks { get; set; } = 2;
        public int BankThresholdForBots { get; set; } = 4;
    }
}
